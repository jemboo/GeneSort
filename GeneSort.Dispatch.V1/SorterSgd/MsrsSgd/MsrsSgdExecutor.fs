namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

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
open GeneSort.Eval.V1.Sgd
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module MsrsSgdExecutor =

/// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRun
            (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token

                // 1. Gather all required run metrics and options out of your parameters block securely
                let! genLast = rp.GetGenerationLast() |> Result.ofOption "Missing genLast."             
                let! genCurrent = rp.GetGenerationCurrent() |> Result.ofOption "Missing genCurrent."
                let! genReportInterval = rp.GetGenerationReportInterval() |> Result.ofOption "Missing generation report interval."
                let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
                let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
                let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count"
                let! sorterEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
                let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."
                let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
                let! sortedFraction = rp.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."
                let! sorterCountCycle = rp.GetSorterCountCycle() |> Result.ofOption "Missing sorterCountCycle."
                let! sorterCountCycleMultiplier = rp.GetSorterCountCycleMultiplier() |> Result.ofOption "Missing sorterCountCycleMultiplier."
                let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."

                // 2. Resolve target seed sorterPoolSet collection state depending on genFirst criteria
                let! initialSeedPoolSet: sorterPoolSet = 
                    if %genCurrent > 0 then
                        log "Looking up historical sorterPoolSet from database..."
                        let qpSRRResult = 
                            host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterRunResult "")
                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                        asyncResult {
                            let! qpSRR = qpSRRResult 
                            let! (outData: outputData) = host.RunDb.loadAsync qpSRR |> AsyncResult.mapError (fun err -> sprintf "Database load error: %A" err)
                            let! sorterRunRes = outData |> OutputData.asSorterRunResult
                            return sorterRunRes.FinalPoolSet
                        }
                    else
                        log "Make seedSorterPoolSet..."
                        sorterPoolSetCreator rp

                do! checkCancellation cts.Token
                log "Executing makeSortableTests..."
                let! (sortableTest: sortableTest) = makeSortableTests rp
                
                log "Making sorterModelMutator..."
                let! (orthoRate: float<orthoRate>) = rp.GetOrthoRate() |> Result.ofOption "Missing orthoRate in run parameters"
                let! (paraRate: float<paraRate>) = rp.GetParaRate() |> Result.ofOption "Missing paraRate in run parameters"
                let! (selfSymRate: float<selfSymRate>) = rp.GetSelfSymRate() |> Result.ofOption "Missing selfSymRate in run parameters"
                let! modificationRate = rp.GetModificationRate() |> Result.ofOption "Missing modificationRate."

                let sorterModelMutator = 
                    SimpleSorterModelMutator.getMsrsModelMutator
                        (RngFactory.create rngType)
                        ExcludeSelfCe
                        modificationRate
                        orthoRate
                        paraRate
                        selfSymRate
                    |> sorterModelMutator.Simple

                // 3. Define the local mutation/step strategy closure
                let stepExecutionStrategy targetGenFirst stepSize workingPoolSet =
                    SorterRunResult.runEvolutionAsync
                        targetGenFirst stepSize sorterCountCycle sorterCountCycleMultiplier
                        sorterModelMutator prioritizeNewMutants 
                        distinctSorterHashes sortersPerPool sorterChildCount sortableTest 
                        sorterEvalType sorterEvalMeasure workingPoolSet sortedFraction cts.Token log

                // 4. Hand execution over to the centralized orchestrator loop
                log "Executing chunked SorterRunResult loop via loop manager..."
                let! finalRp: runParameters = 
                    EvolutionOrchestrator.runSlicesInLoop
                        host rp genCurrent genLast genReportInterval 
                        initialSeedPoolSet allowOverwrite cts.Token log stepExecutionStrategy
                
                log "evaluateEvolutionRun completed."
                return finalRp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    SortableTestMakers.makeStandardTests
                    PoolSetMakers.createSeedSorterPoolSetStandard
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    SortableTestMakers.makeMergeTests
                    PoolSetMakers.createSeedSorterPoolSetMerge
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeSummaryReport
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.SummaryReport -> Reporting.fullReportExecutor
        | sorterSgdExecutorType.SnapshotReport -> Reporting.snapshotReportExecutor
