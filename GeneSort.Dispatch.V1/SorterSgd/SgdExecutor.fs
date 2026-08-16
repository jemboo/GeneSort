namespace GeneSort.Dispatch.V1.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
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
            log "No saved checkpoint found. Creating initial seedSorterPoolSet..."
            let! seedPoolSet = sorterPoolSetCreator rp
            
            let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."

            let computedEvals = 
                seedPoolSet 
                |> SorterPoolRunner.evaluatePoolSet 
                    sortableTest 
                    sorterEvalType
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

                log "Initializing seed pool set..."
                let! (initialSeedPoolSet: sorterPoolSet) = 
                        initializeAndSaveSeedPoolSet
                            sorterPoolSetCreator
                            genDb
                            rp
                            sortableTest
                            log

                do! checkCancellation cts.Token
                
                log "Making sorterModelMutator..."
                let! (simpleSorterModelMutator: simpleSorterModelMutator) = MutatorMakers.makeSimpleSorterModelMutator rp
                let sorterModelMutator = simpleSorterModelMutator |> sorterModelMutator.Simple

                log "Executing unified evolution run..."
                let! (_finalRunResult: sorterRunResult) = 
                    EvolutionOrchestrator.runEvolutionAsync
                        genDb
                        rp
                        allowOverwrite
                        initialSeedPoolSet
                        sortableTest
                        sorterModelMutator
                        cts.Token
                        log

                log "evaluateEvolutionRun completed."
                let finalRp = rp.WithRunFinished(Some true)
                return finalRp

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (OpsUtils.logResult progress log)