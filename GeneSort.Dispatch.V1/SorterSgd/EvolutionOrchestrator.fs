namespace GeneSort.Dispatch.V1.SorterSgd

open FSharp.UMX
open System.Threading
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Db.V1
open GeneSort.Dispatch.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps
open System
open GeneSort.Model.Sorting.V1

module EvolutionOrchestrator =

    /// Asynchronously runs evolution step-by-step over genCount generations.
    /// Handles exponential sampling triggers for history accumulation, pool expansion, and DB persistence.
    let runEvolutionAsync
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (genStart: int<generationNumber>)
            (genCount: int<generationNumber>)
            (sorterCountCycle: int<sorterCountCycle>)
            (sorterCountCycleMultiplier: float<sorterCountCycleMultiplier>)
            (sorterPoolExpansionRate: int<sorterPoolExpansionRate>)
            (mutator: sorterModelMutator)
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sorterCountPerPool: int<sorterCountPerPool>)
            (sorterChildCount: int<sorterChildCount>)
            (sortableTest: sortableTest)
            (srtrEvalType: sorterEvalType)
            (selectionMeasure: sorterEvalMeasure)
            (initialPoolSet: sorterPoolSet)
            (collectNewSortableTests: bool<collectNewSortableTests>)
            (sortedFractionThreshold: float<sortedFraction>)
            (snapshotReportInterval: essData)
            (summaryReportInterval: essData)
            (sorterPoolSelectionIntervals: essData)
            (poolMeasure: sorterPoolMeasure)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult, string>> =

        let totalGenInt = int (genStart + genCount)
        
        // --- Exponential Frequency Triggers ---
        let targetGenerationsForPoolExpansion = 
                        EssData.getSampleSet sorterPoolSelectionIntervals totalGenInt

        let targetGenerationsForSaveRunResult = 
                        EssData.getSampleSet snapshotReportInterval totalGenInt

        let targetGenerationsForSummaryReport = 
                        EssData.getSampleSet summaryReportInterval totalGenInt


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

                    // Dynamic Periodic Variation for sorterCountPerPool
                    let scm = float %sorterCountCycleMultiplier
                    let scPP = float %sorterCountPerPool
                    let multiplier = if ((%currentGen / %sorterCountCycle) % 2 = 0) then (1.0 / scm) else (2.0 - 1.0 / scm)
                    let currentSorterCountPerPool : int<sorterCountPerPool> = UMX.tag (int (scPP * multiplier))

                    // Evaluate Triggers
                    let shouldSummaryReport = Set.contains %currentGen targetGenerationsForSummaryReport
                    let shouldSaveRunResult = Set.contains %currentGen targetGenerationsForSaveRunResult
                    let shouldExpandPools = Set.contains %currentGen targetGenerationsForPoolExpansion

                    if shouldSummaryReport then
                        log (sprintf "Starting evolution step. Generation %d of %d" currentGen totalGen)

                    // 1. Snapshot summary before applying structural changes
                    let updatedSorterPoolSetSummary = 
                        if shouldSummaryReport then 
                            let currentSnapshot = SorterPoolSetSummary.fromPoolSet currentSorterPoolSet
                            currentSnapshot :: historyAcc
                        else 
                            historyAcc

                    // 2. Perform Pool Expansion on exponential milestone
                    let poolSetForStep =
                        if shouldExpandPools && (%currentGen > 0) && (sorterPoolExpansionRate > 1<sorterPoolExpansionRate>) then
                            log (sprintf "Expanding pools at Generation %d (Rate: %d)..." %currentGen %sorterPoolExpansionRate)
                            currentSorterPoolSet
                            |> SorterPoolSet.trimPools sorterPoolExpansionRate poolMeasure
                            |> SorterPoolSet.expandPools sorterPoolExpansionRate
                        else
                            currentSorterPoolSet

                    // 3. Step Evolution
                    let adjSorterEvalType = if (remainingSteps = 1) then sorterEvalType.V2 else srtrEvalType
                    let reEvaluateParents = (remainingSteps % 10 = 0)

                    let nextSorterPoolSet = 
                        SorterPipeline.runGenerationStepDebug 
                            mutator 
                            currentSorterCountPerPool
                            sorterChildCount
                            prioritizeNewMutants
                            distinctSorterHashes
                            sortableTest 
                            adjSorterEvalType
                            selectionMeasure
                            reEvaluateParents
                            poolSetForStep
                            collectNewSortableTests
                            sortedFractionThreshold

                    // 4. Save RunResult to Database on exponential milestone
                    let! historyAccNext = 
                        asyncResult {
                            if shouldSaveRunResult && (%currentGen > 0) then
                                let currentRunResult = 
                                    sorterRunResult.create 
                                        nextSorterPoolSet 
                                        (updatedSorterPoolSetSummary |> List.rev |> List.toArray)

                                let stepRp = rp.WithGenerationCurrent(Some currentGen)
            
                                let! qp = 
                                    host.RunDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
                                    |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                                log (sprintf "Saving SorterRunResult checkpoint at Generation %d..." %currentGen)
            
                                // saveAsync matches Async<Result<unit, string>> so do! directly unrolls it
                                do! host.RunDb.saveAsync qp (currentRunResult |> outputData.SorterRunResult) allowOverwrite

                                // Reset accumulator on success
                                return []
                            else
                                return updatedSorterPoolSetSummary
                        }


                    // Forced GC Compacting
                    if remainingSteps % 50 = 0 then
                        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                        GC.Collect(2, GCCollectionMode.Forced, true, true)

                    return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext
            }

        loop %genCount initialPoolSet []

