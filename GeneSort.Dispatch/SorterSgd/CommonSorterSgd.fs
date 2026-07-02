namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.SortingOps
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Sorting
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Dispatch.V1
open System.Threading
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Model.Sorting.V1


type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport


module SorterSgdExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"



module CommonSorterSgd =

    let makeStandardTests (rp:runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let sortableTestId = Guid.NewGuid() |> UMX.tag<sortableTestId>
                return (sortingWidth, sortableTestId)
            }
            match paramsOpt with
            | Some (sortingWidth, sortableTestId) ->
                let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                return Ok ( SortableTestModel.makeSortableTest 
                                    sortableTestId
                                    testModel 
                                    _dataFormatBitVector512)
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }


    let makeMergeTests (rp: runParameters) : Async<Result<sortableTest, string>> =
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
                return! SortableMergeTestDb.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }



    let createSeedSorterPoolSetStandard 
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getStandardSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    GeneSort.SortingOps.sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                seedSorterModelSet
        }


    let createSeedSorterPoolSetMerge
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! mergeDimension = rp.GetMergeDimension() |> Result.ofOption "Missing mergeDimension."
            let! mergeSuffixType = rp.GetMergeSuffixType() |> Result.ofOption "Missing mergeSuffixType."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getMergeSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    mergeDimension
                    mergeSuffixType
                    GeneSort.SortingOps.sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag<sorterModelSetId>) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                seedSorterModelSet
        }


    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =


        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
                let newRp = rp.WithQueryWithGenFirst (Some false)
                let! qpSorterRunResult = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                let! outB = host.RunDb.loadAsync qpSorterRunResult
                let! (srResult : sorterRunResult) = outB |> OutputData.asSorterRunResult |> Async.singleton

                let reportName = (sprintf "SorterRunResult_report" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = srResult |> SorterRunResult.toDataTableRecords ""
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (newRp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)


