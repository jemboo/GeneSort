namespace GeneSort.Project.V1.Catalogs.Reports.SorterMutate

open System
open System.Threading
open FSharp.UMX
open FsToolkit.ErrorHandling
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Runtime.OpsUtils

module Reporting = 

    let makeMutantReport
            (mutantDetailsMaker: runParameters -> Async<Result<sorterSelection * Map<Guid<sorterModelId>, Guid<sorterModelId>>, string>>)
            (configured: run) (storage: runStorage)
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
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let! outB = storage.RunDb.loadAsync geneSortDataMode.Results qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                // 3. Prepare parent record lookup
                let! qpReport = 
                    storage.QueryParamsFromRunParams rp (geneSortDataType.TextReport reportName)
                    |> Result.ofOption "Failed to create QueryParams for Report."

                let leadCols = qpReport |> QueryParams.makeReportDataTableRecord configured.RunName
                let parentRecordMap = sorterEvalSelection |> EvalReporting.toDataTableRecords leadCols "Parent_"

                // 4. Directly create bin set from parent evaluation set
                let binSet = sorterEvalBinSet_Standard.create (Guid.Empty |> UMX.tag<sorterEvalBinSetId>) sorterSetEvals

                // 5. Map evaluations to parent records and combine
                let childRecords = binSet |> SorterEvalBinSet_Standard.makeDataTableRecords

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
                do! storage.RunDb.saveAsync geneSortDataMode.Results qpReport (report |> outputData.TextReport) allowOverwrite

                log "MutantReport completed successfully."
                return rp.WithRunFinished(Some true)

            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)


    let makeFullReport 
            (configured: run) (storage: runStorage)
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
    
                let! qpSorterSetEval = storage.QueryParamsFromRunParams rp (geneSortDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = storage.RunDb.loadAsync geneSortDataMode.Results qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "FullEvalReport" |> UMX.tag<textReportName>)

                let! qpReport = storage.QueryParamsFromRunParams rp (geneSortDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeReportDataTableRecord configured.RunName
                let details = sorterSetEvals |> SorterSetEval.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = storage.RunDb.saveAsync geneSortDataMode.Results qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (rp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)





    let makeStandardMutantDetails (sources: evolutionSetupSources) (rp:runParameters) :
            Async<Result<
                        sorterSelection * 
                        Map<Guid<sorterModelId>, Guid<sorterModelId>>, 
                        string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (channelCount: int<channelCount>) =
                        rp.GetChannelCount()
                        |> Result.ofOption "Missing channel count in run parameters"
    
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

            let! (sest: sorterSelectionType) = 
                        rp.GetSeedSorterPoolSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutVariant:mutatorVariant) = 
                        rp.GetMutatorVariant ()
                        |> Result.ofOption "Missing mutatorVariant in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) = 
                        rp.GetExcludeSelfCe() 
                        |> Result.ofOption "Missing excludeSelfCe in run parameters"

            let! (parentSorterSetEval: sorterSetEval) =
                        sources.LoadStandardSorterEvals
                                            channelCount
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection sem sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let! (parentSorterModelGen: sorterModelGen) =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                                        rngType 
                                        channelCount
                                        simpleSorterModelType
                                        excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen


            let baseMutator =
                match rp.GetMutatorParams() with
                | Some configured -> MutatorParams.toModelMutator modificationRate configured
                | None ->
                    SimpleSorterModelMutator.getMsceModelMutator (RngFactory.create rngType)
                        excludeSelfCe modificationRate mutationRate insertionRate deletionRate
                        (rp.GetMutatorVariant() |> Option.defaultValue mutatorVariant.V1)
                    |> sorterModelMutator.Simple
            let! sorterModelMutator = MutatorMakers.applyParameters rp baseMutator

            let parentMutantMap = 
                    SorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        parentSorterModelSet.SorterModels
                                        %sorterChildCount
                                        mutationMod

            return (_sorterEvalSelection, parentMutantMap)
        }




    let makeMergeMutantDetails (sources: evolutionSetupSources) (rp:runParameters) :
            Async<Result<
                        sorterSelection * 
                        Map<Guid<sorterModelId>, Guid<sorterModelId>>, 
                        string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! repl =
                   rp.GetRepl()
                   |> Result.ofOption "Missing Repl"

            let! (mrgLibId: mergeLibId) = 
                        rp.GetMergeLibId() 
                        |> Result.ofOption "Missing merge library ID in run parameters"
    
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

            let! (sest: sorterSelectionType) = 
                        rp.GetSeedSorterPoolSelectionType()
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
                        sources.LoadMergeSorterEvals
                                            mrgLibId
                                            repl
                                            simpleSorterModelType

            let (_sorterEvalSelection: sorterSelection) = 
                            SorterSelection.makeSelection sem sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let! (parentSorterModelGen: sorterModelGen) =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                                        rngType 
                                        mrgLibId.ChannelCount
                                        simpleSorterModelType
                                        excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen


            let baseMutator =
                match rp.GetMutatorParams() with
                | Some configured -> MutatorParams.toModelMutator modificationRate configured
                | None ->
                    SimpleSorterModelMutator.getMsceModelMutator (RngFactory.create rngType)
                        excludeSelfCe modificationRate mutationRate insertionRate deletionRate
                        (rp.GetMutatorVariant() |> Option.defaultValue mutatorVariant.V1)
                    |> sorterModelMutator.Simple
            let! sorterModelMutator = MutatorMakers.applyParameters rp baseMutator

            let parentMutantMap = 
                    SorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        parentSorterModelSet.SorterModels
                                        %sorterChildCount
                                        mutationMod

            return (_sorterEvalSelection, parentMutantMap)
        }


    let makePrefixMutantDetails (sources: evolutionSetupSources) (rp:runParameters) :
            Async<Result<
                        sorterSelection * 
                        Map<Guid<sorterModelId>, Guid<sorterModelId>>, 
                        string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (channelCount: int<channelCount>) =
                        rp.GetChannelCount()
                        |> Result.ofOption "Missing channel count in run parameters"
    
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

            let! (sest: sorterSelectionType) = 
                        rp.GetSeedSorterPoolSelectionType()
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
                        sources.LoadStandardSorterEvals
                                            channelCount
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection sem sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let! (parentSorterModelGen: sorterModelGen) =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                                        rngType 
                                        channelCount
                                        simpleSorterModelType
                                        excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen


            let baseMutator =
                match rp.GetMutatorParams() with
                | Some configured -> MutatorParams.toModelMutator modificationRate configured
                | None ->
                    SimpleSorterModelMutator.getMsceModelMutator (RngFactory.create rngType)
                        excludeSelfCe modificationRate mutationRate insertionRate deletionRate
                        (rp.GetMutatorVariant() |> Option.defaultValue mutatorVariant.V1)
                    |> sorterModelMutator.Simple
            let! sorterModelMutator = MutatorMakers.applyParameters rp baseMutator

            let parentMutantMap = 
                    SorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        parentSorterModelSet.SorterModels
                                        %sorterChildCount
                                        mutationMod

            return (_sorterEvalSelection, parentMutantMap)
        }


