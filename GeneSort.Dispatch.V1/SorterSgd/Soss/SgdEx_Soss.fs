namespace GeneSort.Dispatch.V1.SorterSgd.Soss

open GeneSort.Project.V1.Catalogs.Evolution
open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps
open GeneSort.Dispatch.V1
open GeneSort.Sorting.Sorter
open GeneSort.Dispatch.V1.SorterSgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Project.V1.Parameters

module SgdEx_Soss =

    /// Branch an imported seed in memory. Call only when starting from Setup, never on resume.
    let prepareImportedPool (rp: runParameters) (pool: sorterPoolSet_Soss) =
        if not (Map.containsKey ParamKeys.sourcePoolFileIdKey rp.ParamMap) then Ok pool
        else
            rp.GetMutationMod() |> Result.ofOption "Missing MutationMod."
            |> Result.map (fun modifier ->
                if pool.SorterPools |> Map.forall (fun _ item -> item.MutationMod = modifier) then pool
                else
                    let pools =
                        pool.SorterPools |> Map.values
                        |> Seq.map (fun item ->
                            if item.MutationMod = modifier then item
                            else SorterPool_Soss.deriveChildPool (UMX.tag (Guid.NewGuid())) modifier item)
                        |> Seq.toArray
                    sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) pool.GenerationNumber pool.LatticeBounds (Some pools))

    /// Creates, evaluates, and saves the initial pool as Setup when no saved pool exists
    let initializeAndSaveSeedPoolSet 
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (rp: runParameters)
            (sortableTests: sortableTests)
            (prefix: ceBlock)
            (log: string -> unit) : Async<Result<sorterPoolSet_Soss, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            log "No saved checkpoint or Setup pool found. Creating initial seedSorterPoolSet..."
            let! seedPoolSet = sorterPoolSetCreator rp
            let! seedSossPoolSet = SorterPoolSet.asSoss seedPoolSet
            
            let computedEvals = 
                seedSossPoolSet 
                |> PoolRunner_Soss.evaluatePoolSet
                    sortableTests 
                    prefix
                    evalType
                    true // reEvaluateParents
                    (false |> UMX.tag<collectNewSortableTests>)
            
            let! selectionMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
            let evaluatedSeedSet =
                seedSossPoolSet
                |> SorterPoolSet_Soss.updateSorterEvals computedEvals
                |> SorterPoolSet_Soss.withSelectionMeasure selectionMeasure

            // Save the evaluated initial pool for reuse when no Results checkpoint exists
            let! qpSsrr = 
                genDb.makeQueryParamsFromRunParams rp (geneSortDataType.SorterPoolSet "")
                |> Result.ofOption "Failed to create QueryParams for seedSorterRunResult."   
            do! genDb.saveAsync geneSortDataMode.Setup qpSsrr (evaluatedSeedSet |> sorterPoolSet.Soss |> outputData.SorterPoolSet) (false |> UMX.tag<allowOverwrite>)
            log (sprintf "Initial seedSorterPoolSet saved as Setup at generation %d." %evaluatedSeedSet.GenerationNumber)

            return evaluatedSeedSet
        }


    /// Prepares reusable generation-zero tests and the evaluated seed pool.
    let setupEvolutionRunSoss
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
                    let! _ = OutputData.asSorterPoolSetSoss saved
                    log "Reusing initial Setup sorterPoolSet."
                else
                    let prefix = ceBlock.create (UMX.tag Guid.Empty) width ces
                    let! (_: sorterPoolSet_Soss) =
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
    let evaluateEvolutionRunSoss
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

                let! partitionCount = rp.GetPartitionCount() |> Result.ofOption "Missing PartitionCount."
                do! if %partitionCount > 0 then Ok () else Error "PartitionCount must be greater than zero."

                log "Loading saved Setup SortableTestsWithPrefix..."
                let! sWidth = 
                    rp.GetSortingWidth() 
                    |> Result.ofOption "Missing sorting width."
                let! (sortableTests, ces) = Utils.loadSortableTestsWithPrefix genDb rp
                let! seedSoss =
                    rp.GetSeedSoss()
                    |> Result.ofOption "Missing SeedSoss."
                let! rngType =
                    rp.GetRngType()
                    |> Result.ofOption "Missing RNG type."
                let sossRng: IRando =
                    match rngType with
                    | Lcg -> randomLcg(seedSoss) :> IRando
                    | Net -> randomNet(UMX.tag<randomSeed> (int32 seedSoss)) :> IRando
                    | Smx -> randomSplitMix64(seedSoss) :> IRando
                let sortableTestsPartitions =
                    PartitionOfSortableTests.partition sossRng.NextIndex (UMX.cast<partitionCount, sortableTestsCount> partitionCount) sortableTests
                let partitionSizes =
                    sortableTestsPartitions.SortableTests
                    |> Map.values
                    |> Seq.map (fun tests -> string %(SortableTests.getSortableCount tests))
                    |> String.concat ", "
                if %partitionCount = 1 then
                    log (sprintf "Using unpartitioned sortable tests with test count: %s." partitionSizes)
                else
                    log (sprintf "Partitioned sortable tests into %d subsets with test counts: %s." %partitionCount partitionSizes)

                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                // 1. Check for existing checkpoints directly via genDb
                let! highestPoolSetOpt = Utils.loadHighestGenSorterPoolSetSoss saveIntervals genDb rp

                // 2. Resume Results or load the initial pool prepared by setup
                let! (activeSeedPoolSet, activeRp) = 
                    match highestPoolSetOpt with
                    | None -> 
                        asyncResult {
                            let initRp = rp.WithGenerationCurrent(Some (0 |> UMX.tag<generationNumber>))
                            let! setupData = Utils.loadSetupOutputData genDb initRp (geneSortDataType.SorterPoolSet "")
                            let! seedSet = OutputData.asSorterPoolSetSoss setupData
                            let! seedSet = prepareImportedPool rp seedSet
                            log "Loaded initial Setup sorterPoolSet. Beginning evolution."
                            return seedSet, initRp.WithGenerationCurrent(Some seedSet.GenerationNumber)
                        }
                    | Some (highestPoolSet: sorterPoolSet_Soss) ->
                        asyncResult {
                            let currentGen = highestPoolSet.GenerationNumber
                            log (sprintf "Found existing checkpoint at Generation %d. Resuming evolution." %currentGen)
                            let updatedRp = rp.WithGenerationCurrent(Some currentGen)
                            return highestPoolSet, updatedRp
                        }

                do! checkCancellation cts.Token
                
                log "Making sorterModelMutator..."
                let! (sSmm: simpleSorterModelMutator) = makeMutator activeRp
                let (sorterModelMutator: sorterModelMutator) = sSmm |> sorterModelMutator.Simple

                log "Executing unified evolution run..."
                let! (_finalRunResult: sorterPoolSet_Soss) =
                    EvoOrch_Soss.runSossEvolutionAsync
                        genDb
                        saveIntervals
                        subIntervals
                        activeRp
                        allowOverwrite
                        activeSeedPoolSet
                        sortableTestsPartitions
                        prefix
                        sorterModelMutator
                        cts.Token
                        log

                log "evaluateEvolutionRun completed."
                return activeRp

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (OpsUtils.logResult progress log)
