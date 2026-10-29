namespace GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Standard

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup

module SgdEx_Standard =

    /// Executes a resolved evolutionary path with separate progress via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRunStandard
            (makeMutator: sorterModelMutatorMaker)
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (path: evoPath)
            (state: evoProgress)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<evoProgress, string>> =

        let start = path.Start
        let log (msg: string) =
                OpsUtils.report progress
                    (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (string path.Id) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token

                log "Loading saved Setup SortableTestsWithPrefix..."
                let! sWidth =
                    path.GetSortingWidth()
                    |> Result.ofOption "Missing sorting width."
                let! testsData = genDb.loadAsync geneSortDataMode.Setup start.Tests
                let! (sortableTests, ces) = OutputData.asSortableTestsWithPrefix testsData
                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                // 1. Check for existing checkpoints directly via genDb
                let! highestPoolSetOpt = EvolutionFiles.loadHighestStandard saveIntervals genDb path state

                // 2. Resume Results or load the initial pool prepared by setup
                let! (activeSeedPoolSet, activeState) =
                    match highestPoolSetOpt with
                    | None ->
                        asyncResult {
                            let! setupData = genDb.loadAsync geneSortDataMode.Setup start.Pool
                            let! seedSet = OutputData.asSorterPoolSetStandard setupData
                            let! seedSet = EvoAppendPools.prepareStandard path seedSet
                            log "Loaded initial Setup sorterPoolSet. Beginning evolution."
                            return seedSet, state.WithCurrentGeneration(Some seedSet.GenerationNumber)
                        }
                    | Some (highestPoolSet: sorterPoolSet_Standard) ->
                        asyncResult {
                            let currentGen = highestPoolSet.GenerationNumber
                            log (sprintf "Found existing checkpoint at Generation %d. Resuming evolution." %currentGen)
                            let updatedState = state.WithCurrentGeneration(Some currentGen)
                            return highestPoolSet, updatedState
                        }

                do! checkCancellation cts.Token

                log "Making sorterModelMutator..."
                do! (activeSeedPoolSet: sorterPoolSet_Standard).SorterPools |> Map.values
                    |> Seq.collect (fun pool -> pool.SorterPoolMembers)
                    |> Seq.map (fun memberValue -> memberValue.SorterModel)
                    |> MutatorMakers.validateModels path
                let! (sSmm: simpleSorterModelMutator) = makeMutator path
                let! sorterModelMutator = sSmm |> sorterModelMutator.Simple |> MutatorMakers.applyPath path

                log "Executing unified evolution run..."
                let! (finalRunResult: sorterPoolSet_Standard) =
                    EvoOrch_Standard.runStandardEvolutionAsync
                        genDb
                        saveIntervals
                        subIntervals
                        path
                        activeState
                        allowOverwrite
                        activeSeedPoolSet
                        sortableTests
                        prefix
                        sorterModelMutator
                        cts.Token
                        log

                log "evaluateEvolutionRun completed."
                return activeState.WithCurrentGeneration(Some finalRunResult.GenerationNumber).WithFinished(Some true)

            with
            | :? OperationCanceledException when cts.IsCancellationRequested -> return! Error "Operation cancelled"
            | e ->
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg
                return! Error errorMsg
        } |> Async.map (OpsUtils.logResult progress log)
