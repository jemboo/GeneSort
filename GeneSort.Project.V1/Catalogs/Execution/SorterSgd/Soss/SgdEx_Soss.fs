namespace GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Soss

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup

module SgdEx_Soss =

    let prepareImportedPool = EvoAppendPools.prepareSoss

    /// Recreate partition membership from saved setup tests and the run's fixed seed.
    /// Each invocation starts a fresh RNG, independently of the resumed generation.
    let makeSortableTestsPartitions rngType seedSoss (partitionCount: int<partitionCount>) sortableTests =
        let sossRng: IRando =
            match rngType with
            | Lcg -> randomLcg(seedSoss) :> IRando
            | Net -> randomNet(UMX.tag<randomSeed> (int32 seedSoss)) :> IRando
            | Smx -> randomSplitMix64(seedSoss) :> IRando
        PartitionOfSortableTests.partition sossRng.NextIndex
            (UMX.cast<partitionCount, sortableTestsCount> partitionCount) sortableTests

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
                let sortableTestsPartitions =
                    makeSortableTestsPartitions rngType seedSoss partitionCount sortableTests
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
                do! (activeSeedPoolSet: sorterPoolSet_Soss).SorterPools |> Map.values
                    |> Seq.collect (fun pool -> pool.SorterPoolMembers)
                    |> Seq.map (fun memberValue -> memberValue.SorterModel)
                    |> MutatorMakers.validateModels activeRp
                let! (sSmm: simpleSorterModelMutator) = makeMutator activeRp
                let! sorterModelMutator = sSmm |> sorterModelMutator.Simple |> MutatorMakers.applyParameters activeRp

                let! poolSetForEvolution = asyncResult {
                    if highestPoolSetOpt.IsNone then return activeSeedPoolSet
                    else
                        // Legacy caches do not record the sorter length used to
                        // evaluate them. Refresh all subsets before the orchestrator
                        // can rank members or pools; preserve the previous selection
                        // subset so transition detection still works on resume.
                        let subsetId =
                            sortableTestsPartitions.SortableTests |> Map.keys |> Seq.head
                            |> UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId>
                        let! collectNewSortableTests =
                            activeRp.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests."
                        log "Refreshing checkpoint evaluations at the saved pool CE lengths."
                        return activeSeedPoolSet
                               |> SorterPoolSet_Soss.clearEvaluations
                               |> Pipeline_Soss.evaluateRequiredPartitions sortableTestsPartitions prefix sorterEvalType.V2 true
                                   collectNewSortableTests subsetId true
                }

                log "Executing unified evolution run..."
                let! (finalRunResult: sorterPoolSet_Soss) =
                    EvoOrch_Soss.runSossEvolutionAsync
                        genDb
                        saveIntervals
                        subIntervals
                        activeRp
                        allowOverwrite
                        poolSetForEvolution
                        sortableTestsPartitions
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
