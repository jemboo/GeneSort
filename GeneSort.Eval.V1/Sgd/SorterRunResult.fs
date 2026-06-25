namespace GeneSort.Eval.V1

open FSharp.UMX
open System.Threading
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open System


/// Holds the combined results of the historical optimization run
type sorterRunResult = 
    private {
        _intermediateHistory: sorterPoolSetDescription array
        _finalPoolSet: sorterPoolSet
    }
    member this.FinalPoolSet with get() = this._finalPoolSet
    member this.IntermediateHistory with get() = this._intermediateHistory
    static member create 
                    (finalPoolSet:sorterPoolSet) 
                    (intermediateHistory:sorterPoolSetDescription []) =
            {
                _intermediateHistory = intermediateHistory
                _finalPoolSet = finalPoolSet
            }

module SorterRunResult =

    /// Recursively runs the generation loop step N times. 
    /// Saves optimized lightweight history tokens for intermediate steps, 
    /// and preserves the fully realized heavy pool data structure on the final step.
    let runEvolution 
            (genCount: int<generationNumber>)
            (mutator: sorterModelMutator)
            (mutantsPerSorter: int<sorterCount>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (collectNewSortableTests: bool)
            (selectionMeasure: sorterEvalMeasure)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (initialPoolSet: sorterPoolSet) : sorterRunResult =

        let rec loop (remainingSteps: int) (currentSet: sorterPoolSet) (historyAcc: sorterPoolSetDescription list) =
            if remainingSteps <= 0 then
                sorterRunResult.create
                        currentSet
                        (historyAcc |> List.rev |> List.toArray)
            else
                // 1. Take a lightweight snapshot of the current generation state before transitioning
                let currentSnapshot = SorterPoolSetDescription.fromPoolSet currentSet
                let updatedHistory = currentSnapshot :: historyAcc

                // 2. Advance the heavy state across the pipeline axis
                let nextSet = 
                    currentSet 
                    |> SorterPipeline.runGenerationStep 
                        mutator 
                        mutantsPerSorter 
                        sortableTest 
                        sorterEvalType 
                        collectNewSortableTests 
                        selectionMeasure 
                        sorterCountPerPool

                loop (remainingSteps - 1) nextSet updatedHistory

        loop %genCount initialPoolSet []


    /// Asynchronously runs the generation loops step N times using an asyncResult pipeline.
    /// Accumulates lightweight snapshots for intermediate states and forced GC compacting 
    /// over long execution steps.
    let runEvolutionAsync
            (genCount: int<generationNumber>)
            (mutator: sorterModelMutator)
            (sorterChildCount: int<sorterCount>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (collectNewSortableTests: bool)
            (selectionMeasure: sorterEvalMeasure)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (initialPoolSet: sorterPoolSet)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult, string>> =

        let rec loop 
                    (remainingSteps: int) 
                    (currentSet: sorterPoolSet) 
                    (historyAcc: sorterPoolSetDescription list) 
                    : Async<Result<sorterRunResult, string>> =
            asyncResult {
                // Safeguard against downstream timeouts or execution cancellations
                if cts.IsCancellationRequested then 
                    return! Error "runEvolutionAsync was cancelled."

                if remainingSteps <= 0 then
                    let finalResult = 
                        sorterRunResult.create 
                            currentSet 
                            (historyAcc |> List.rev |> List.toArray)
                    return finalResult
                else
                    log (sprintf "Starting evolution step. Remaining generations: %d" remainingSteps)

                    // 1. Snapshot the current generation state before applying structural changes
                    let currentSnapshot = SorterPoolSetDescription.fromPoolSet currentSet
                    let updatedHistory = currentSnapshot :: historyAcc

                    // 2. Advance the heavy state across your pipeline step axis.
                    // If your real SorterPipeline is still synchronous, wrap it inside asyncResult.retn
                    // log "Executing generation processing step..."
                    let nextSet = 
                        SorterPipeline.runGenerationStep 
                            mutator 
                            sorterChildCount 
                            sortableTest 
                            sorterEvalType 
                            collectNewSortableTests 
                            selectionMeasure 
                            sorterCountPerPool
                            currentSet

                    // 3. Clear transient allocations before executing the next generational depth
                    System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                    GC.Collect(2, GCCollectionMode.Forced, true, true)

                    return! loop (remainingSteps - 1) nextSet updatedHistory
            }

        loop %genCount initialPoolSet []

