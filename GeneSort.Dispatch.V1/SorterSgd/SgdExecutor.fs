namespace GeneSort.Dispatch.V1.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Sorting.Sortable
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps
open GeneSort.Dispatch.V1

module SgdExecutor =

    /// Handles initialization, evaluation, and DB saving when no checkpoint exists
    let initializeAndSaveSeedPoolSet 
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (genDb: IGeneSortGenDb)
            (rp: runParameters)
            (sortableTest: sortableTest)
            (log: string -> unit) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let evalType = sorterEvalType.V2
            log "No saved checkpoint found. Creating initial seedSorterPoolSet..."
            let! seedPoolSet = sorterPoolSetCreator rp
            
            let computedEvals = 
                seedPoolSet 
                |> SorterPoolRunner.evaluatePoolSet 
                    sortableTest 
                    evalType
                    true // reEvaluateParents
                    (false |> UMX.tag<collectNewSortableTests>)
            
            let evaluatedSeedSet = seedPoolSet |> SorterPoolSet.updateSorterEvals computedEvals
            let seedSorterRunResult = sorterRunResult.create evaluatedSeedSet [||] |> outputData.SorterRunResult
            
            let! qpSsrr = 
                genDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterRunResult "")
                |> Result.ofOption "Failed to create QueryParams for seedSorterRunResult."
                
            do! genDb.saveAsync qpSsrr seedSorterRunResult (false |> UMX.tag<allowOverwrite>)
            log (sprintf "Initial seedSorterPoolSet saved at generation %d." %evaluatedSeedSet.GenerationNumber)

            return evaluatedSeedSet
        }


    /// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRun
            (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (genDb: IGeneSortGenDb)
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

                log "Executing makeSortableTests..."
                let! (sortableTest: sortableTest) = makeSortableTests rp

                // 1. Check for existing checkpoints directly via genDb
                let! highestResultOpt = 
                    Utils.loadSorterRunResultWithHighestGenerationNumber genDb rp cts.Token log

                // 2. Conditionally initialize or resume from the highest discovered checkpoint
                let! (activeSeedPoolSet: sorterPoolSet), (activeRp: runParameters) = 
                    match highestResultOpt with
                    | None -> 
                        asyncResult {
                            let initRp = rp.WithGenerationCurrent(Some (0 |> UMX.tag<generationNumber>))
                            let! (seedSet: sorterPoolSet) = initializeAndSaveSeedPoolSet sorterPoolSetCreator genDb initRp sortableTest log
                            return seedSet, initRp
                        }
                    | Some (highestResult: sorterRunResult) -> 
                        asyncResult {
                            let (currentGen: int<generationNumber>) = highestResult.FinalPoolSet.GenerationNumber
                            log (sprintf "Found existing checkpoint at Generation %d. Resuming evolution." %currentGen)
                            let updatedRp = rp.WithGenerationCurrent(Some currentGen)
                            return highestResult.FinalPoolSet, updatedRp
                        }

                do! checkCancellation cts.Token
                
                log "Making sorterModelMutator..."
                let! (simpleSorterModelMutator: simpleSorterModelMutator) = MutatorMakers.makeSimpleSorterModelMutator activeRp
                let sorterModelMutator = simpleSorterModelMutator |> sorterModelMutator.Simple

                log "Executing unified evolution run..."
                let! (_finalRunResult: sorterRunResult) = 
                    EvolutionOrchestrator.runEvolutionAsync
                        genDb
                        activeRp
                        allowOverwrite
                        activeSeedPoolSet
                        sortableTest
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