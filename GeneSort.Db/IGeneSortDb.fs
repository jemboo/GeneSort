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
        abstract member saveAsync :  runParameters option -> outputData -> outputDataType -> Async<unit>
        abstract member loadAsync : runParameters option -> outputDataType -> Async<Result<outputData, OutputError>>
        abstract member getAllRunParametersAsync : CancellationToken option -> IProgress<string> option -> Async<runParameters[]>



module GeneSortDb =
    
    let getRunParametersAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<runParameters, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.RunParameters
            return 
                match result with
                | Ok (RunParameters rp) -> Ok rp
                | Ok _ -> Error "Unexpected output data type: expected RunParameters"
                | Error err -> Error err
        }
    
    let getSorterSetAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sorterSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterSet
            return 
                match result with
                | Ok (SorterSet ss) -> Ok ss
                | Ok _ -> Error "Unexpected output data type: expected SorterSet"
                | Error err -> Error err
        }
    
    let getSortableTestSetAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sortableTestSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SortableTestSet
            return 
                match result with
                | Ok (SortableTestSet sts) -> Ok sts
                | Ok _ -> Error "Unexpected output data type: expected SortableTestSet"
                | Error err -> Error err
        }
    
    let getSorterModelSetMakerAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sorterModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterModelSetMaker
            return 
                match result with
                | Ok (SorterModelSetMaker smsm) -> Ok smsm
                | Ok _ -> Error "Unexpected output data type: expected SorterModelSetMaker"
                | Error err -> Error err
        }
    
    let getSortableTestModelSetAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sortableTestModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SortableTestModelSet
            return 
                match result with
                | Ok (SortableTestModelSet stms) -> Ok stms
                | Ok _ -> Error "Unexpected output data type: expected SortableTestModelSet"
                | Error err -> Error err
        }
    
    let getSortableTestModelSetMakerAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sortableTestModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SortableTestModelSetMaker
            return 
                match result with
                | Ok (SortableTestModelSetMaker stmsm) -> Ok stmsm
                | Ok _ -> Error "Unexpected output data type: expected SortableTestModelSetMaker"
                | Error err -> Error err
        }
    
    let getSorterSetEvalAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sorterSetEval, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterSetEval
            return 
                match result with
                | Ok (SorterSetEval sse) -> Ok sse
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEval"
                | Error err -> Error err
        }
    
    let getSorterSetEvalBinsAsync (geneSortDb: IGeneSortDb) (runParameters: runParameters) : Async<Result<sorterSetEvalBins, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync (Some runParameters) outputDataType.SorterSetEvalBins
            return 
                match result with
                | Ok (SorterSetEvalBins sseb) -> Ok sseb
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEvalBins"
                | Error err -> Error err
        }
    
    let getProjectAsync (geneSortDb: IGeneSortDb) : Async<Result<project, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync None outputDataType.Project
            return 
                match result with
                | Ok (Project p) -> Ok p
                | Ok _ -> Error "Unexpected output data type: expected Project"
                | Error err -> Error err
        }