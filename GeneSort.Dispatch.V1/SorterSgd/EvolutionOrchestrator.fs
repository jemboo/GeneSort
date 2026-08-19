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
    let runEvolutionAsyncOld
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
                                do! host.RunDb.saveAsync qp (currentRunResult |> outputData.SorterRunResult) allowOverwrite

                                // Reset accumulator on success, and allow cancellation
                                if cts.IsCancellationRequested then return! Error "runEvolutionAsync was cancelled."
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



    //let runEvolutionAsync
    //        (genDb: IGeneSortGenDb)
    //        (rp: runParameters)
    //        (allowOverwrite: bool<allowOverwrite>)
    //        (initialPoolSet: sorterPoolSet)
    //        (sortableTest: sortableTest)
    //        (mutator: sorterModelMutator)
    //        (cts: CancellationToken)
    //        (log: string -> unit) : Async<Result<sorterRunResult, string>> =

    //    asyncResult {
    //        let evalType = sorterEvalType.V2
    //        // Mandatory parameters
    //        let! genStart = rp.GetGenerationCurrent() |> Result.ofOption "Missing generationCurrent"
    //        let! genIntervalCount = rp.GetGenerationIntervalCount() |> Result.ofOption "Missing genIntervalCount."
    //        let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
    //        let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
    //        let! sorterCountPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
    //        let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count."
    //        let! srtrEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
    //        let! collectNewSortableTests = rp.GetCollectNewSortableTests() |> Result.ofOption "Missing collectNewSortableTests."
    //        let! sortedFractionThreshold = rp.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."

    //        // Optional parameters for Dynamic Variation and Pool Expansion
    //        let optSorterCountCycle = rp.GetSorterCountCycle()
    //        let optSorterCountCycleMultiplier = rp.GetSorterCountCycleMultiplier()
    //        let optSorterPoolExpansionRate = rp.GetSorterPoolExpansionRate()
    //        let optSorterPoolSelectionIntervals = rp.GetSorterPoolSelectionIntervals()
    //        let optPoolMeasure = rp.GetSorterPoolMeasure()

    //        // 1. Extract save configs
    //        let saveIntervals = genDb.getGenSaveIntervals()
    //        let subIntervals = genDb.getGenSaveSubIntervals()

    //        // 2. Fetch the minimal sample set starting from genStart
    //        let startInt = int genStart
    //        let requiredCount = int genIntervalCount

    //        let targetSamples = 
    //            SamplingConfig.getSampleSetWithMinBound saveIntervals (startInt - 1) requiredCount
    //            |> Set.toArray
    //            |> Array.sort

    //        if targetSamples.Length < requiredCount then
    //            return! Error (sprintf "Target generation sequence ended early: requested %d intervals from %d, but only obtained %d." %genIntervalCount startInt targetSamples.Length)
    //        else
    //            let targetGenInt = targetSamples.[targetSamples.Length - 1]
    //            let totalGen = %targetGenInt : int<generationNumber>
    //            let genCount = totalGen - genStart

    //            // --- Frequency Triggers ---
    //            let targetGenerationsForPoolExpansion = 
    //                match optSorterPoolSelectionIntervals with
    //                | Some intervals -> SamplingConfig.getSampleSetMaxBound intervals targetGenInt
    //                | None -> Set.empty

    //            let targetGenerationsForSaveRunResult = 
    //                SamplingConfig.getSampleSetMaxBound saveIntervals targetGenInt

    //            let targetGenerationsForSummaryReport = 
    //                SamplingConfig.getSampleSetMaxBound subIntervals targetGenInt

    //            // Condensed Optional Features Log
    //            let countLog = 
    //                match optSorterCountCycle, optSorterCountCycleMultiplier with
    //                | Some c, Some m -> sprintf "ENABLED (Cycle: %d, Mult: %.2f)" %c (float %m)
    //                | _ -> "DISABLED (Static)"

    //            let poolLog = 
    //                match optSorterPoolExpansionRate, optSorterPoolSelectionIntervals, optPoolMeasure with
    //                | Some r, Some i, Some m -> sprintf "ENABLED (Rate: %d, Intervals: %A, Measure: %A)" %r i m
    //                | _ ->
    //                    let missing = 
    //                        [ if optSorterPoolExpansionRate.IsNone then yield "Rate"
    //                          if optSorterPoolSelectionIntervals.IsNone then yield "Intervals"
    //                          if optPoolMeasure.IsNone then yield "Measure" ] |> String.concat ","
    //                    sprintf "DISABLED (Missing: %s)" missing

    //            log (sprintf "  [Optional Features] Dynamic Count: %s | Pool Expansion: %s" countLog poolLog)

    //            // Recursive loop modified to carry evalBinsSetAcc
    //            let rec loop 
    //                    (remainingSteps: int) 
    //                    (currentSorterPoolSet: sorterPoolSet) 
    //                    (historyAcc: sorterPoolSetSummary list) 
    //                    (evalBinsSetAcc: sorterPoolEvalBinsSet list) 
    //                    : Async<Result<sorterRunResult, string>> =
    //                asyncResult {
    //                    if remainingSteps < 0 then
    //                        return sorterRunResult.create currentSorterPoolSet (historyAcc |> List.rev |> List.toArray)
    //                    else
    //                        let currentGen = genStart + (genCount - %remainingSteps)

    //                        // 3. Dynamic Periodic Variation for sorterCountPerPool (Conditional)
    //                        let currentSorterCountPerPool : int<sorterCountPerPool> =
    //                            match optSorterCountCycle, optSorterCountCycleMultiplier with
    //                            | Some cycle, Some multiplierVal when %cycle > 0 ->
    //                                let scm = float %multiplierVal
    //                                let scPP = float %sorterCountPerPool
    //                                let multiplier = if ((%currentGen / %cycle) % 2 = 0) then (1.0 / scm) else (2.0 - 1.0 / scm)
    //                                UMX.tag (int (scPP * multiplier))
    //                            | _ ->
    //                                sorterCountPerPool

    //                        // 4. Evaluate Triggers
    //                        let shouldSummaryReport = Set.contains %currentGen targetGenerationsForSummaryReport
    //                        let shouldSaveRunResult = Set.contains %currentGen targetGenerationsForSaveRunResult

    //                        let shouldExpandPools = 
    //                            match optSorterPoolExpansionRate, optPoolMeasure with
    //                            | Some expansionRate, Some _ when expansionRate > 1<sorterPoolExpansionRate> ->
    //                                Set.contains %currentGen targetGenerationsForPoolExpansion && (%currentGen > 0)
    //                            | _ -> false

    //                        if shouldSummaryReport then
    //                            log (sprintf "Starting evolution step. Generation %d of %d" currentGen totalGen)

    //                        // 5. Snapshot summary before applying structural changes
    //                        let updatedSorterPoolSetSummary = 
    //                            if shouldSummaryReport then 
    //                                let currentSnapshot = SorterPoolSetSummary.fromPoolSet currentSorterPoolSet
    //                                currentSnapshot :: historyAcc
    //                            else 
    //                                historyAcc

    //                        // 6. Perform Pool Expansion on milestone (Conditional on Rate + Measure)
    //                        let poolSetForStep =
    //                            if shouldExpandPools then
    //                                let expansionRate = optSorterPoolExpansionRate.Value
    //                                let poolMeasure = optPoolMeasure.Value
    //                                log (sprintf "Expanding pools at Generation %d (Rate: %d)..." %currentGen %expansionRate)
    //                                currentSorterPoolSet
    //                                |> SorterPoolSet.trimPools expansionRate poolMeasure
    //                                |> SorterPoolSet.expandPools expansionRate
    //                            else
    //                                currentSorterPoolSet

    //                        // 7. Step Evolution
    //                        let reEvaluateParents = (remainingSteps % 10 = 0)

    //                        let nextSorterPoolSet = 
    //                            SorterPipeline.runGenerationStepDebug 
    //                                mutator 
    //                                currentSorterCountPerPool
    //                                sorterChildCount
    //                                prioritizeNewMutants
    //                                distinctSorterHashes
    //                                sortableTest 
    //                                evalType
    //                                srtrEvalMeasure
    //                                reEvaluateParents
    //                                poolSetForStep
    //                                collectNewSortableTests
    //                                sortedFractionThreshold

    //                        // Accumulate sorterPoolEvalBinsSet at summary report frequency
    //                        let updatedEvalBinsSetAcc =
    //                            if shouldSummaryReport then
    //                                let binsSetId = Guid.NewGuid() |> UMX.tag<sorterPoolEvalBinsSetId>
    //                                let currentBinsSet = sorterPoolEvalBinsSet.create binsSetId nextSorterPoolSet
    //                                currentBinsSet :: evalBinsSetAcc
    //                            else
    //                                evalBinsSetAcc

    //                        // 8. Save RunResult and SorterPoolEvalBinsSetCollection on milestone
    //                        let! (historyAccNext, evalBinsSetAccNext) = 
    //                            asyncResult {
    //                                if shouldSaveRunResult && (%currentGen > 0) then
    //                                    let currentRunResult = 
    //                                        sorterRunResult.create 
    //                                            nextSorterPoolSet 
    //                                            (updatedSorterPoolSetSummary |> List.rev |> List.toArray)

    //                                    let stepRp = rp.WithGenerationCurrent(Some currentGen)

    //                                    // Save SorterRunResult
    //                                    let! qpRunResult = 
    //                                        genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
    //                                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
    //                                    log (sprintf "Saving SorterRunResult checkpoint at Generation %d..." %currentGen)
    //                                    do! genDb.saveAsync qpRunResult (currentRunResult |> outputData.SorterRunResult) allowOverwrite

    //                                    // Save SorterPoolEvalBinsSetCollection
    //                                    let collectionId = Guid.NewGuid() |> UMX.tag<sorterPoolEvalBinsSetCollectionId>
    //                                    let evalBinsCollection = 
    //                                            sorterPoolEvalBinsSetCollection.create collectionId (updatedEvalBinsSetAcc |> List.rev)

    //                                    let! qpBinsCollection = 
    //                                        genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolEvalBinsSetCollection "")
    //                                        |> Result.ofOption "Failed to create QueryParams for SorterPoolEvalBinsSetCollection."
    //                                    log (sprintf "Saving SorterPoolEvalBinsSetCollection checkpoint at Generation %d..." %currentGen)
    //                                    do! genDb.saveAsync qpBinsCollection (evalBinsCollection |> outputData.SorterPoolEvalBinsSetCollection) allowOverwrite

    //                                    if cts.IsCancellationRequested then return! Error "runEvolutionAsync was cancelled."
    //                                    return ([], [])
    //                                else
    //                                    return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc)
    //                            }

    //                        // 9. Forced GC Compacting
    //                        if remainingSteps % 50 = 0 then
    //                            System.Runtime.GCSettings.LargeObjectHeapCompactionMode 
    //                                    <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
    //                            GC.Collect(2, GCCollectionMode.Forced, true, true)

    //                        return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext
    //                }

    //            // Execute loop with initial empty collection accumulator
    //            return! loop %genCount initialPoolSet [] []
    //}











