namespace GeneSort.Dispatch.V1.SorterSgd.Standard

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd
open GeneSort.Model.Sorting.Simple.V1

module SgdEx_Standard =

    /// Creates, evaluates, and saves the initial pool as Setup when no saved pool exists
    let initializeAndSaveSeedPoolSet 
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (rp: runParameters)
            (sortableTests: sortableTests)
            (prefix: ceBlock)
            (log: string -> unit) : Async<Result<sorterPoolSet_Standard, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            log "No saved checkpoint or Setup pool found. Creating initial seedSorterPoolSet..."
            let! seed = sorterPoolSetCreator rp
            let! seedPoolSet = SorterPoolSet.asStandard seed
            
            let computedEvals = 
                seedPoolSet 
                |> PoolRunner_Standard.evaluatePoolSet 
                    sortableTests 
                    prefix
                    evalType
                    true // reEvaluateParents
                    (false |> UMX.tag<collectNewSortableTests>)
            
            let! selectionMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
            let evaluatedSeedSet =
                seedPoolSet
                |> SorterPoolSet_Standard.updateSorterEvals computedEvals
                |> SorterPoolSet_Standard.withSelectionMeasure selectionMeasure

            // Save the evaluated initial pool for reuse when no Results checkpoint exists
            let! qpSsrr = 
                genDb.makeQueryParamsFromRunParams rp (geneSortDataType.SorterPoolSet "")
                |> Result.ofOption "Failed to create QueryParams for seedSorterRunResult."   
            do! genDb.saveAsync geneSortDataMode.Setup qpSsrr (evaluatedSeedSet |> sorterPoolSet.Standard |> outputData.SorterPoolSet) (false |> UMX.tag<allowOverwrite>)
            log (sprintf "Initial seedSorterPoolSet saved as Setup at generation %d." %evaluatedSeedSet.GenerationNumber)

            return evaluatedSeedSet
        }


    /// Prepares reusable generation-zero tests and the evaluated seed pool.
    let setupEvolutionRunStandard
            (makeSortableTests: runParameters -> Async<Result<sortableTests * ce array, string>>)
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (rp: runParameters)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let log msg =
            OpsUtils.report progress
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (RunParameters.getIdString rp) msg)
        asyncResult {
            try
                do! checkCancellation cts.Token
                let initRp = rp.WithGenerationCurrent(Some (UMX.tag<generationNumber> 0))
                let! width = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
                let! (tests, ces) = Utils.loadOrCreateSortableTestsWithPrefix makeSortableTests genDb initRp
                do! checkCancellation cts.Token
                let! query =
                    genDb.makeQueryParamsFromRunParams initRp (geneSortDataType.SorterPoolSet "")
                    |> Result.ofOption "Failed to create QueryParams for initial Setup sorterPoolSet."
                let! exists = genDb.doesOutPutDataExist geneSortDataMode.Setup query
                if exists then
                    let! saved = genDb.loadAsync geneSortDataMode.Setup query
                    let! _ = OutputData.asSorterPoolSetStandard saved
                    log "Reusing initial Setup sorterPoolSet."
                else
                    let prefix = ceBlock.create (UMX.tag Guid.Empty) width ces
                    let! (_: sorterPoolSet_Standard) =
                        initializeAndSaveSeedPoolSet
                            sorterPoolSetCreator genDb saveIntervals subIntervals initRp tests prefix log
                    ()
                do! checkCancellation cts.Token
                log "Evolution setup complete."
                return rp
            with e ->
                return! Error (sprintf "Error in evolution setup: %s" e.Message)
        } |> Async.map (OpsUtils.logResult progress log)

    /// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRunStandard
            (makeMutator: sorterModelMutatorMaker)
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log (msg: string) =
                OpsUtils.report progress 
                    (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token

                log "Loading saved Setup SortableTestsWithPrefix..."
                let! sWidth = 
                    rp.GetSortingWidth() 
                    |> Result.ofOption "Missing sorting width."
                let! (sortableTests, ces) = Utils.loadSortableTestsWithPrefix genDb rp
                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                // 1. Check for existing checkpoints directly via genDb
                let! highestPoolSetOpt = Utils.loadHighestGenSorterPoolSet saveIntervals genDb rp

                // 2. Resume Results or load the initial pool prepared by setup
                let! (activeSeedPoolSet, activeRp) = 
                    match highestPoolSetOpt with
                    | None -> 
                        asyncResult {
                            let initRp = rp.WithGenerationCurrent(Some (0 |> UMX.tag<generationNumber>))
                            let! setupData = Utils.loadSetupOutputData genDb initRp (geneSortDataType.SorterPoolSet "")
                            let! seedSet = OutputData.asSorterPoolSetStandard setupData
                            let! seedSet = EvoAppendPools.prepareStandard rp seedSet
                            log "Loaded initial Setup sorterPoolSet. Beginning evolution."
                            return seedSet, initRp.WithGenerationCurrent(Some seedSet.GenerationNumber)
                        }
                    | Some (highestPoolSet: sorterPoolSet_Standard) -> 
                        asyncResult {
                            let currentGen = highestPoolSet.GenerationNumber
                            log (sprintf "Found existing checkpoint at Generation %d. Resuming evolution." %currentGen)
                            let updatedRp = rp.WithGenerationCurrent(Some currentGen)
                            return highestPoolSet, updatedRp
                        }

                do! checkCancellation cts.Token
                
                log "Making sorterModelMutator..."
                do! (activeSeedPoolSet: sorterPoolSet_Standard).SorterPools |> Map.values
                    |> Seq.collect (fun pool -> pool.SorterPoolMembers)
                    |> Seq.map (fun memberValue -> memberValue.SorterModel)
                    |> MutatorMakers.validateModels activeRp
                let! (sSmm: simpleSorterModelMutator) = makeMutator activeRp
                let! sorterModelMutator = sSmm |> sorterModelMutator.Simple |> MutatorMakers.applyParameters activeRp

                log "Executing unified evolution run..."
                let! (finalRunResult: sorterPoolSet_Standard) =
                    EvoOrch_Standard.runStandardEvolutionAsync
                        genDb
                        saveIntervals
                        subIntervals
                        activeRp
                        allowOverwrite
                        activeSeedPoolSet
                        sortableTests
                        prefix
                        sorterModelMutator
                        cts.Token
                        log

                log "evaluateEvolutionRun completed."
                return activeRp.WithGenerationCurrent(Some finalRunResult.GenerationNumber).WithRunFinished(Some true)

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (OpsUtils.logResult progress log)
