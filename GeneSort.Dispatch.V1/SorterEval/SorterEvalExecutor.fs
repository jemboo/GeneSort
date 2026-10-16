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
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils



module SorterEvalExecutor =

    let _makeSorterEvals 
            (host: runHost)
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
                log "Generating and saving evaluation setup..."
                do! host.Setup rp
                do! checkCancellation cts.Token
                let! qpModels =
                    host.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterModelSet."
                log "Loading saved SorterModelSet..."
                let! savedModels = host.RunDb.loadAsync geneSortDataMode.Setup qpModels
                let! modelSet = savedModels |> OutputData.asSorterModelSet

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
                let! qpTests =
                    host.QueryParamsFromRunParams rp (geneSortDataType.SortableTestsWithPrefix "")
                    |> Result.ofOption "Failed to create QueryParams for SortableTestsWithPrefix."
                log "Loading saved Sortable Tests with prefix..."
                let! savedTests = host.RunDb.loadAsync geneSortDataMode.Setup qpTests
                let! tests, ces = savedTests |> OutputData.asSortableTestsWithPrefix
                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                let! qpSorterSet = 
                    host.QueryParamsFromRunParams rp (geneSortDataType.SorterSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSet."

                let! qpEval = 
                    host.QueryParamsFromRunParams rp (geneSortDataType.SorterSetEval "")
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
                do! host.RunDb.saveAsync geneSortDataMode.Results qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)





        
    let private bindSetup (host: runHost) allowOverwrite (cts: CancellationTokenSource) =
        let checkedSetup (setup: setupFunction) : setupFunction =
            fun rp -> asyncResult {
                do! checkCancellation cts.Token
                return! setup rp
            }
        let context =
            setupContext.create
                (Map.ofList [
                    SorterModelSetCatalogNames.uniform, SorterModelSetMakers.makeUniformSorterModelSet host allowOverwrite |> checkedSetup
                ])
                (SortableTestsMakers.storeTests SortableTestsMakers.makeStandardTests host allowOverwrite |> checkedSetup)
                (SortableTestsMakers.storeTests SortableTestsMakers.makeMergeTests host allowOverwrite |> checkedSetup)
                (SortableTestsMakers.storeTests SortableTestsMakers.getPrefixTests host allowOverwrite |> checkedSetup)
        host.WithSetupContext(context)

    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let host = bindSetup host allowOverwrite cts
                _makeSorterEvals host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let host = bindSetup host allowOverwrite cts
                _makeSorterEvals host rp allowOverwrite cts progress }

    let prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let host = bindSetup host allowOverwrite cts
                _makeSorterEvals host rp allowOverwrite cts progress }


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
        | _ -> failwithf "Executor type %A not implemented." executorType
