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


    

    /// Asynchronously runs evolution step-by-step over generation intervals.
    /// Handles sampling triggers for history accumulation, pool expansion, and DB persistence.
    let runEvolutionAsync
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (genStart: int<generationNumber>)
            (genIntervalCount: int<generationIntervalCount>)
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
            (sorterPoolSelectionIntervals: samplingConfig)
            (poolMeasure: sorterPoolMeasure)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult, string>> =

        // 1. Verify host.RunDb is IGeneSortGenDb and extract save configs
        let genDb = 
            match host.RunDb with
            | :? IGeneSortGenDb as gdb -> gdb
            | _ -> failwith "host.RunDb must implement IGeneSortGenDb"

        let saveIntervals = genDb.getGenSaveIntervals()
        let subIntervals = genDb.getGenSaveSubIntervals()

        // 2. Fetch the minimal sample set starting from genStart for genIntervalCount steps
        let startInt = int genStart
        let requiredCount = int genIntervalCount + 1

        let targetSamples = 
            IntSampleMethod.getSampleSetMinBound saveIntervals (startInt - 1) requiredCount
            |> Set.toArray
            |> Array.sort

        if targetSamples.Length = 0 || targetSamples.[0] <> startInt then
            async { return Error (sprintf "genStart (%d) is not a valid member of the getGenSaveIntervals sampling series." startInt) }
        elif targetSamples.Length < requiredCount then
            async { return Error (sprintf "Target generation sequence ended early: requested %d intervals from %d, but only obtained %d." %genIntervalCount startInt targetSamples.Length) }
        else
            let targetGenInt = targetSamples.[targetSamples.Length - 1]
            let totalGen = %targetGenInt : int<generationNumber>
            let genCount = totalGen - genStart

            // --- Frequency Triggers ---
            let targetGenerationsForPoolExpansion = 
                IntSampleMethod.getSampleSetMaxBound sorterPoolSelectionIntervals targetGenInt

            let targetGenerationsForSaveRunResult = 
                IntSampleMethod.getSampleSetMaxBound saveIntervals targetGenInt

            let targetGenerationsForSummaryReport = 
                IntSampleMethod.getSampleSetMaxBound subIntervals targetGenInt

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

                        // 2. Perform Pool Expansion on milestone
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

                        // 4. Save RunResult to Database on milestone
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
                                
                                    // Save run result to DB
                                    do! genDb.saveAsync qp (currentRunResult |> outputData.SorterRunResult) allowOverwrite

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