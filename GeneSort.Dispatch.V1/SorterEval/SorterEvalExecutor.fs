namespace GeneSort.Dispatch.V1.SorterEval

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils



module SorterEvalExecutor =

    let _makeSorterEvals 
            (makeSorterModelSet: runParameters -> sorterModelSet option)
            (makeSortableTests: runParameters -> Async<Result<sortableTest * (ce array), string>>)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log (msg: string) : unit = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                
                // 1. Unpack run configuration
                let! modelSet = 
                    makeSorterModelSet rp 
                    |> Result.ofOption "Failed: SorterModelSet could not be initialized from parameters."

                let! sWidth = 
                    rp.GetSortingWidth() 
                    |> Result.ofOption "Missing sorting width."

                let! sorterEvalType =
                    rp.GetSorterEvalType() 
                    |> Result.ofOption "Missing sorterEvalType."

                let! collectSortableTests =
                    rp.GetCollectNewSortableTests() 
                    |> Result.ofOption "Missing collectNewSortableTests."

                // 2. Generate common evaluation dependencies
                do! checkCancellation cts.Token
                log "Generating Sortable Tests..."
                let! (tests, ces) = makeSortableTests rp 
                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                let! qpSorterSet = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSet."

                let! qpEval = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let testId = tests |> SortableTests.getId

                // 3. Materialize full SorterSet and Evaluate
                log (sprintf "Evaluating SorterModelSet with %d sorters..." modelSet.SorterModels.Length)
                
                let maxCeCount = None
                let fullSorterSet = SorterModelSet.makeSorterSet (Guid.Empty |> UMX.tag) maxCeCount modelSet

                do! checkCancellation cts.Token
                let sorterEvalsArray = 
                    SorterSetEval.makeSorterEvals fullSorterSet.Sorters prefix tests sorterEvalType collectSortableTests

                // 4. Build Master SorterSetEval
                log "Compiling final SorterSetEval structure..."
                let correctSorterSetId = (%qpSorterSet.Id) |> UMX.tag<sorterSetId>

                let finalSorterSetEval = 
                    sorterSetEval.create 
                        (%qpEval.Id |> UMX.tag) 
                        correctSorterSetId 
                        testId 
                        sorterEvalsArray

                // 5. Persistence
                log (sprintf "Saving Combined SorterSetEval %s" (string %qpEval.Id))
                do! host.RunDb.saveAsync qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)





        
    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvals 
                    SorterModelSetMakers.makeUniformSorterModelSet
                    SortableTestMakers.makeStandardTests
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvals 
                    SorterModelSetMakers.makeUniformSorterModelSet
                    SortableTestMakers.makeMergeTests
                    host rp allowOverwrite cts progress }

    let prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvals 
                    SorterModelSetMakers.makeUniformSorterModelSet
                    SortableTestMakers.getPrefixTests
                    host rp allowOverwrite cts progress }


    let stageStatsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeStageStatsReport
                    host rp allowOverwrite cts progress }

    let ceBinsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeCeBinSummaryStats
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeFullReport
                    host rp allowOverwrite cts progress }


    let getExecutor (executorType: sorterEvalExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterEvalExecutorType.GenStandard -> standardExecutor
        | sorterEvalExecutorType.GenMerge -> mergeExecutor
        | sorterEvalExecutorType.GenPrefix -> prefixExecutor
        | FullReport -> fullReportExecutor
        | StageStatsReport -> stageStatsReportExecutor