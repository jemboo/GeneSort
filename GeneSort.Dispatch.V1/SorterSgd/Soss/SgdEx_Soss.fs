namespace GeneSort.Dispatch.V1.SorterSgd.Soss

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

module SgdEx_Soss =

    /// Handles initialization, evaluation, and DB saving when no checkpoint exists
    let initializeAndSaveSeedPoolSet 
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet_Standard, string>>)
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (rp: runParameters)
            (sortableTests: sortableTests)
            (prefix: ceBlock)
            (log: string -> unit) : Async<Result<sorterPoolSet_Soss, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            log "No saved checkpoint found. Creating initial seedSorterPoolSet..."
            let! standardSeedPoolSet = sorterPoolSetCreator rp
            let seedPoolSet = SorterPoolSet_Soss.fromStandard standardSeedPoolSet
            
            let computedEvals = 
                seedPoolSet 
                |> PoolRunner_Soss.evaluatePoolSet
                    sortableTests 
                    prefix
                    evalType
                    true // reEvaluateParents
                    (false |> UMX.tag<collectNewSortableTests>)
            
            let evaluatedSeedSet = seedPoolSet |> SorterPoolSet_Soss.updateSorterEvals computedEvals

            // Save SorterPoolSetSummaries
            let! qpSsrr = 
                genDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterPoolSet "")
                |> Result.ofOption "Failed to create QueryParams for seedSorterRunResult."   
            do! genDb.saveAsync qpSsrr (seedPoolSet |> sorterPoolSet.Soss |> outputData.SorterPoolSet) (false |> UMX.tag<allowOverwrite>)
            log (sprintf "Initial seedSorterPoolSet saved at generation %d." %evaluatedSeedSet.GenerationNumber)

            return evaluatedSeedSet
        }


    /// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRunSoss
            (makeSortableTests: runParameters ->  Async<Result<sortableTests * (ce array), string>> )
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet_Standard, string>>)
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

                log "Executing makeSortableTests..."
                let! sWidth = 
                    rp.GetSortingWidth() 
                    |> Result.ofOption "Missing sorting width."
                let! (sortableTests, ces) = makeSortableTests rp 
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
                    SetOfSortableTests.partition sossRng.NextIndex (2 |> UMX.tag<sortableTestsCount>) sortableTests
                let sortableTestsFirst = sortableTestsPartitions.SortableTests.[UMX.tag<sortableTestsSubsetId> "0"]
                let sortableTestsSecond = sortableTestsPartitions.SortableTests.[UMX.tag<sortableTestsSubsetId> "1"]
                log (sprintf "Partitioned sortable tests into two subsets of %d and %d tests."
                        %(SortableTests.getSortableCount sortableTestsFirst)
                        %(SortableTests.getSortableCount sortableTestsSecond))

                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                // 1. Check for existing checkpoints directly via genDb
                let! highestPoolSetOpt = Utils.loadHighestGenSorterPoolSetSoss saveIntervals genDb rp

                // 2. Conditionally initialize or resume from the highest discovered checkpoint
                let! (activeSeedPoolSet, activeRp) = 
                    match highestPoolSetOpt with
                    | None -> 
                        asyncResult {
                            let initRp = rp.WithGenerationCurrent(Some (0 |> UMX.tag<generationNumber>))
                            let! (seedSet: sorterPoolSet_Soss) =
                                        initializeAndSaveSeedPoolSet 
                                            sorterPoolSetCreator genDb saveIntervals subIntervals initRp sortableTests prefix log
                            return seedSet, initRp
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
                let! (sSmm: simpleSorterModelMutator) = MutatorMakers.makeSimpleSorterModelMutator activeRp
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
