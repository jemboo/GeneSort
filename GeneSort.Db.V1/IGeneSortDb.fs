namespace GeneSort.Db.V1
open System
open System.Threading
open FSharp.UMX
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting

[<Measure>] type projectFolder
[<Measure>] type allowOverwrite

type OutputError = string

//type IGeneSortDb =
//    abstract member projectFolder : string<projectFolder>
//    abstract member saveAsync : queryParams -> outputData -> bool<allowOverwrite> -> Async<Result<unit, string>>
//    abstract member loadAsync : queryParams -> Async<Result<outputData, OutputError>>
//    abstract member getAllProjectNamesAsync : unit -> Async<Result<string<projectName>[], string>>
//    abstract member getProjectRunParametersForReplRangeAsync :
//            int<replNumber> option ->
//            int<replNumber> option ->
//            CancellationToken option ->
//            IProgress<string> option ->
//            Async<Result<runParameters[], string>>
//    abstract member saveAllRunParametersAsync :
//            runParameters[] ->
//            (runParameters -> outputDataType -> queryParams) ->
//            bool<allowOverwrite> ->
//            CancellationToken option ->
//            IProgress<string> option -> Async<Result<unit, string>>