//namespace GeneSort.Dispatch.V1.SorterSgd

//open FSharp.UMX
//open System.Threading
//open GeneSort.Core
//open GeneSort.Sorting.Sortable
//open GeneSort.Project.V1
//open GeneSort.Eval.V1
//open GeneSort.Db.V1
//open GeneSort.Dispatch.V1
//open GeneSort.Eval.V1.Sgd
//open GeneSort.SortingOps
//open System
//open GeneSort.Model.Sorting.V1

//module EvolutionOrchestrator =

    let runEvolutionAsync
            (genDb: IGeneSortGenDb)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (initialPoolSet: sorterPoolSet)
            (sortableTest: sortableTest)
            (mutator: sorterModelMutator)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult, string>> =

        asyncResult {
            let evalType = sorterEvalType.V2
            // Mandatory parameters
            let! genStart = rp.GetGenerationCurrent() |> Result.ofOption "Missing generationCurrent"
            let! genIntervalCount = rp.GetGenerationIntervalCount() |> Result.ofOption "Missing genIntervalCount."
            let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
            let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
            let! sorterCountPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
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
            let startInt = int genStart
            let requiredCount = int genIntervalCount

            let targetSamples = 
                SamplingConfig.getSampleSetWithMinBound saveIntervals (startInt - 1) requiredCount
                |> Set.toArray
                |> Array.sort

            if targetSamples.Length < requiredCount then
                return! Error (sprintf "Target generation sequence ended early: requested %d intervals from %d, but only obtained %d." %genIntervalCount startInt targetSamples.Length)
            else
                let targetGenInt = targetSamples.[targetSamples.Length - 1]
                let totalGen = %targetGenInt : int<generationNumber>
                let genCount = totalGen - genStart

                // --- Frequency Triggers ---
                let targetGenerationsForPoolExpansion = 
                    match optSorterPoolSelectionIntervals with
                    | Some intervals -> SamplingConfig.getSampleSetMaxBound intervals targetGenInt
                    | None -> Set.empty

                let targetGenerationsForSaveRunResult = 
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

                // Recursive loop modified to carry evalBinsSetAcc and poolSetHistoryAcc
                let rec loop 
                        (remainingSteps: int) 
                        (currentSorterPoolSet: sorterPoolSet) 
                        (historyAcc: sorterPoolSetSummary list) 
                        (evalBinsSetAcc: sorterPoolEvalBinsSet list) 
                        (poolSetHistoryAcc: sorterPoolSetHistory list)
                        : Async<Result<sorterRunResult, string>> =
                    asyncResult {
                        if remainingSteps < 0 then
                            return sorterRunResult.create currentSorterPoolSet (historyAcc |> List.rev |> List.toArray)
                        else
                            let currentGen = genStart + (genCount - %remainingSteps)

                            // 3. Dynamic Periodic Variation for sorterCountPerPool (Conditional)
                            let currentSorterCountPerPool : int<sorterCountPerPool> =
                                match optSorterCountCycle, optSorterCountCycleMultiplier with
                                | Some cycle, Some multiplierVal when %cycle > 0 ->
                                    let scm = float %multiplierVal
                                    let scPP = float %sorterCountPerPool
                                    let multiplier = if ((%currentGen / %cycle) % 2 = 0) then (1.0 / scm) else (2.0 - 1.0 / scm)
                                    UMX.tag (int (scPP * multiplier))
                                | _ ->
                                    sorterCountPerPool

                            // 4. Evaluate Triggers
                            let shouldSummaryReport = Set.contains %currentGen targetGenerationsForSummaryReport
                            let shouldSaveRunResult = Set.contains %currentGen targetGenerationsForSaveRunResult

                            let shouldExpandPools = 
                                match optSorterPoolExpansionRate, optPoolMeasure with
                                | Some expansionRate, Some _ when expansionRate > 1<sorterPoolExpansionRate> ->
                                    Set.contains %currentGen targetGenerationsForPoolExpansion && (%currentGen > 0)
                                | _ -> false

                            if shouldSummaryReport then
                                log (sprintf "Starting evolution step. Generation %d of %d" currentGen totalGen)

                            // 5. Snapshot summary before applying structural changes
                            let updatedSorterPoolSetSummary = 
                                if shouldSummaryReport then 
                                    let currentSnapshot = SorterPoolSetSummary.fromPoolSet currentSorterPoolSet
                                    currentSnapshot :: historyAcc
                                else 
                                    historyAcc

                            // 6. Perform Pool Expansion on milestone (Conditional on Rate + Measure)
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

                            // 7. Step Evolution
                            let reEvaluateParents = (remainingSteps % 10 = 0)

                            let nextSorterPoolSet = 
                                SorterPipeline.runGenerationStepDebug 
                                    mutator 
                                    currentSorterCountPerPool
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

                            // Accumulate sorterPoolEvalBinsSet at summary report frequency
                            let updatedEvalBinsSetAcc =
                                if shouldSummaryReport then
                                    let binsSetId = Guid.NewGuid() |> UMX.tag<sorterPoolEvalBinsSetId>
                                    let currentBinsSet = sorterPoolEvalBinsSet.create binsSetId nextSorterPoolSet
                                    currentBinsSet :: evalBinsSetAcc
                                else
                                    evalBinsSetAcc

                            // Accumulate sorterPoolSetHistory at summary report frequency
                            let updatedPoolSetHistoryAcc =
                                if shouldSummaryReport then
                                    let currentHistory = SorterPoolSetHistory.fromPoolSet currentGen nextSorterPoolSet
                                    currentHistory :: poolSetHistoryAcc
                                else
                                    poolSetHistoryAcc

                            // 8. Save RunResult, SorterPoolEvalBinsSetCollection, and SorterPoolSetHistory on milestone
                            let! (historyAccNext, evalBinsSetAccNext, poolSetHistoryAccNext) = 
                                asyncResult {
                                    if shouldSaveRunResult && (%currentGen > 0) then
                                        let currentRunResult = 
                                            sorterRunResult.create 
                                                nextSorterPoolSet 
                                                (updatedSorterPoolSetSummary |> List.rev |> List.toArray)

                                        let stepRp = rp.WithGenerationCurrent(Some currentGen)

                                        // Save SorterRunResult
                                        let! qpRunResult = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                                        log (sprintf "Saving SorterRunResult checkpoint at Generation %d..." %currentGen)
                                        do! genDb.saveAsync qpRunResult (currentRunResult |> outputData.SorterRunResult) allowOverwrite

                                        // Save SorterPoolEvalBinsSetCollection
                                        let collectionId = Guid.NewGuid() |> UMX.tag<sorterPoolEvalBinsSetCollectionId>
                                        let evalBinsCollection = 
                                                sorterPoolEvalBinsSetCollection.create collectionId (updatedEvalBinsSetAcc |> List.rev)

                                        let! qpBinsCollection = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolEvalBinsSetCollection "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolEvalBinsSetCollection."
                                        log (sprintf "Saving SorterPoolEvalBinsSetCollection checkpoint at Generation %d..." %currentGen)
                                        do! genDb.saveAsync qpBinsCollection (evalBinsCollection |> outputData.SorterPoolEvalBinsSetCollection) allowOverwrite

                                        // Save SorterPoolSetHistoryCollection
                                        let historyCollectionId = Guid.NewGuid() |> UMX.tag<sorterPoolSetHistoryCollectionId>
                                        let poolSetHistoryCollection = 
                                                sorterPoolSetHistoryCollection.create historyCollectionId (updatedPoolSetHistoryAcc |> List.rev)

                                        let! qpHistoryCollection = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterPoolSetHistoryCollection "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterPoolSetHistoryCollection."
                                        log (sprintf "Saving SorterPoolSetHistoryCollection checkpoint at Generation %d..." %currentGen)
                                        do! genDb.saveAsync qpHistoryCollection (poolSetHistoryCollection |> outputData.SorterPoolSetHistoryCollection) allowOverwrite

                                        if cts.IsCancellationRequested then return! Error "runEvolutionAsync was cancelled."
                                        return ([], [], [])
                                    else
                                        return (updatedSorterPoolSetSummary, updatedEvalBinsSetAcc, updatedPoolSetHistoryAcc)
                                }

                            // 9. Forced GC Compacting
                            if remainingSteps % 50 = 0 then
                                System.Runtime.GCSettings.LargeObjectHeapCompactionMode 
                                        <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                                GC.Collect(2, GCCollectionMode.Forced, true, true)

                            return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext evalBinsSetAccNext poolSetHistoryAccNext
                    }

                // Execute loop with initial empty collection accumulators
                return! loop %genCount initialPoolSet [] [] []
    }