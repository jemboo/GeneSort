namespace GeneSort.Dispatch.V1.SorterEvalBins

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Model.Sorting.Simple.V1
open Common
open GeneSort.Dispatch.V1.OpsUtils

module EvalBinsExecutor =

    let makeStandardTests (rp:runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let! qpTests = SimpleSorterModelDbs.makeStandardQueryParamsFromRunParams rp (outputDataType.SortableTest "")
                return (sortingWidth, qpTests)
            }
            match paramsOpt with
            | Some (sortingWidth, qpTests) ->
                let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                return Ok (SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel standardSortableDataFormat)
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }


    let makeMergeTests (rp: runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   
                let! sw = rp.GetSortingWidth()
                let! md = rp.GetMergeDimension()
                let! mst = rp.GetMergeSuffixType()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, sw, md, mst, sdf)
            }

            match paramsOpt with
            | Some (repl, sw, md, mst, sdf) ->
                return! GeneSort.Dispatch.V1.SortableTest.SortableTestDb.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }



    let makeUniformSorterModelSets (rp:runParameters) (splitFactor:int) : seq<sorterModelSet> option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let! rngType = rp.GetRngType()

            let sorterModelGen = Common.getSimpleUniformSorterModelGen 
                                    rngType sortingWidth simpleSorterModelType

            let! qpModelSet = SimpleSorterModelDbs.makeStandardQueryParamsFromRunParams 
                                         rp (outputDataType.SorterModelSet "")

            let! repl = rp.GetRepl()
            let! totalSorterCount = rp.GetSorterCount()
        
            let chunkCount = (%totalSorterCount / splitFactor) |> UMX.tag<sorterCount>
            let baseFirstIdx = (%repl * %totalSorterCount)

            // Using 'seq' instead of '[|' means this block yields elements lazily
            return seq {
                for i in 0 .. (splitFactor - 1) do
                    let pieceFirstIdx = (baseFirstIdx + (i * %chunkCount)) |> UMX.tag<sorterCount>
                
                    yield SorterModelGen.makeSorterModelSetFromIndexSpan 
                            (%qpModelSet.Id |> UMX.tag) pieceFirstIdx chunkCount sorterModelGen
            }
        }


    let _makeSorterEvalBins 
            (makeSorterModelSets: runParameters -> int -> sorterModelSet seq option) // Updated signature
            (makeTests: runParameters -> Async<Result<Sortable.sortableTest, string>>)
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Splitting Sorter Model Creation
                do! checkCancellation cts.Token
                
                let totalSorterCount = rp.GetSorterCount() |> Option.defaultValue (1000 |> UMX.tag)
                let splitFactor = %totalSorterCount / 100
                log (sprintf "Creating Sorter Model Sets split into %d pieces..." splitFactor)
        
                let! modelSets = 
                    makeSorterModelSets rp splitFactor 
                    |> Result.ofOption "Failed: SorterModelSets could not be split or initialized."

                // 2. Test Generation (Shared across all chunk evaluations)
                do! checkCancellation cts.Token
                log "Generating Sortable Tests..."
                let! tests = makeTests rp 

                let! qpEval = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                              |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let! qpBins = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                              |> Result.ofOption "Failed to create QueryParams for Bins."

                let collectTests = Common.CollectSortableTests
                let testId = tests |> SortableTests.getId

                // 3. Sequential Chunk Evaluation & Merging
                log "Running Split Sorter Evaluations & Bin Accumulation..."
            
                // We initialize an empty bin set to accumulate results into
                let initialBins = sorterEvalBinsV1.createEmpty (%qpBins.Id |> UMX.tag) testId

                let mutable accumulatedBins = initialBins

                let mutable chunkIdx = 1
                for modelSetChunk in modelSets do
                    do! checkCancellation cts.Token
                    log (sprintf "Evaluating chunk %d of %d..." chunkIdx splitFactor)
    
                    let fullSorterSetChunk = SorterModelSet.makeSorterSet (%modelSetChunk.Id |> UMX.tag) modelSetChunk
    
                    let sorterSetEvalChunk = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSetChunk tests collectTests
    
                    let chunkBins = sorterEvalBinsV1.createFromEvals 
                                        (%qpBins.Id |> UMX.tag) 
                                        testId 
                                        sorterSetEvalChunk.SorterEvals 
    
                    accumulatedBins <- SorterEvalBinsV1.merge accumulatedBins chunkBins
                    chunkIdx <- chunkIdx + 1


                let sorterEvalBins = sorterEvalBins.V1 accumulatedBins

                // 4. Persistence
                log (sprintf "Saving Merged SorterEvalBins %s" (string %qpBins.Id))
                do! host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
        
                log "Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)



    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpBins = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                              |> Result.ofOption "Failed to create QueryParams for Bins."
                let! outB = host.ProjectDb.loadAsync qpBins
                let! bins = outB |> OutputData.asSorterEvalBins |> Async.singleton

                let reportName = sprintf "FullReport" |> UMX.tag<textReportName>
                let! qpReport = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.ProjectDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        }


    let makeBinsReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Bins Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpBins = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                                    |> Result.ofOption "Failed to create QueryParams for Bins."
                let! outB = host.ProjectDb.loadAsync qpBins 
                let! bins = outB |> OutputData.asSorterEvalBins |> Async.singleton

                let reportName = sprintf "BinsReport" |> UMX.tag<textReportName>
                let! qpReport = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                    |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = bins |> SorterEvalBins.makeSummaryDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.ProjectDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        }

    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvalBins 
                    makeUniformSorterModelSets
                    makeStandardTests
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSorterEvalBins 
                    makeUniformSorterModelSets
                    makeMergeTests
                    host rp allowOverwrite cts progress }

    let binsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeBinsReport
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeFullReport
                    host rp allowOverwrite cts progress }

    let getExecutor (executorType: evalBinsExecutorType) : IRunParamsExecutor =
        match executorType with
        | StandardSortables -> standardExecutor
        | MergeSortables -> mergeExecutor
        | FullReport -> fullReportExecutor
        | BinsReport -> binsReportExecutor