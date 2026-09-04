namespace GeneSort.Dispatch.V1.SorterMutate

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Db.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.SortingLib.Sorter

module Reporting = 

    let makeMutantReport
            (mutantDetailsMaker: runParameters -> Async<Result<sorterEvalSelection * Map<Guid<sorterModelId>, Guid<sorterModelId>>, string>>)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Mutant Report for Run %s" (StringUtils.getTimestampString()) %runId)
                let reportName = "MutantReport" |> UMX.tag<textReportName>

                // 1. Fetch selection and parent mapping
                let! (sorterEvalSelection, mutantIdToParentIdMap) = mutantDetailsMaker rp

                // 2. Load evaluation data
                let! qpSorterSetEval = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                // 3. Prepare parent record lookup
                let! qpReport = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                    |> Result.ofOption "Failed to create QueryParams for Report."

                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let parentRecordMap = sorterEvalSelection |> EvalReporting.toDataTableRecords leadCols "Parent_"

                // 4. Directly create bin set from parent evaluation set
                let binSet = sorterEvalBinSet.create (Guid.Empty |> UMX.tag<sorterEvalBinSetId>) sorterSetEvals

                // 5. Map evaluations to parent records and combine
                let childRecords = binSet |> SorterEvalBinSet.makeDataTableRecords

                let dtaTableRs = 
                    sorterSetEvals.SorterEvals
                    |> Array.choose (fun se ->
                        let sorterModelId = se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>
                        
                        match mutantIdToParentIdMap |> Map.tryFind sorterModelId with
                        | None -> None
                        | Some (parentModelId: Guid<sorterModelId>) ->
                            let parentKey = (%parentModelId) |> UMX.tag<sorterId>
                            match parentRecordMap |> Map.tryFind parentKey with
                            | None -> None
                            | Some parentRecord ->
                                // Combine individual child evaluations with their parent lead row
                                Some (dataTableRecord.combineWithMany childRecords parentRecord)
                    )
                    |> Seq.concat

                // 6. Build and save final text report
                let dtrs = dataTableRecord.combineWithMany dtaTableRs leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                do! checkCancellation cts.Token
                do! host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite

                log "MutantReport completed successfully."
                return rp.WithRunFinished(Some true)

            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)


    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =


        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (StringUtils.getTimestampString()) %runId)
    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "FullEvalReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = sorterSetEvals |> SorterSetEval.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (rp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)



    let makeMergeMutantDetails (rp:runParameters) : 
            Async<Result<
                        sorterEvalSelection * 
                        Map<Guid<sorterModelId>, Guid<sorterModelId>>, 
                        string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterEvalSelectionType) = 
                        rp.GetSeedPoolSorterEvalSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mergeDimension: int<mergeDimension>) = 
                        rp.GetMergeDimension() 
                        |> Result.ofOption "Missing mergeDimension in run parameters"

            let! (mergeSuffixType: mergeSuffixType) = 
                        rp.GetMergeSuffixType() 
                        |> Result.ofOption "Missing mergeSuffixType in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) = 
                        rp.GetExcludeSelfCe() 
                        |> Result.ofOption "Missing excludeSelfCe in run parameters"

            let! (slv: sorterLibVariant) = 
                        rp.GetSorterLibVariant()
                        |> Result.ofOption "Missing sorterLibVariant in run parameters"


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            mergeDimension
                                            mergeSuffixType
                                            slv
                                            sorterEvalType.V2

            let (_sorterEvalSelection: sorterEvalSelection) = 
                            SorterEvalSelection.makeSelection sem sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let simpleSorterModels = parentSorterModelSet.SorterModels |> Array.map (SorterModel.asSimpleSorterModel)

            let sorterModelMutator = SimpleSorterModelMutator.getMsceModelMutator
                                            (rngType |> RngFactory.create)
                                            excludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate

            let parentMutantMap = 
                    SimpleSorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        simpleSorterModels
                                        %sorterChildCount
                                        mutationMod

            return (_sorterEvalSelection, parentMutantMap)
        }



    let makeStandardMutantDetails (rp:runParameters) : 
            Async<Result<
                        sorterEvalSelection * 
                        Map<Guid<sorterModelId>, Guid<sorterModelId>>, 
                        string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterEvalSelectionType) = 
                        rp.GetSeedPoolSorterEvalSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) = 
                        rp.GetExcludeSelfCe() 
                        |> Result.ofOption "Missing excludeSelfCe in run parameters"

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterEvalSelection.makeSelection sem sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let simpleSorterModels = parentSorterModelSet.SorterModels |> Array.map (SorterModel.asSimpleSorterModel)

            let sorterModelMutator = SimpleSorterModelMutator.getMsceModelMutator
                                            (rngType |> RngFactory.create)
                                            excludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate

            let parentMutantMap = 
                    SimpleSorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        simpleSorterModels
                                        %sorterChildCount
                                        mutationMod

            return (_sorterEvalSelection, parentMutantMap)
        }


