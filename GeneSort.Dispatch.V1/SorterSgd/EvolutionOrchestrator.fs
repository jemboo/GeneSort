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
            (sortedFractionThreshold: float<sortedFraction>)
            (snapshotReportInterval: essData)
            (summaryReportInterval: essData)
            (sorterPoolSelectionIntervals: essData)
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
                            |> SorterPoolSet.trimPools sorterPoolExpansionRate selectionMeasure
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
                            sortedFractionThreshold

                    // 4. Save RunResult to Database on exponential milestone
                    if shouldSaveRunResult && (%currentGen > 0) then
                        let currentRunResult = 
                            sorterRunResult.create 
                                nextSorterPoolSet 
                                (updatedSorterPoolSetSummary |> List.rev |> List.toArray)

                        let stepRp = rp.WithGenerationCurrent(Some currentGen)
                        let! qp = 
                            host.RunDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                        log (sprintf "Saving SorterRunResult checkpoint at Generation %d (Id: %s)..." %currentGen (string qp.Id))
                        do! host.RunDb.saveAsync qp (currentRunResult |> outputData.SorterRunResult) allowOverwrite

                    // Forced GC Compacting
                    if remainingSteps % 50 = 0 then
                        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                        GC.Collect(2, GCCollectionMode.Forced, true, true)

                    return! loop (remainingSteps - 1) nextSorterPoolSet updatedSorterPoolSetSummary
            }

        loop %genCount initialPoolSet []











    //let saveCheckpoint
    //        (host: IRunHost)
    //        (currentRp: runParameters)
    //        (currentGen: int<generationNumber>)
    //        (runResult: sorterRunResult)
    //        (allowOverwrite: bool<allowOverwrite>)
    //        (log: string -> unit) =
    //    asyncResult {
    //        let stepRp = currentRp.WithGenerationCurrent(Some currentGen)
    //        let! qp = 
    //            host.RunDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
    //            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

    //        log (sprintf "Saving SorterRunResult for generation %d - Id: %s" %currentGen (string qp.Id))
    //        do! host.RunDb.saveAsync qp (runResult |> outputData.SorterRunResult) allowOverwrite
    //        return stepRp
    //    }

    //let runSlicesInLoop
    //        (host: IRunHost)
    //        (rp: runParameters)
    //        (genFirst: int<generationNumber>)
    //        (genLast: int<generationNumber>)
    //        (genSliceInterval: int<generationNumber>)
    //        (genReportInterval: int<generationNumber>)
    //        (measure: sorterEvalMeasure)
    //        (sorterPoolExpansionRate: int<sorterPoolExpansionRate>)
    //        (initialSeedPoolSet: sorterPoolSet)
    //        (allowOverwrite: bool<allowOverwrite>)
    //        (cts: CancellationToken)
    //        (log: string -> unit)
    //        (runSliceAsync: int<generationNumber> -> int<generationNumber> -> sorterPoolSet -> Async<Result<sorterRunResult, string>>) 
    //        : Async<Result<runParameters, string>> =

    //    let rec stepLoop 
    //                (currentGen: int<generationNumber>) 
    //                (currentPoolSet: sorterPoolSet) 
    //                (currentRp: runParameters) =

    //        asyncResult {
    //            if currentGen >= genLast then
    //                return currentRp
    //            else
    //                do! checkCancellation cts

    //                // 1. Calculate step size driven by the reporting interval (or remaining count)
    //                let stepSize = min genReportInterval (genLast - currentGen)
    //                let nextGen = currentGen + stepSize

    //                // 2. Pool Expansion: Run every `genSliceInterval` generations (or at start gen 0)
    //                let expandedPoolSet =
    //                    if (%currentGen > 0) && (%currentGen % %genSliceInterval = 0) && (sorterPoolExpansionRate > 1<sorterPoolExpansionRate>) then
    //                        log (sprintf "Expanding pools at Generation %d (Rate: %d)..." %currentGen %sorterPoolExpansionRate)
    //                        currentPoolSet
    //                        |> SorterPoolSet.trimPools sorterPoolExpansionRate measure
    //                        |> SorterPoolSet.expandPools sorterPoolExpansionRate
    //                    else
    //                        currentPoolSet

    //                log (sprintf "Stepping evolution: Generation %d -> %d..." %currentGen %nextGen)
    //                let! runResult = runSliceAsync currentGen stepSize expandedPoolSet
    //                do! checkCancellation cts

    //                // 3. Database Persistence: Run every `genReportInterval` generations (or at run end)
    //                let! updatedRp =
    //                    if (%nextGen % %genReportInterval = 0) || (nextGen >= genLast) then
    //                        saveCheckpoint host currentRp nextGen runResult allowOverwrite log
    //                    else
    //                        asyncResult { return currentRp.WithGenerationCurrent(Some nextGen) }

    //                return! stepLoop nextGen runResult.FinalPoolSet updatedRp
    //        }

    //    stepLoop genFirst initialSeedPoolSet rp

