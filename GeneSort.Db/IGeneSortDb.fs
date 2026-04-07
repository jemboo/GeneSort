namespace GeneSort.Db
open System
open System.Threading
open FSharp.UMX
open GeneSort.Runs
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting
open GeneSort.SortingResults.Bins

[<Measure>] type projectFolder
[<Measure>] type allowOverwrite

type OutputError = string

type IGeneSortDb =
    abstract member projectFolder : string<projectFolder>
    abstract member saveAsync : queryParams -> outputData -> bool<allowOverwrite> -> Async<Result<unit, string>>
    abstract member loadAsync : queryParams -> Async<Result<outputData, OutputError>>
    abstract member getAllProjectNamesAsync : unit -> Async<Result<string<projectName>[], string>>
    abstract member getProjectRunParametersForReplRangeAsync :
            int<replNumber> option ->
            int<replNumber> option ->
            CancellationToken option ->
            IProgress<string> option ->
            Async<Result<runParameters[], string>>
    abstract member saveAllRunParametersAsync :
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

    let getRunParametersAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<runParameters, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.RunParameters rp -> Some rp | _ -> None) result
        }

    let getSorterSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SorterSet ss -> Some ss | _ -> None) result
        }

    let getSortableTestSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SortableTestSet sts -> Some sts | _ -> None) result
        }

    let getSorterModelSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortingSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SortingSet smsm -> Some smsm | _ -> None) result
        }

    let getSorterModelSetGenAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortingGenSegment, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SortingSetGen smsm -> Some smsm | _ -> None) result
        }

    let getSortableTestModelSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestModelSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SortableTestModelSet stms -> Some stms | _ -> None) result
        }

    let getSortableTestModelSetGenAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sortableTestModelSetGen, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SortableTestModelSetGen stmsm -> Some stmsm | _ -> None) result
        }

    let getSorterSetEvalAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterSetEval, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SorterSetEval sse -> Some sse | _ -> None) result
        }

    let getSorterEvalBinsAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<sorterEvalBins, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.SorterEvalBins sseb -> Some sseb | _ -> None) result
        }

    let getMutationSegmentEvalBinsSetAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<mutationSegmentEvalBinsSet, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.MutationSegmentEvalBinsSet msr -> Some msr | _ -> None) result
        }

    let getProjectAsync (geneSortDb: IGeneSortDb) (queryParams: queryParams) : Async<Result<project, OutputError>> =
        async {
            let! result = geneSortDb.loadAsync queryParams
            return unwrapOutput (function | outputData.Project p -> Some p | _ -> None) result
        }