namespace GeneSort.Project.V1.Storage
open System
open System.Threading
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

[<Measure>] type allowOverwrite

type OutputError = string

type IGeneSortDb =
    abstract member databaseName : string<databaseName>

    abstract member saveAsync : 
                        geneSortDataMode ->
                        queryParams -> 
                        outputData -> 
                        bool<allowOverwrite> -> 
                            Async<Result<unit, string>>

    abstract member loadAsync : 
                        geneSortDataMode ->
                        queryParams -> 
                            Async<Result<outputData, OutputError>>

    abstract member loadIfFoundAsync : 
                        geneSortDataMode ->
                        queryParams -> 
                            Async<outputData option>

    abstract member doesOutPutDataExist : 
                        geneSortDataMode ->
                        queryParams -> 
                            Async<bool>

    abstract member getRunParameters :
                        string<runName> ->
                        int<replNumber> option ->
                        int<replNumber> option ->
                        CancellationToken option ->
                        IProgress<string> option ->
                                Async<Result<runParameters[], string>>

    abstract member makeQueryParamsFromRunParams :
                        runParameters ->
                        geneSortDataType ->
                        queryParams option

/// Opens a source database without prescribing a filesystem or database implementation.
type sourceDatabaseOpener = string<projectName> -> string<databaseName> -> string -> IGeneSortDb
