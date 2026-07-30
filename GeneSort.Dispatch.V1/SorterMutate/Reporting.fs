namespace GeneSort.Dispatch.V1.SorterMutate

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Model.Sorting.V1
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Db.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Dispatch.V1.CommonParams


module Reporting = 


    let makeMutantReport
            (mutantDetailsMaker: runParameters -> Async<Result<sorterEvalSelection * Map<Guid<sorterModelId>, Guid<sorterModelId>>, string>> )
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =


        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Mutant Report for Run %s" (MathUtils.getTimestampString()) %runId)
                let reportName = (sprintf "MutantReport" |> UMX.tag<textReportName>)

                let! (_sorterEvalSelection, (mutantIdToParentIdMap: Map<Guid<sorterModelId>,Guid<sorterModelId>>)) = mutantDetailsMaker rp

                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let parentRecordMap = _sorterEvalSelection |> EvalReporting.toDataTableRecords leadCols "Parent_"

                let tupes =
                    sorterSetEvals.SorterEvals
                    |> Array.choose (fun se -> 
                        let (sorterModelId : Guid<sorterModelId>) = se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>
        
                        match mutantIdToParentIdMap |> Map.tryFind sorterModelId with
                        | None -> None // Safely ignore if the parent mapping is missing
                        | Some parentSorterModelId ->
                            Some (parentSorterModelId, se)
                    ) |> Array.groupBy fst

                let yab = tupes |> Array.map(fun (parentSorterModelId, group) ->
                    (parentSorterModelId, group |> Array.map(snd)))

                let wab = yab |> Array.map(fun (parentSorterModelId, ses) ->
                            let evBinSet = sorterEvalBinSet.createFromSorterEvals
                                                (Guid.Empty |> UMX.tag<sorterEvalBinSetId>)
                                                (sorterSetEvals.SorterTestId)
                                                ses
                            (parentSorterModelId, evBinSet))


                let chubby = wab |> Array.choose(fun (parentSorterModelId, evBinSet) ->
                            let parentKey = %parentSorterModelId |> UMX.tag<sorterId>
                            match parentRecordMap |> Map.tryFind parentKey with
                            | None -> None // Safely ignore if the parent record detail is missing
                            | Some parentRecord ->
                                let childRecords = evBinSet |> SorterEvalBinSet.makeDataTableRecords
                                Some (dataTableRecord.combineWithMany childRecords parentRecord |> Array.ofSeq))
                            |> Array.concat


                let dtrs = dataTableRecord.combineWithMany chubby leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return (rp : runParameters).WithRunFinished(Some true)
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
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
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
                        rp.GetSorterEvalSelectionType()
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


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            mergeDimension
                                            mergeSuffixType
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

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let simpleSorterModels = parentSorterModelSet.SorterModels |> Array.map (SorterModel.asSimpleSorterModel)

            let sorterModelMutator = SimpleSorterModelMutator.getMsceModelMutator
                                            (rngType |> RngFactory.create)
                                            ExcludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate

            let parentMutantMap = 
                    SimpleSorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        simpleSorterModels
                                        %sorterChildCount

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
                        rp.GetSorterEvalSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"


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

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let simpleSorterModels = parentSorterModelSet.SorterModels |> Array.map (SorterModel.asSimpleSorterModel)

            let sorterModelMutator = SimpleSorterModelMutator.getMsceModelMutator
                                            (rngType |> RngFactory.create)
                                            ExcludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate

            let parentMutantMap = 
                    SimpleSorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        simpleSorterModels
                                        %sorterChildCount

            return (_sorterEvalSelection, parentMutantMap)
        }


