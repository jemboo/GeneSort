namespace GeneSort.Db.V1
open System
open System.Threading
open FSharp.UMX
open GeneSort.Eval.V1
open GeneSort.Project.V1

[<Measure>] type allowOverwrite

type OutputError = string

type IGeneSortDb =
    abstract member databaseName : string<databaseName>

    abstract member saveAsync : 
                        queryParams -> 
                        outputData -> 
                        bool<allowOverwrite> -> 
                            Async<Result<unit, string>>

    abstract member loadAsync : 
                        queryParams -> 
                            Async<Result<outputData, OutputError>>

    abstract member loadIfFoundAsync : 
                        queryParams -> 
                            Async<outputData option>

    abstract member getNextGenerationalItemAsync :
                        queryParams ->
                        (queryParams -> int<generationNumber> -> queryParams) ->
                        outputData option ->
                            Async<outputData option>

    abstract member getRunParameters :
                        string<runName> ->
                        int<replNumber> option ->
                        int<replNumber> option ->
                        CancellationToken option ->
                        IProgress<string> option ->
                                Async<Result<runParameters[], string>>

    abstract member MakeQueryParamsFromRunParams :
                        runParameters ->
                        outputDataType ->
                        queryParams option