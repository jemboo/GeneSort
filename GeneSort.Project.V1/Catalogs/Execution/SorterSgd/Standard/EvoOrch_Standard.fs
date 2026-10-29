namespace GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Standard

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.Project.V1.Storage


module EvoOrch_Standard =

    let inline private triggerCompactingGC () =
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- 
            System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
        GC.Collect(2, GCCollectionMode.Forced, true, true)

    let runStandardEvolutionAsync
            (genDb: IGeneSortDb)
            (saveIntervals: genIntervalConfig)
            (subIntervals: genIntervalConfig)
            (path: evoPath)
            (state: evoProgress)
            (allowOverwrite: bool<allowOverwrite>)
            (initialPoolSet: sorterPoolSet_Standard)
            (sortableTests: sortableTests)
            (prefix: ceBlock)
            (mutator: sorterModelMutator)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterPoolSet_Standard, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            // Mandatory parameters
            do! ParameterValidation.validateValues SgdParameterValidation.standardEvolutionKeys path.Settings |> ParameterValidation.toResult
            let! genIntervalLast = path.GetGenerationIntervalLast() |> Result.ofOption "Missing generationIntervalLast."
            let! prioritizeNewMutants = path.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
            let! distinctSorterHashes = path.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
            let! sorterCountPerPool = path.GetSorterCountPerPool() |> Result.ofOption "Missing sorterCountPerPool."
            let! selectedSorterCountPerPool = path.GetSelectedSorterCountPerPool() |> Result.ofOption "Missing selectedSorterCountPerPool."
            let! sorterChildCount = path.GetSorterChildCount() |> Result.ofOption "Missing sorter child count."
            let! adjustCeLengthFrequency = path.GetAdjustCeLengthFrequency() |> Result.ofOption "Missing AdjustCeLengthFrequency."
            let! srtrEvalMeasure = path.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
            let! collectNewSortableTests = path.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests."
            let! sortedFractionThreshold = path.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."

            // Optional parameters for Dynamic Variation and Pool Expansion
            let optSorterCountCycle = path.GetSorterCountCycle()
            let optSorterCountCycleMultiplier = path.GetSorterCountCycleMultiplier()
            let optSorterPoolExpansionRate = path.GetSorterPoolExpansionRate()
            let optSorterPoolSelectionIntervals = path.GetSorterPoolSelectionIntervals()
            let optPoolMeasure = path.GetSorterPoolMeasure()
            do! ParameterValidation.intervalIndex saveIntervals %genIntervalLast
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

                let targetGenerationsForSnapshots =
                    GenIntervalConfig.getSampleSetMaxBound saveIntervals targetGenInt
                    |> Set.add targetGenInt

                let targetGenerationsForSaveResults =
                    path.CheckpointIntervals
                    |> Option.map (fun config -> GenIntervalConfig.getSampleSetMaxBound config targetGenInt)
                    |> Option.defaultValue Set.empty
                    |> Set.union targetGenerationsForSnapshots

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
                        (currentSorterPoolSet: sorterPoolSet_Standard)
                        (historyAcc: sorterPoolSetSummary_Standard list)
                        (sorterPoolBinsSetAcc: sorterPoolBinsSet_Standard list)
                        (runningMap: mhMap_Standard)
                        : Async<Result<sorterPoolSet_Standard, string>> =

                    asyncResult {
                        // Cooperative cancellation evaluation at top of loop
                        if cts.IsCancellationRequested then
                            return! Error "runEvolutionAsync execution was cancelled."
                        elif remainingSteps <= 0 then
                            return currentSorterPoolSet
                        else
                            // Label each checkpoint with the generation produced by this step.
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
                            let isSnapshot = Set.contains %currentGen targetGenerationsForSnapshots

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
                                    let currentSnapshot = SorterPoolSetSummary_Standard.fromPoolSet currentSorterPoolSet
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
                                    |> SorterPoolSet_Standard.trimPools expansionRate poolMeasure
                                    |> SorterPoolSet_Standard.expandPools expansionRate
                                else
                                    currentSorterPoolSet

                            // Step Evolution

                            let nextSorterPoolSet = 
                                Pipeline_Standard.runGenerationStepDebug
                                    mutator 
                                    currentSorterCountPerPool
                                    selectedSorterCountPerPool
                                    sorterChildCount
                                    prioritizeNewMutants
                                    distinctSorterHashes
                                    sortableTests
                                    prefix
                                    evalType
                                    srtrEvalMeasure
                                    adjustCeLengthFrequency
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
                                    let currentBinsSet = sorterPoolBinsSet_Standard.create binsSetId nextSorterPoolSet
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


                                        // Save SorterPoolSetSummaries
                                        let! qpSummaries = 
                                            EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolSetSummarySet "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolSetSummarySet."
                                        let spsstID = qpSummaries.Id |> UMX.cast<queryParamsId, sorterPoolSetSummarySetId>
                                        let currentSummaries = updatedSorterPoolSetSummary |> List.toArray
                                        let sorterPoolSetSummarySet = spSummarySet_Standard.create spsstID currentGen currentSummaries


                                        // Save SorterPoolBinsSetSeries
                                        let collectionId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsSetSeriesId>
                                        let evalBinsCollection = 
                                            sorterPoolBinsSetSeries_Standard.create collectionId (updatedEvalBinsSetAcc |> List.rev)
                                        let! qpBinsSeries = 
                                            EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolBinsSetSeries "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolEvalBinsSetCollection."
                                        do! EvolutionFiles.saveRetainedCheckpoint genDb path isSnapshot [|
                                            qpSorterPoolSet, (nextSorterPoolSet |> sorterPoolSet.Standard |> outputData.SorterPoolSet)
                                            qpSummaries, (sorterPoolSetSummarySet |> spSummarySet.Standard |> outputData.SorterPoolSetSummarySet)
                                            qpBinsSeries, (evalBinsCollection |> sorterPoolBinsSetSeries.Standard |> outputData.SorterPoolBinsSetSeries)
                                            |] allowOverwrite


                                        // Prune dead lineages and generate SorterPoolSetHistory
                                        //let poolSetHistory, prunedRunningMap = 
                                        //    SorterPoolSetHistory.pruneAndCreateFromPoolSet currentGen nextSorterPoolSet updatedRunningHistoryMap
                                        //let! qpHistory = 
                                        //    EvolutionFiles.queryAtGeneration genDb path state currentGen (geneSortDataType.SorterPoolSetHistory "")
                                        //    |> Result.ofOption "Failed to create QueryParams for SorterPoolSetHistory."
                                        //do! genDb.saveAsync geneSortDataMode.Results qpHistory (poolSetHistory |> spsh.Standard |> outputData.SorterPoolSetHistory) allowOverwrite

                                       // return ([], [], prunedRunningMap)
                                        return (if isSnapshot then ([], [], MhMap_Standard.empty) else (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, MhMap_Standard.empty))
                                    else
                                        //return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, updatedRunningHistoryMap)
                                        return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, MhMap_Standard.empty)
                                }

                            return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext MhMap_Standard.empty
                           // return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext []
                    }

                // An interim recovery checkpoint carries the report segment since the last snapshot.
                let! initialHistory, initialBins = asyncResult {
                    if path.CheckpointIntervals.IsSome && genStart > path.Start.FirstGeneration && not (Set.contains %genStart targetGenerationsForSnapshots) then
                        let! summaryQuery = EvolutionFiles.queryAtGeneration genDb path state genStart (geneSortDataType.SorterPoolSetSummarySet "") |> Result.ofOption "Missing checkpoint summary query."
                        let! binsQuery = EvolutionFiles.queryAtGeneration genDb path state genStart (geneSortDataType.SorterPoolBinsSetSeries "") |> Result.ofOption "Missing checkpoint bins query."
                        let! summaryData = genDb.loadAsync geneSortDataMode.Results summaryQuery
                        let! binsData = genDb.loadAsync geneSortDataMode.Results binsQuery
                        match summaryData, binsData with
                        | outputData.SorterPoolSetSummarySet (spSummarySet.Standard summaries), outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Standard bins) ->
                            return (summaries.SorterPoolSetSummaries |> Array.toList), (bins.SorterPoolEvalBinsSets.Values |> Seq.sortByDescending (fun b -> b.GenerationNumber) |> Seq.toList)
                        | _ -> return! Error "Checkpoint report payloads do not match the workflow."
                    else return [], []
                }
                // Continue the segment without dropping samples saved before cancellation.
                return! loop (%totalGen - %genStart) initialPoolSet initialHistory initialBins MhMap_Standard.empty
        }
