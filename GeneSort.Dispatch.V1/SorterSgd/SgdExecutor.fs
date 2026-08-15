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
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps

module SgdExecutor =

/// Handles initialization, evaluation, and DB saving when no checkpoint exists
    let initializeAndSaveSeedPoolSet 
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (genDb: IGeneSortGenDb)
            (rp: runParameters)
            (sortableTest: sortableTest)
            (log: string -> unit) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            log "No saved checkpoint found. Creating initial seedSorterPoolSet..."
            let! seedPoolSet = sorterPoolSetCreator rp
            
            let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."

            let computedEvals = 
                seedPoolSet 
                |> SorterPoolRunner.evaluatePoolSet 
                    sortableTest 
                    sorterEvalType
                    true // reEvaluateParents
                    (false |> UMX.tag<collectNewSortableTests>)
            
            let evaluatedSeedSet = seedPoolSet |> SorterPoolSet.updateSorterEvals computedEvals
            let seedSorterRunResult = sorterRunResult.create evaluatedSeedSet [||] |> outputData.SorterRunResult
            
            let! qpSsrr = 
                genDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterRunResult "")
                |> Result.ofOption "Failed to create QueryParams for seedSorterRunResult."
                
            do! genDb.saveAsync qpSsrr seedSorterRunResult (false |> UMX.tag<allowOverwrite>)
            log (sprintf "Initial seedSorterPoolSet saved at generation %d." %evaluatedSeedSet.GenerationNumber)

            return evaluatedSeedSet
        }


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
            // Mandatory parameters
            let! genStart = rp.GetGenerationCurrent() |> Result.ofOption "Missing generationCurrent"
            let! genIntervalCount = rp.GetGenerationIntervalCount() |> Result.ofOption "Missing genIntervalCount."
            let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
            let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
            let! sorterCountPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count."
            let! evalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."
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
            let requiredCount = int genIntervalCount + 1

            let targetSamples = 
                SamplingConfig.getSampleSetMinBound saveIntervals (startInt - 1) requiredCount
                |> Set.toArray
                |> Array.sort

            if targetSamples.Length = 0 || targetSamples.[0] <> startInt then
                return! Error (sprintf "genStart (%d) is not a valid member of the getGenSaveIntervals sampling series." startInt)
            elif targetSamples.Length < requiredCount then
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

                let rec loop 
                        (remainingSteps: int) 
                        (currentSorterPoolSet: sorterPoolSet) 
                        (historyAcc: sorterPoolSetSummary list) 
                        : Async<Result<sorterRunResult, string>> =
                    asyncResult {
                        if remainingSteps <= 0 then
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
                                              if optPoolMeasure.IsNone then yield "Measure" ]
                                            |> String.concat ","
                                        sprintf "DISABLED (Missing: %s)" missing

                                log (sprintf "  [Optional Features] Dynamic Count: %s | Pool Expansion: %s" countLog poolLog)

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
                            let adjSorterEvalType = if (remainingSteps = 1) then sorterEvalType.V2 else evalType
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
                                    srtrEvalMeasure
                                    reEvaluateParents
                                    poolSetForStep
                                    collectNewSortableTests
                                    sortedFractionThreshold

                            // 8. Save RunResult to Database on milestone
                            let! historyAccNext = 
                                asyncResult {
                                    if shouldSaveRunResult && (%currentGen > 0) then
                                        let currentRunResult = 
                                            sorterRunResult.create 
                                                nextSorterPoolSet 
                                                (updatedSorterPoolSetSummary |> List.rev |> List.toArray)

                                        let stepRp = rp.WithGenerationCurrent(Some currentGen)
                                
                                        let! qp = 
                                            genDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
                                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                                        log (sprintf "Saving SorterRunResult checkpoint at Generation %d..." %currentGen)
                                        do! genDb.saveAsync qp (currentRunResult |> outputData.SorterRunResult) allowOverwrite

                                        if cts.IsCancellationRequested then return! Error "runEvolutionAsync was cancelled."
                                        return []
                                    else
                                        return updatedSorterPoolSetSummary
                                }

                            // 9. Forced GC Compacting
                            if remainingSteps % 50 = 0 then
                                System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                                GC.Collect(2, GCCollectionMode.Forced, true, true)

                            return! loop (remainingSteps - 1) nextSorterPoolSet historyAccNext
                    }

                // Execute loop
                return! loop %genCount initialPoolSet []
        }