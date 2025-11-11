namespace GeneSort.Db

open System
open System.Threading

open FSharp.UMX

open GeneSort.Core
open GeneSort.Runs
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults


type IGeneSortDb =
    abstract member saveAsync : queryParams -> outputData -> Async<unit>
    abstract member loadAsync : queryParams -> Async<Result<outputData, OutputError>>
    abstract member getAllProjectNamesAsync : 
                unit -> Async<Result<string<projectName>[], string>>

    abstract member getAllProjectRunParametersAsync : 
            string<projectName> -> 
            CancellationToken option -> 
            IProgress<string> option -> 
            Async<Result<runParameters[], string>>

    abstract member saveAllRunParametersAsync : 
            runParameters[] -> 
            CancellationToken option -> 
            IProgress<string> option -> Async<unit>




module GeneSortDb =

    let makeOutputDataName (queryParams: queryParams) : string =
        match queryParams.OutputDataType with
        | outputDataType.Project -> "Project"
        | outputDataType.RunParameters ->
            match queryParams.Index, queryParams.Repl with
                | Some idx, Some rpl -> sprintf "RunParameters_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for RunParameters output data type"

        | outputDataType.SorterSet _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SorterSet_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SorterSet_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SorterSet output data type"

        | outputDataType.SortableTestSet _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SortableTestSet_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SortableTestSet_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SortableTestSet output data type"
    
        | outputDataType.SorterModelSet _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SorterModelSet_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SorterModelSet_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SorterModelSet output data type"

        | outputDataType.SorterModelSetMaker _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SorterModelSetMaker_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SorterModelSetMaker_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SorterModelSetMaker output data type"

        | outputDataType.SortableTestModelSet _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SortableTestModelSet_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SortableTestModelSet_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SortableTestModelSet output data type"

        | outputDataType.SortableTestModelSetMaker _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SortableTestModelSetMaker_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SortableTestModelSetMaker_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SortableTestModelSetMaker output data type"

        | outputDataType.SorterSetEval _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SorterSetEval_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SorterSetEval_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SorterSetEval output data type"

        | outputDataType.SorterSetEvalBins _ ->
            match queryParams.Index, queryParams.Repl, queryParams.Generation with
                | Some idx, Some rpl, Some gen -> sprintf "SorterSetEvalBins_Index%d_Repl%d_Gen%d" (%idx) (%rpl) (%gen)
                | Some idx, Some rpl, None -> sprintf "SorterSetEvalBins_Index%d_Repl%d" (%idx) (%rpl)
                | _ -> failwith "Index and Repl must be specified for SorterSetEvalBins output data type"

        | outputDataType.TextReport tr -> %tr



    let getRunParametersAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<runParameters, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.RunParameters rp) -> Ok rp
                | Ok _ -> Error "Unexpected output data type: expected RunParameters"
                | Error err -> Error err
        }
    
    let getSorterSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SorterSet ss) -> Ok ss
                | Ok _ -> Error "Unexpected output data type: expected SorterSet"
                | Error err -> Error err
        }
    
    let getSortableTestSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SortableTestSet sts) -> Ok sts
                | Ok _ -> Error "Unexpected output data type: expected SortableTestSet"
                | Error err -> Error err
        }

    let getSorterModelSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SorterModelSet smsm) -> Ok smsm
                | Ok _ -> Error "Unexpected output data type: expected SorterModelSet"
                | Error err -> Error err
        }
    
    let getSorterModelSetMakerAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SorterModelSetMaker smsm) -> Ok smsm
                | Ok _ -> Error "Unexpected output data type: expected SorterModelSetMaker"
                | Error err -> Error err
        }
    
    let getSortableTestModelSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SortableTestModelSet stms) -> Ok stms
                | Ok _ -> Error "Unexpected output data type: expected SortableTestModelSet"
                | Error err -> Error err
        }
    
    let getSortableTestModelSetMakerAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SortableTestModelSetMaker stmsm) -> Ok stmsm
                | Ok _ -> Error "Unexpected output data type: expected SortableTestModelSetMaker"
                | Error err -> Error err
        }
    
    let getSorterSetEvalAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSetEval, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SorterSetEval sse) -> Ok sse
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEval"
                | Error err -> Error err
        }
    
    let getSorterSetEvalBinsAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSetEvalBins, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.SorterSetEvalBins sseb) -> Ok sseb
                | Ok _ -> Error "Unexpected output data type: expected SorterSetEvalBins"
                | Error err -> Error err
        }
    
    let getProjectAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<project, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return 
                match result with
                | Ok (outputData.Project p) -> Ok p
                | Ok _ -> Error "Unexpected output data type: expected Project"
                | Error err -> Error err
        }