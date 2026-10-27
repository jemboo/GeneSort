namespace GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Soss

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Soss
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage


module EvoOrch_Soss =

    let inline private triggerCompactingGC () =
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- 
            System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
        GC.Collect(2, GCCollectionMode.Forced, true, true)

    let runSossEvolutionAsync
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (path: evoPath)
            (state: evoProgress)
            (allowOverwrite: bool<allowOverwrite>)
            (initialPoolSet: sorterPoolSet_Soss)
            (sortableTestPartitions: partitionOfSortableTests)
            (prefix: ceBlock)
            (mutator: sorterModelMutator)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterPoolSet_Soss, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            // Mandatory parameters
            let! genIntervalLast = path.GetGenerationIntervalLast() |> Result.ofOption "Missing generationIntervalLast."
            let! prioritizeNewMutants = path.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
            let! distinctSorterHashes = path.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
            let! sorterCountPerPool = path.GetSorterCountPerPool() |> Result.ofOption "Missing sorterCountPerPool."
            let! selectedSorterCountPerPool = path.GetSelectedSorterCountPerPool() |> Result.ofOption "Missing selectedSorterCountPerPool."
            let! sorterChildCount = path.GetSorterChildCount() |> Result.ofOption "Missing sorter child count."
            let! generationsPerSubset = path.GetGenerationsPerSubset() |> Result.ofOption "Missing GenerationsPerSubset."
            do! if %generationsPerSubset > 0 then Ok () else Error "GenerationsPerSubset must be greater than zero."
            let! generationsPerMergedSet = path.GetGenerationsPerMergedSet() |> Result.ofOption "Missing GenerationsPerMergedSet."
            do! if %generationsPerMergedSet > 0 then Ok () else Error "GenerationsPerMergedSet must be greater than zero."
            let! srtrEvalMeasure = path.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
            let! collectNewSortableTests = path.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests."
            let! sortedFractionThreshold = path.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."

            // Optional parameters for Dynamic Variation and Pool Expansion
            let optSorterCountCycle = path.GetSorterCountCycle()
            let optSorterCountCycleMultiplier = path.GetSorterCountCycleMultiplier()
            let optSorterPoolExpansionRate = path.GetSorterPoolExpansionRate()
            let optSorterPoolSelectionIntervals = path.GetSorterPoolSelectionIntervals()
            let optPoolMeasure = path.GetSorterPoolMeasure()
            let genStart = initialPoolSet.GenerationNumber
            let! targetGenInt =
                GenIntervalConfig.getSampleAtIndex saveIntervals %genIntervalLast
                |> Result.ofOption (sprintf "generationIntervalLast index %d is not available in saveIntervals '%s'." %genIntervalLast saveIntervals.Name)

            // Resume from the latest completed interval and stop at the configured interval index.
            if targetGenInt < %genStart then
                return! Error (sprintf "generationIntervalLast index %d resolves to generation %d, before the resumed generation %d." %genIntervalLast targetGenInt %genStart)
            else
                let totalGen = %targetGenInt : int<generationNumber>

                // --- Frequency Triggers ---
                let targetGenerationsForPoolExpansion = 
                    match optSorterPoolSelectionIntervals with
                    | Some intervals -> GenIntervalConfig.getSampleSetMaxBound intervals targetGenInt
                    | None -> Set.empty

                let targetGenerationsForSaveResults = 
                    GenIntervalConfig.getSampleSetMaxBound saveIntervals targetGenInt
                    |> Set.add targetGenInt

                let targetGenerationsForSummaryReport = 
                    GenIntervalConfig.getSampleSetMaxBound subIntervals targetGenInt

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
                        (currentSorterPoolSet: sorterPoolSet_Soss)
                        (historyAcc: sorterPoolSetSummary_Soss list)
                        (sorterPoolBinsSetAcc: sorterPoolBinsSet_Soss list)
                        (runningMap: mhMap_Soss)
                        : Async<Result<sorterPoolSet_Soss, string>> =

                    asyncResult {
                        // Cooperative cancellation evaluation at top of loop
                        if cts.IsCancellationRequested then
                            return! Error "runEvolutionAsync execution was cancelled."
                        elif remainingSteps <= 0 then
                            return currentSorterPoolSet
                        else
                            // Save the generation produced by this step, not the incoming one.
                            let currentGen = currentSorterPoolSet.GenerationNumber + 1<generationNumber>

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

                            // Perform Pool Expansion on milestone
                            let poolSetForStep =
                                if shouldExpandPools then
                                    let expansionRate = optSorterPoolExpansionRate.Value
                                    let poolMeasure = optPoolMeasure.Value
                                    log (sprintf "Expanding pools at Generation %d (Rate: %d)..." %currentGen %expansionRate)
                                    let subsetId =
                                        sortableTestPartitions.SortableTests |> Map.keys |> Seq.toArray
                                        |> CommonSgdSoss.subsetIdForGeneration generationsPerSubset generationsPerMergedSet currentSorterPoolSet.GenerationNumber
                                    currentSorterPoolSet
                                    |> Pipeline_Soss.evaluateRequiredPartitions sortableTestPartitions prefix evalType false
                                        collectNewSortableTests subsetId false
                                    |> SorterPoolSet_Soss.trimPoolsForSubset subsetId expansionRate poolMeasure
                                    |> SorterPoolSet_Soss.expandPools expansionRate
                                else
                                    currentSorterPoolSet

                            // Step Evolution

                            let nextSorterPoolSet = 
                                Pipeline_Soss.runGenerationStepSoss
                                    mutator 
                                    currentSorterCountPerPool
                                    selectedSorterCountPerPool
                                    sorterChildCount
                                    prioritizeNewMutants
                                    distinctSorterHashes
                                    sortableTestPartitions
                                    generationsPerSubset
                                    generationsPerMergedSet
                                    prefix
                                    evalType
                                    srtrEvalMeasure
                                    poolSetForStep
                                    shouldSummaryReport
                                    collectNewSortableTests
                                    sortedFractionThreshold

                            // Summaries, bins, and checkpoints describe the same completed
                            // generation, including expansion, CE adjustment and selection.
                            let updatedSorterPoolSetSummary =
                                if shouldSummaryReport then
                                    SorterPoolSetSummary_Soss.fromPoolSet nextSorterPoolSet :: historyAcc
                                else historyAcc

                            // Track all generated members using direct parent reference
                            //let updatedRunningHistoryMap = 
                            //    runningMap |> RunningMemberHistoryMap.updateFromPoolSet currentGen nextSorterPoolSet

                            // Accumulate sorterPoolEvalBinsSet at summary report frequency
                            let updatedEvalBinsSetAcc =
                                if shouldSummaryReport then
                                    let binsSetId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsSetId>
                                    let currentBinsSet = sorterPoolBinsSet_Soss.create binsSetId nextSorterPoolSet
                                    currentBinsSet :: sorterPoolBinsSetAcc
                                else
                                    sorterPoolBinsSetAcc

                            // Save RunResult, SorterPoolEvalBinsSetCollection, and SorterPoolSetHistory on shouldSaveRunResult
                            let! (historyAccNext, evalBinsSetAccNext, runningMemberHistoryMapNext) = 
                                asyncResult {

                                    if shouldSaveResults && (%currentGen > 0) then
                                        log (sprintf "Saving SorterPoolSet and others at Generation %d..." %currentGen)

                                        // Save SorterPoolSet
                                        let! qpSorterPoolSet = 
                                            EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolSet "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolSet."
                                        do! genDb.saveAsync geneSortDataMode.Results qpSorterPoolSet (nextSorterPoolSet |> sorterPoolSet.Soss |> outputData.SorterPoolSet) allowOverwrite


                                        // Save SorterPoolSetSummaries
                                        let! qpSummaries = 
                                            EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolSetSummarySet "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolSetSummarySet."
                                        let spsstID = qpSummaries.Id |> UMX.cast<queryParamsId, sorterPoolSetSummarySetId>
                                        let currentSummaries = updatedSorterPoolSetSummary |> List.toArray
                                        let sorterPoolSetSummarySet = spSummarySet_Soss.create spsstID currentGen currentSummaries
                                        do! genDb.saveAsync geneSortDataMode.Results qpSummaries (sorterPoolSetSummarySet |> spSummarySet.Soss |> outputData.SorterPoolSetSummarySet) allowOverwrite


                                        // Save SorterPoolBinsSetSeries
                                        let collectionId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsSetSeriesId>
                                        let evalBinsCollection = 
                                            sorterPoolBinsSetSeries_Soss.create collectionId (updatedEvalBinsSetAcc |> List.rev)
                                        let! qpBinsSeries = 
                                            EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolBinsSetSeries "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolEvalBinsSetCollection."
                                        do! genDb.saveAsync geneSortDataMode.Results qpBinsSeries (evalBinsCollection |> sorterPoolBinsSetSeries.Soss |> outputData.SorterPoolBinsSetSeries) allowOverwrite


                                        // Prune dead lineages and generate SorterPoolSetHistory
                                        //let poolSetHistory, prunedRunningMap = 
                                        //    SorterPoolSetHistory.pruneAndCreateFromPoolSet currentGen nextSorterPoolSet updatedRunningHistoryMap
                                        //let! qpHistory = 
                                        //    EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolSetHistory "")
                                        //    |> Result.ofOption "Failed to create QueryParams for SorterPoolSetHistory."
                                        //do! genDb.saveAsync geneSortDataMode.Results qpHistory (poolSetHistory |> spsh.Soss |> outputData.SorterPoolSetHistory) allowOverwrite

                                       // return ([], [], prunedRunningMap)
                                        return ([], [], MhMap_Soss.empty)
                                    else
                                        //return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, updatedRunningHistoryMap)
                                        return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, MhMap_Soss.empty)
                                }

                            return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext MhMap_Soss.empty
                           // return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext []
                    }

                // Execute loop with empty initial states
                return! loop (%totalGen - %genStart) initialPoolSet [] [] MhMap_Soss.empty
        }
