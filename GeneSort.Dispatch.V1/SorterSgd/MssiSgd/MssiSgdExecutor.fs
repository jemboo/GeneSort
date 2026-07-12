namespace GeneSort.Dispatch.V1.SorterSgd.Mssi


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
open GeneSort.Eval.V1


module MssiSgdExecutor =

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
                let! genFirst = rp.GetGenerationFirst() |> Result.ofOption "Missing genFirst."
                let! prioritizeNewMutants = rp.GetPrioritizeNewMutants() |> Result.ofOption "Missing prioritizeNewMutants."
                let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
                let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count"
                let! sorterEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
                let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."
                let! distinctSorterHashes = rp.GetDistinctSorterHashes() |> Result.ofOption "Missing distinctSorterHashes."
                let! sortedFraction = rp.GetSortedFraction() |> Result.ofOption "Missing sortedFraction."
                let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."

                // 2. Resolve target seed sorterPoolSet collection state depending on genFirst criteria
                let! seedSorterPoolSet = 
                    if %genFirst > 0 then
                        log "Looking up historical sorterPoolSet from database..."
                        let newRp = rp.WithQueryWithGenFirst (Some true)
                        
                        let qpSRRResult = 
                            host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                        asyncResult {
                            let! qpSRR = qpSRRResult 
                            let! (outData: outputData) = 
                                host.RunDb.loadAsync qpSRR 
                                |> AsyncResult.mapError (fun err -> sprintf "Database load error: %A" err)

                            let! sorterRunRes = outData |> OutputData.asSorterRunResult
                            return sorterRunRes.FinalPoolSet
                        }
                    else
                        // Otherwise create it ..
                        log "Make seedSorterPoolSet..."
                        sorterPoolSetCreator rp

                do! checkCancellation cts.Token
                log "Executing makeSortableTests..."
                let! sortableTest = makeSortableTests rp
                
                log "Making sorterModelMutator..."
                // Rates for mutator creation

                let! (orthoRate: float<orthoRate>) = rp.GetOrthoRate() |> Result.ofOption "Missing orthoRate in run parameters"
                let! (paraRate: float<paraRate>) = rp.GetParaRate() |> Result.ofOption "Missing paraRate in run parameters"
                let! modificationRate = rp.GetModificationRate() |> Result.ofOption "Missing modificationRate."


                let sorterModelMutator = SimpleSorterModelMutator.getMssiModelMutator
                                                (RngFactory.create rngType)
                                                ExcludeSelfCe
                                                modificationRate
                                                orthoRate
                                                paraRate
                                         |> sorterModelMutator.Simple

                log "Executing SorterRunResult.runEvolutionAsync..."
                let! (runResult: sorterRunResult) = SorterRunResult.runEvolutionAsync
                                                        genFirst
                                                        (genLast - genFirst)
                                                        sorterModelMutator
                                                        prioritizeNewMutants
                                                        distinctSorterHashes
                                                        sortersPerPool
                                                        sorterChildCount
                                                        sortableTest
                                                        sorterEvalType
                                                        sorterEvalMeasure
                                                        seedSorterPoolSet
                                                        sortedFraction
                                                        cts.Token
                                                        log

                // 4. Persistence of run stats mapping results out to output streams

                let newRp = rp.WithQueryWithGenFirst (Some false)
                let! qpSorterRunResult = 
                    host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                    |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                log (sprintf "Saving SorterRunResult - Id: %s" (string qpSorterRunResult.Id))
                do! host.RunDb.saveAsync qpSorterRunResult (runResult |> outputData.SorterRunResult) allowOverwrite
                
                
                log "evaluateEvolutionRun completed."
                return newRp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    CommonSorterSgd.makeStandardTests
                    CommonSorterSgd.createSeedSorterPoolSetStandard
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    CommonSorterSgd.makeMergeTests
                    CommonSorterSgd.createSeedSorterPoolSetMerge
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                CommonSorterSgd.makeFullReport
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.FullReport -> fullReportExecutor
