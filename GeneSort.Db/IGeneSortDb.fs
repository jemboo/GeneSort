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

type IGeneSortDb =
        abstract member saveAsync :  queryParams -> outputData -> Async<unit>
        abstract member loadAsync : queryParams -> outputDataType -> Async<Result<outputData, OutputError>>
        abstract member getAllRunParametersAsync : CancellationToken option -> IProgress<string> option -> Async<runParameters[]>



module GeneSortDb =
    
    let getRunParametersAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<runParameters, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.RunParameters
            return 
                match result with
                | Ok (RunParameters rp) -> Ok rp
                | Ok _ -> Error "Unexpected output data type: expected RunParameters"
                | Error err -> Error err
        }
    
    let getSorterSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SorterSet
            return 
                match result with
                | Ok (SorterSet ss) -> Ok ss
                | Ok _ -> Error "Unexpected output data type: expected SorterSet"
                | Error err -> Error err
        }
    
    let getSortableTestSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SortableTestSet
            return 
                match result with
                | Ok (SortableTestSet sts) -> Ok sts
                | Ok _ -> Error "Unexpected output data type: expected SortableTestSet"
                | Error err -> Error err
        }
    
    let getSorterModelSetMakerAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SorterModelSetMaker
            return 
                match result with
                | Ok (SorterModelSetMaker smsm) -> Ok smsm
                | Ok _ -> Error "Unexpected output data type: expected SorterModelSetMaker"
                | Error err -> Error err
        }
    
    let getSortableTestModelSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SortableTestModelSet
            return 
                match result with
                | Ok (SortableTestModelSet stms) -> Ok stms
                | Ok _ -> Error "Unexpected output data type: expected SortableTestModelSet"
                | Error err -> Error err
        }
    
    let getSortableTestModelSetMakerAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SortableTestModelSetMaker
            return 
                match result with
                | Ok (SortableTestModelSetMaker stmsm) -> Ok stmsm
                | Ok _ -> Error "Unexpected output data type: expected SortableTestModelSetMaker"
                | Error err -> Error err
        }
    
    let getSorterSetEvalAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSetEval, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SorterSetEval
            return 
                match result with
                | Ok (SorterSetEval sse) -> Ok sse
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEval"
                | Error err -> Error err
        }
    
    let getSorterSetEvalBinsAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSetEvalBins, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.SorterSetEvalBins
            return 
                match result with
                | Ok (SorterSetEvalBins sseb) -> Ok sseb
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEvalBins"
                | Error err -> Error err
        }
    
    let getProjectAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<project, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams outputDataType.Project
            return 
                match result with
                | Ok (Project p) -> Ok p
                | Ok _ -> Error "Unexpected output data type: expected Project"
                | Error err -> Error err
        }