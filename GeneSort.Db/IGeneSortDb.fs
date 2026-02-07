namespace GeneSort.Db
open System
open System.Threading
open FSharp.UMX
open GeneSort.Runs
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorter
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults

[<Measure>] type projectFolder
[<Measure>] type allowOverwrite

type OutputError = string

type IGeneSortDb =
    abstract member saveAsync : string<projectFolder> -> queryParams -> outputData -> bool<allowOverwrite> -> Async<Result<unit, string>>
    abstract member loadAsync : string<projectFolder> -> queryParams -> Async<Result<outputData, OutputError>>
    abstract member getAllProjectNamesAsync : unit -> Async<Result<string<projectName>[], string>>
    abstract member getProjectRunParametersForReplRangeAsync :
            string<projectFolder> ->
            int<replNumber> option ->
            int<replNumber> option ->
            CancellationToken option ->
            IProgress<string> option ->
            Async<Result<runParameters[], string>>
    abstract member saveAllRunParametersAsync :
            string<projectFolder> ->
            runParameters[] ->
            (runParameters -> outputDataType -> queryParams) ->
            bool<allowOverwrite> ->
            CancellationToken option ->
            IProgress<string> option -> Async<Result<unit, string>>

module GeneSortDb =

    let private unwrapOutput (extractor: outputData -> 'Domain option) (result: Result<outputData, OutputError>) =
        match result with
        | Ok data -> 
            match extractor data with
            | Some domain -> Ok domain
            | None -> Error "Unexpected output data type"
        | Error err -> Error err

    let getRunParametersAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<runParameters, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.RunParameters rp -> Some rp | _ -> None) result
        }

    let getSorterSetAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sorterSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SorterSet ss -> Some ss | _ -> None) result
        }

    let getSortableTestSetAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sortableTestSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SortableTestSet sts -> Some sts | _ -> None) result
        }

    let getSorterModelSetAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sorterModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SorterModelSet smsm -> Some smsm | _ -> None) result
        }

    let getSorterModelSetMakerAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sorterModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SorterModelSetMaker smsm -> Some smsm | _ -> None) result
        }

    let getSortableTestModelSetAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sortableTestModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SortableTestModelSet stms -> Some stms | _ -> None) result
        }

    let getSortableTestModelSetMakerAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sortableTestModelSetMaker, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SortableTestModelSetMaker stmsm -> Some stmsm | _ -> None) result
        }

    let getSorterSetEvalAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sorterModelSetEval, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SorterSetEval sse -> Some sse | _ -> None) result
        }

    let getSorterSetEvalBinsAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<sorterSetEvalBins, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.SorterSetEvalBins sseb -> Some sseb | _ -> None) result
        }

    let getProjectAsync (geneSortDb: IGeneSortDb) (projectFolder: string<projectFolder>) (queryParams: queryParams) : Async<Result<project, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync projectFolder queryParams
            return unwrapOutput (function | outputData.Project p -> Some p | _ -> None) result
        }