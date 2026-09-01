namespace GeneSort.Dispatch.V1.SorterSgd

open FSharp.UMX
open System.Threading
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Db.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps
open System
open GeneSort.Model.Sorting.V1

module EvolutionOrchestrator =

    let inline private triggerCompactingGC () =
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- 
            System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
        GC.Collect(2, GCCollectionMode.Forced, true, true)

    let runEvolutionAsync
            (genDb: IGeneSortGenDb)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (initialPoolSet: sorterPoolSet)
            (sortableTest: sortableTest)
            (mutator: sorterModelMutator)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterPoolSet, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            // Mandatory parameters
            let! genStart = rp.GetGenerationCurrent() |> Result.ofOption "Missing generationCurrent"
            let! genIntervalCount = rp.GetGenerationIntervalCount() |> Result.ofOption "Missing genIntervalCount."
            let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
            let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
            let! sorterCountPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sorterCountPerPool."
            let! selectedSorterCountPerPool = rp.GetSelectedSorterCountPerPool() |> Result.ofOption "Missing selectedSorterCountPerPool."
            let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count."
            let! srtrEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
            let! collectNewSortableTests = rp.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests."
            let! sortedFractionThreshold = rp.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."

            // Optional parameters for Dynamic Variation and Pool Expansion
            let optSorterCountCycle = rp.GetSorterCountCycle()
            let optSorterCountCycleMultiplier = rp.GetSorterCountCycleMultiplier()
            let optSorterPoolExpansionRate = rp.GetSorterPoolExpansionRate()
            let optSorterPoolSelectionIntervals = rp.GetSorterPoolSelectionIntervals()
            let optPoolMeasure = rp.GetSorterPoolMeasure()

            // 1. Extract save configs
            let saveIntervals = genDb.getGenSaveIntervals()
            let subIntervals = genDb.getGenSaveSubIntervals()

            // 2. Fetch the minimal sample set starting from genStart
            let requiredCount = int genIntervalCount

            let targetSamples = 
                SamplingConfig.getSampleSetWithMinBound saveIntervals (%genStart - 1) requiredCount
                |> Set.toArray
                |> Array.sort

            if targetSamples.Length < requiredCount then
                return! Error (sprintf "Target generation sequence ended early: requested %d intervals from %d, but only obtained %d."
                                        %genIntervalCount %genStart targetSamples.Length)
            else
                let targetGenInt = targetSamples.[targetSamples.Length - 1]
                let totalGen = %targetGenInt : int<generationNumber>

                // --- Frequency Triggers ---
                let targetGenerationsForPoolExpansion = 
                    match optSorterPoolSelectionIntervals with
                    | Some intervals -> SamplingConfig.getSampleSetMaxBound intervals targetGenInt
                    | None -> Set.empty

                let targetGenerationsForSaveResults = 
                    SamplingConfig.getSampleSetMaxBound saveIntervals targetGenInt

                let targetGenerationsForSummaryReport = 
                    SamplingConfig.getSampleSetMaxBound subIntervals targetGenInt

                // Condensed Optional Features Log
                let countLog = 
                    match optSorterCountCycle, optSorterCountCycleMultiplier with
                    | Some c, Some m -> sprintf "ENABLED (Cycle: %d, Mult: %.2f)" %c (float %m)
                    | _ -> "DISABLED (Static)"

                let poolLog = 
                    match optSorterPoolExpansionRate, optSorterPoolSelectionIntervals, optPoolMeasure with
                    | Some r, Some i, Some m -> sprintf "ENABLED (Rate: %d, Intervals: %A, Measure: %A)" %r i m
                    | _ ->
                        let missing = 
                            [ if optSorterPoolExpansionRate.IsNone then yield "Rate"
                              if optSorterPoolSelectionIntervals.IsNone then yield "Intervals"
                              if optPoolMeasure.IsNone then yield "Measure" ] |> String.concat ","
                        sprintf "DISABLED (Missing: %s)" missing

                log (sprintf "  [Optional Features] Dynamic Count: %s | Pool Expansion: %s" countLog poolLog)

                // Recursive loop carrying evalBinsSetAcc and active lineage tracking map
                let rec loop 
                        (remainingSteps: int)
                        (currentSorterPoolSet: sorterPoolSet)
                        (historyAcc: sorterPoolSetSummary list)
                        (sorterPoolBinsSetAcc: sorterPoolBinsSet list)
                        (runningMap: runningMemberHistoryMap)
                        : Async<Result<sorterPoolSet, string>> =

                    asyncResult {
                        // Cooperative cancellation evaluation at top of loop
                        if cts.IsCancellationRequested then
                            return! Error "runEvolutionAsync execution was cancelled."
                        elif remainingSteps < 0 then
                            return currentSorterPoolSet
                        else
                            let currentGen = totalGen - %remainingSteps

                            // Dynamic Periodic Variation for sorterCountPerPool
                            let currentSorterCountPerPool : int<sorterCountPerPool> =
                                match optSorterCountCycle, optSorterCountCycleMultiplier with
                                | Some cycle, Some multiplierVal when %cycle > 0 ->
                                    let scm = float %multiplierVal
                                    let scPP = float %sorterCountPerPool
                                    let multiplier = if ((%currentGen / %cycle) % 2 = 0) then (1.0 / scm) else (2.0 - 1.0 / scm)
                                    UMX.tag (int (scPP * multiplier))
                                | _ ->
                                    sorterCountPerPool

                            // Triggers
                            let shouldSummaryReport = Set.contains %currentGen targetGenerationsForSummaryReport
                            let shouldSaveResults = Set.contains %currentGen targetGenerationsForSaveResults

                            let shouldExpandPools = 
                                match optSorterPoolExpansionRate, optPoolMeasure with
                                | Some expansionRate, Some _ when expansionRate > 1<sorterPoolExpansionRate> ->
                                    Set.contains %currentGen targetGenerationsForPoolExpansion && (%currentGen > 0)
                                | _ -> false

                            if shouldSummaryReport then
                                log (sprintf "Starting evolution step. Generation %d of %d" currentGen totalGen)

                            // Snapshot summary before applying structural changes
                            let updatedSorterPoolSetSummary = 
                                if shouldSummaryReport then 
                                    let currentSnapshot = SorterPoolSetSummary.fromPoolSet currentSorterPoolSet
                                    currentSnapshot :: historyAcc
                                else 
                                    historyAcc

                            // Perform Pool Expansion on milestone
                            let poolSetForStep =
                                if shouldExpandPools then
                                    let expansionRate = optSorterPoolExpansionRate.Value
                                    let poolMeasure = optPoolMeasure.Value
                                    log (sprintf "Expanding pools at Generation %d (Rate: %d)..." %currentGen %expansionRate)
                                    currentSorterPoolSet
                                    |> SorterPoolSet.trimPools expansionRate poolMeasure
                                    |> SorterPoolSet.expandPools expansionRate
                                else
                                    currentSorterPoolSet

                            // Step Evolution
                            let reEvaluateParents = (remainingSteps % 10 = 0)

                            let nextSorterPoolSet = 
                                SorterPipeline.runGenerationStepDebug
                                    mutator 
                                    currentSorterCountPerPool
                                    selectedSorterCountPerPool
                                    sorterChildCount
                                    prioritizeNewMutants
                                    distinctSorterHashes
                                    sortableTest 
                                    evalType
                                    srtrEvalMeasure
                                    reEvaluateParents
                                    poolSetForStep
                                    collectNewSortableTests
                                    sortedFractionThreshold

                            // Track all generated members using direct parent reference
                            //let updatedRunningHistoryMap = 
                            //    runningMap |> RunningMemberHistoryMap.updateFromPoolSet currentGen nextSorterPoolSet

                            // Accumulate sorterPoolEvalBinsSet at summary report frequency
                            let updatedEvalBinsSetAcc =
                                if shouldSummaryReport then
                                    let binsSetId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsSetId>
                                    let currentBinsSet = sorterPoolBinsSet.create binsSetId nextSorterPoolSet
                                    currentBinsSet :: sorterPoolBinsSetAcc
                                else
                                    sorterPoolBinsSetAcc

                            // Save RunResult, SorterPoolEvalBinsSetCollection, and SorterPoolSetHistory on shouldSaveRunResult
                            let! (historyAccNext, evalBinsSetAccNext, runningMemberHistoryMapNext) = 
                                asyncResult {

                                    if shouldSaveResults && (%currentGen > 0) then
                                        let stepRp = rp.WithGenerationCurrent(Some currentGen)
                                        log (sprintf "Saving SorterPoolSet and others at Generation %d..." %currentGen)

                                        // Save SorterPoolSet
                                        let! qpSummaries = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolSet "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolSet."
                                        do! genDb.saveAsync qpSummaries (nextSorterPoolSet |> outputData.SorterPoolSet) allowOverwrite


                                        // Save SorterPoolSetSummaries
                                        let currentSummaries = updatedSorterPoolSetSummary |> List.toArray
                                        let! qpSummaries = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolSetSummaries "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolSetSummaries."
                                        do! genDb.saveAsync qpSummaries (currentSummaries |> outputData.SorterPoolSetSummaries) allowOverwrite


                                        // Save SorterPoolBinsSetSeries
                                        let collectionId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsSetSeriesId>
                                        let evalBinsCollection = 
                                            sorterPoolBinsSetSeries.create collectionId (updatedEvalBinsSetAcc |> List.rev)
                                        let! qpBinsSeries = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolBinsSetSeries "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolEvalBinsSetCollection."
                                        do! genDb.saveAsync qpBinsSeries (evalBinsCollection |> outputData.SorterPoolBinsSetSeries) allowOverwrite


                                        // Prune dead lineages and generate SorterPoolSetHistory
                                        //let poolSetHistory, prunedRunningMap = 
                                        //    SorterPoolSetHistory.pruneAndCreateFromPoolSet currentGen nextSorterPoolSet updatedRunningHistoryMap
                                        //let! qpHistory = 
                                        //    genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolSetHistory "")
                                        //    |> Result.ofOption "Failed to create QueryParams for SorterPoolSetHistory."
                                        //do! genDb.saveAsync qpHistory (poolSetHistory |> outputData.SorterPoolSetHistory) allowOverwrite

                                       // return ([], [], prunedRunningMap)
                                        return ([], [], RunningMemberHistoryMap.empty)
                                    else
                                        //return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, updatedRunningHistoryMap)
                                        return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, RunningMemberHistoryMap.empty)
                                }

                            return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext RunningMemberHistoryMap.empty
                           // return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext []
                    }

                // Execute loop with empty initial states
                return! loop (%totalGen - %genStart) initialPoolSet [] [] RunningMemberHistoryMap.empty
        }