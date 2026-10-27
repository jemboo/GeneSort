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
