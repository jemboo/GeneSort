namespace GeneSort.Db

open System
open System.Threading
open GeneSort.Core
open GeneSort.Runs.Params
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs

type IGeneSortDb2 =
        abstract member saveAsync :  runParameters option -> outputData -> Async<unit>
        abstract member loadAsync : runParameters option -> outputDataType -> Async<Result<outputData, OutputError>>
        abstract member getAllRunParametersAsync : CancellationToken option -> IProgress<string> option -> Async<runParameters[]>



module GeneSortDb2 =
    
    let getRunParametersAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<runParameters, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.RunParameters
            return 
                match result with
                | Ok (RunParameters rp) -> Ok rp
                | Ok _ -> Error "Unexpected output data type: expected RunParameters"
                | Error err -> Error err
        }
    
    let getSorterSetAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sorterSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterSet
            return 
                match result with
                | Ok (SorterSet ss) -> Ok ss
                | Ok _ -> Error "Unexpected output data type: expected SorterSet"
                | Error err -> Error err
        }
    
    let getSortableTestsSetAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sortableTestsSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SortableTestsSet
            return 
                match result with
                | Ok (SortableTestsSet sts) -> Ok sts
                | Ok _ -> Error "Unexpected output data type: expected SortableTestsSet"
                | Error err -> Error err
        }
    
    let getSorterModelSetMakerAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sorterModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterModelSetMaker
            return 
                match result with
                | Ok (SorterModelSetMaker smsm) -> Ok smsm
                | Ok _ -> Error "Unexpected output data type: expected SorterModelSetMaker"
                | Error err -> Error err
        }
    
    let getSortableTestsModelSetAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sortableTestsModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SortableTestsModelSet
            return 
                match result with
                | Ok (SortableTestsModelSet stms) -> Ok stms
                | Ok _ -> Error "Unexpected output data type: expected SortableTestsModelSet"
                | Error err -> Error err
        }
    
    let getSortableTestsModelSetMakerAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sortableTestsModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SortableTestsModelSetMaker
            return 
                match result with
                | Ok (SortableTestsModelSetMaker stmsm) -> Ok stmsm
                | Ok _ -> Error "Unexpected output data type: expected SortableTestsModelSetMaker"
                | Error err -> Error err
        }
    
    let getSorterSetEvalAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sorterSetEval, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterSetEval
            return 
                match result with
                | Ok (SorterSetEval sse) -> Ok sse
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEval"
                | Error err -> Error err
        }
    
    let getSorterSetEvalBinsAsync (geneSortDb: IGeneSortDb2) (runParameters: runParameters) : Async<Result<sorterSetEvalBins, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterSetEvalBins
            return 
                match result with
                | Ok (SorterSetEvalBins sseb) -> Ok sseb
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEvalBins"
                | Error err -> Error err
        }
    
    let getProjectAsync (geneSortDb: IGeneSortDb2) : Async<Result<project, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync None outputDataType.Project
            return 
                match result with
                | Ok (Project p) -> Ok p
                | Ok _ -> Error "Unexpected output data type: expected Project"
                | Error err -> Error err
        }