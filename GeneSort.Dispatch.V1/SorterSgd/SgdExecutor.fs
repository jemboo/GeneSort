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

                // 1. Gather all required run metrics and options out of your parameters block securely
                let! genLast = rp.GetGenerationLast() |> Result.ofOption "Missing genLast."             
                let! genCurrent = rp.GetGenerationCurrent() |> Result.ofOption "Missing genCurrent."
                let! snapshotReportInterval = rp.GetSnapshotReportIntervals() |> Result.ofOption "Missing snapshot report interval."
                let! summaryReportInterval = rp.GetSummaryReportIntervals() |> Result.ofOption "Missing summary report interval."
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
                let! (collectNewSortableTests: bool<collectNewSortableTests>) = rp.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests in run parameters"

                log "Executing makeSortableTests..."
                let! (sortableTest: sortableTest) = makeSortableTests rp

                // 2. Resolve target seed sorterPoolSet collection state depending on genFirst criteria
                let! (initialSeedPoolSet: sorterPoolSet) = 
                    if %genCurrent > 0 then
                        log "Looking up historical sorterPoolSet from database..."
                        let qpSRRResult = 
                            host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterRunResult "")
                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                        asyncResult {
                            let! qpSRR = qpSRRResult 
                            let! (outData: outputData) = host.RunDb.loadAsync qpSRR |> AsyncResult.mapError (fun err -> sprintf "Database load error: %A" err)
                            let! sorterRunRes = outData |> OutputData.asSorterRunResult
                            return sorterRunRes.FinalPoolSet
                        }
                    else
                        log "Make seedSorterPoolSet..."
                        asyncResult {
                            let! (seedPoolSet: sorterPoolSet) = sorterPoolSetCreator rp
                            let reEvaluateParents = true
                            let computedEvals = 
                                    seedPoolSet 
                                    |> SorterPoolRunner.evaluatePoolSet 
                                                        sortableTest 
                                                        sorterEvalType
                                                        reEvaluateParents
                                                        collectNewSortableTests
                            return seedPoolSet |> SorterPoolSet.updateSorterEvals computedEvals
                        }

                do! checkCancellation cts.Token
                
                log "Making sorterModelMutator..."
                let! (simpleSorterModelMutator :simpleSorterModelMutator) = MutatorMakers.makeSimpleSorterModelMutator rp
                let sorterModelMutator = simpleSorterModelMutator |> sorterModelMutator.Simple

                log "Executing unified evolution run..."
                let! (finalRunResult: sorterRunResult) = 
                    EvolutionOrchestrator.runEvolutionAsync
                        host rp allowOverwrite genCurrent (genLast - genCurrent)
                        sorterCountCycle sorterCountCycleMultiplier sorterPoolExpansionRate
                        sorterModelMutator prioritizeNewMutants distinctSorterHashes
                        sortersPerPool sorterChildCount sortableTest sorterEvalType
                        sorterEvalMeasure initialSeedPoolSet collectNewSortableTests sortedFraction 
                        snapshotReportInterval summaryReportInterval sorterPoolSelectionIntervals
                        sorterPoolMeasure cts.Token log

                log "evaluateEvolutionRun completed."
                let finalRp = rp.WithRunFinished(Some true)
                return finalRp

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)