namespace GeneSort.Db.V1
open System
open System.Threading
open FSharp.UMX
open GeneSort.Project.V1

[<Measure>] type projectFolder
[<Measure>] type allowOverwrite

type OutputError = string

type IGeneSortDb =
    abstract member projectName : string<projectName>
    abstract member saveAsync : queryParams -> outputData -> bool<allowOverwrite> -> Async<Result<unit, string>>
    abstract member loadAsync : queryParams -> Async<Result<outputData, OutputError>>
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

