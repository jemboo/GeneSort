namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open System.Threading
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open System
open GeneSort.Eval.V1
open GeneSort.Sorting


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
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sorterChildCount: int<sorterChildCount>)
            (sortableTest: sortableTest)
            (sorterEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (celengthSelector: int<generationNumber> -> int<ceLength> option)
            (initialPoolSet: sorterPoolSet) : sorterRunResult =

        let rec loop (remainingSteps: int) (currentSet: sorterPoolSet) (historyAcc: sorterPoolSetDescription list) =
            if remainingSteps <= 0 then
                sorterRunResult.create
                        currentSet
                        (historyAcc |> List.rev |> List.toArray)
            else
                // Calculate the current active generation (assuming 0-indexed start since genStart isn't provided)
                let currentGen = (int genCount - remainingSteps) |> UMX.tag<generationNumber>
                let maxCeCount = celengthSelector currentGen

                // 1. Take a lightweight snapshot of the current generation state before transitioning
                let currentSnapshot = SorterPoolSetDescription.fromPoolSet currentSet
                let updatedHistory = currentSnapshot :: historyAcc

                // 2. Advance the heavy state across the pipeline axis
                let reEvaluateParents = false
                let nextSet = 
                    SorterPipeline.runGenerationStep 
                        mutator  
                        sorterCountPerPool
                        sorterChildCount
                        prioritizeNewMutants
                        distinctSorterHashes
                        sortableTest 
                        sorterEvalType 
                        selectionMeasure 
                        maxCeCount
                        reEvaluateParents
                        currentSet

                loop (remainingSteps - 1) nextSet updatedHistory

        loop %genCount initialPoolSet []


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
            (celengthSelector: int<generationNumber> -> int<ceLength> option)
            (initialPoolSet: sorterPoolSet)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult, string>> =

        // --- Exponential Frequency Configuration ---
        let totalGenInt = int (genStart + genCount)
        let targetGenerations = MathUtils.expSampler 1 totalGenInt MathUtils.cSample100K

        let rec loop 
                    (remainingSteps: int) 
                    (currentSorterPoolSet: sorterPoolSet) 
                    (historyAcc: sorterPoolSetDescription list) 
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
                    let maxCeCount = celengthSelector currentGen
                    
                    // Look up if the current generation is an exponential milestone
                    let shouldReport = (Set.contains (int currentGen) targetGenerations)

                    if shouldReport then
                        log (sprintf "Starting evolution step. Generation %d of %d" currentGen totalGen)

                    // 1. Snapshot the current generation state before applying structural changes
                    let updatedHistory = 
                        if shouldReport then 
                            let currentSnapshot = SorterPoolSetDescription.fromPoolSet currentSorterPoolSet
                            currentSnapshot :: historyAcc
                        else 
                            historyAcc

                    let adjSorterEvalType = if (remainingSteps = 1) then sorterEvalType.V2 else srtrEvalType
                    //let reEvaluateParents = true
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
                            maxCeCount
                            reEvaluateParents
                            currentSorterPoolSet
                            

                    // Only force a GC sweep every 50 generations to minimize overhead
                    if remainingSteps % 50 = 0 then
                        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                        GC.Collect(2, GCCollectionMode.Forced, true, true)

                    let moreUpdatedHistory = 
                        if (remainingSteps = 1) then 
                            let nextSnapshot = SorterPoolSetDescription.fromPoolSet nextSorterPoolSet
                            nextSnapshot :: updatedHistory
                        else 
                            updatedHistory

                    return! loop (remainingSteps - 1) nextSorterPoolSet moreUpdatedHistory
            }


        loop %genCount initialPoolSet []



    // extracts dataTableRecords out of sorterRunResult.IntermediateHistory only
    let toDataTableRecords (prefix: string) (srRes: sorterRunResult) : dataTableRecord array =
        srRes.IntermediateHistory
        |> Array.collect (fun poolSetDesc -> 
            poolSetDesc 
            |> SorterPoolSetDescription.toDataTableRecords prefix
        )
