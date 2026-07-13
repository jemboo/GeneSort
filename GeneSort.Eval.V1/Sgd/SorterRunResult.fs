namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open System.Threading
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open System
open GeneSort.Eval.V1

/// Holds the combined results of the historical optimization run using light snapshot telemetry
type sorterRunResult = 
    private {
        _intermediateHistory: sorterPoolSetSummary array
        _finalPoolSet: sorterPoolSet
    }
    member this.FinalPoolSet with get() = this._finalPoolSet
    member this.IntermediateHistory with get() = this._intermediateHistory
    static member create 
                    (finalPoolSet:sorterPoolSet) 
                    (intermediateHistory:sorterPoolSetSummary []) =
            {
                _intermediateHistory = intermediateHistory
                _finalPoolSet = finalPoolSet
            }


module SorterRunResult =

    /// Asynchronously runs the generation loops step N times using an asyncResult pipeline.
    /// Accumulates lightweight snapshots for intermediate states and forced GC compacting 
    /// over long execution steps.
    let runEvolutionAsync
            (genStart: int<generationNumber>)
            (genCount: int<generationNumber>)
            (mutator: sorterModelMutator)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (sortableTest: sortableTest)
            (srtrEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (initialPoolSet: sorterPoolSet)
            (sortedFractionThreshold: float<sortedFraction>)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult, string>> =

        // --- Exponential Frequency Configuration ---
        let totalGenInt = int (genStart + genCount)
        let targetGenerations = MathUtils.expSampler 1 totalGenInt MathUtils.kSample10K

        let rec loop 
                    (remainingSteps: int) 
                    (currentSorterPoolSet: sorterPoolSet) 
                    (historyAcc: sorterPoolSetSummary list) 
                    : Async<Result<sorterRunResult, string>> =
            asyncResult {
                if cts.IsCancellationRequested then 
                    return! Error "runEvolutionAsync was cancelled."

                if remainingSteps <= 0 then
                    let finalResult = 
                        sorterRunResult.create 
                            currentSorterPoolSet 
                            (historyAcc |> List.rev |> List.toArray)
                    return finalResult
                else
                    let currentGen = genStart + (genCount - %remainingSteps)
                    let totalGen = genStart + genCount
                    
                    // Look up if the current generation is an exponential milestone
                    let shouldReport = (Set.contains (int currentGen) targetGenerations)

                    if shouldReport then
                        log (sprintf "Starting evolution step. Generation %d of %d" currentGen totalGen)

                    // 1. Snapshot the current generation state before applying structural changes
                    let updatedHistory = 
                        if shouldReport then 
                            let currentSnapshot = SorterPoolSetSummary.fromPoolSet currentSorterPoolSet
                            currentSnapshot :: historyAcc
                        else 
                            historyAcc

                    let adjSorterEvalType = if (remainingSteps = 1) then sorterEvalType.V2 else srtrEvalType
                    let reEvaluateParents = (remainingSteps = 1)

                    let nextSorterPoolSet = 
                        SorterPipeline.runGenerationStep 
                            mutator 
                            sorterCountPerPool
                            sorterChildCount
                            prioritizeNewMutants
                            distinctSorterHashes
                            sortableTest 
                            adjSorterEvalType
                            selectionMeasure
                            reEvaluateParents
                            currentSorterPoolSet
                            sortedFractionThreshold

                    // Only force a GC sweep every 50 generations to minimize overhead
                    if remainingSteps % 50 = 0 then
                        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                        GC.Collect(2, GCCollectionMode.Forced, true, true)

                    let moreUpdatedHistory = 
                        if (remainingSteps = 1) then 
                            let nextSnapshot = SorterPoolSetSummary.fromPoolSet nextSorterPoolSet
                            nextSnapshot :: updatedHistory
                        else 
                            updatedHistory

                    return! loop (remainingSteps - 1) nextSorterPoolSet moreUpdatedHistory
            }

        loop %genCount initialPoolSet []


    /// Extracts dataTableRecords out of the run result's intermediate summary history
    let toDataTableRecords (prefix: string) (srRes: sorterRunResult) : dataTableRecord array =
        srRes.IntermediateHistory
        |> Array.collect (fun poolSetSummary ->
            poolSetSummary
            |> SorterPoolSetSummary.toDataTableRecords prefix
        )