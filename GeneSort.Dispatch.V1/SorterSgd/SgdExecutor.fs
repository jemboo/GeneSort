namespace GeneSort.Dispatch.V1.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps

module SgdExecutor =

    /// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRun
            (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (host: IRunHost)
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

                // 1. Gather required run metrics and options out of parameters block
                let! initialGenCurrent = rp.GetGenerationCurrent() |> Result.ofOption "Missing genCurrent."
                let! genIntervalCount = rp.GetGenerationIntervalCount() |> Result.ofOption "Missing genIntervalCount."
                let! sorterPoolSelectionIntervals = rp.GetSorterPoolSelectionIntervals() |> Result.ofOption "Missing sorterPoolSelectionInterval."
                let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
                let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
                let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count"
                let! sorterEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
                let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."
                let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
                let! sortedFraction = rp.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."
                let! sorterCountCycle = rp.GetSorterCountCycle() |> Result.ofOption "Missing sorterCountCycle."
                let! sorterCountCycleMultiplier = rp.GetSorterCountCycleMultiplier() |> Result.ofOption "Missing sorterCountCycleMultiplier."
                let! sorterPoolExpansionRate = rp.GetSorterPoolExpansionRate () |> Result.ofOption "Missing sorterPoolExpansionRate."
                let! sorterPoolMeasure = rp.GetSorterPoolMeasure() |> Result.ofOption "Missing sorterPoolMeasure."
                let! (collectNewSortableTests: bool<collectNewSortableTests>) = rp.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests"
                
                // 2. Make sortableTest
                log "Executing makeSortableTests..."
                let! (sortableTest: sortableTest) = makeSortableTests rp

                // 3. Verify host.RunDb is IGeneSortGenDb and extract save configs
                let genDb = 
                    match host.RunDb with
                    | :? IGeneSortGenDb as gdb -> gdb
                    | _ -> failwith "host.RunDb must implement IGeneSortGenDb"

                // 4. Query DB for existing state to determine genCurrent and initialSeedPoolSet
                log "Checking for saved checkpoint..."
                let! qpBase = 
                    genDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterRunResult "")
                    |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                let! (maybeNextData: outputData option) = 
                    genDb.getNextGenSavePointAsync rp (outputDataType.SorterRunResult "")
                    |> Async.map Ok

                let! (initialSeedPoolSet: sorterPoolSet) = 
                    match maybeNextData with
                    | Some outData ->
                        asyncResult {
                            let! sorterRunRes = outData |> OutputData.asSorterRunResult
                            log (sprintf "Resuming evolution from saved checkpoint at generation %d." %sorterRunRes.FinalPoolSet.GenerationNumber)
                            return sorterRunRes.FinalPoolSet
                        }
                    | None ->
                        asyncResult {
                            log "No saved checkpoint found. Creating initial seedSorterPoolSet..."
                            let! (seedPoolSet: sorterPoolSet) = sorterPoolSetCreator rp
                            let reEvaluateParents = true
                            let computedEvals = 
                                seedPoolSet 
                                |> SorterPoolRunner.evaluatePoolSet 
                                    sortableTest 
                                    sorterEvalType
                                    reEvaluateParents
                                    collectNewSortableTests
                            
                            let evaluatedSeedSet = seedPoolSet |> SorterPoolSet.updateSorterEvals computedEvals
                            return evaluatedSeedSet
                        }

                let updatedRp = rp.WithGenerationCurrent(Some initialSeedPoolSet.GenerationNumber)

                do! checkCancellation cts.Token
                
                log "Making sorterModelMutator..."
                let! (simpleSorterModelMutator: simpleSorterModelMutator) = MutatorMakers.makeSimpleSorterModelMutator updatedRp
                let sorterModelMutator = simpleSorterModelMutator |> sorterModelMutator.Simple

                log (sprintf "Executing unified evolution run starting at generation %d..." %initialSeedPoolSet.GenerationNumber)
                let! (finalRunResult: sorterRunResult) = 
                    EvolutionOrchestrator.runEvolutionAsync
                        host updatedRp allowOverwrite initialSeedPoolSet.GenerationNumber genIntervalCount
                        sorterCountCycle sorterCountCycleMultiplier sorterPoolExpansionRate
                        sorterModelMutator prioritizeNewMutants distinctSorterHashes
                        sortersPerPool sorterChildCount sortableTest sorterEvalType
                        sorterEvalMeasure initialSeedPoolSet collectNewSortableTests sortedFraction 
                        sorterPoolSelectionIntervals
                        sorterPoolMeasure cts.Token log

                log "evaluateEvolutionRun completed."
                return updatedRp

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)