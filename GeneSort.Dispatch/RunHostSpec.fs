namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1


type IRunHost =
    abstract member RunDb: IGeneSortDb
    abstract member Run: run
    abstract member ParameterSpans: (string * string list) list
    abstract member AllowOverwrite: bool<allowOverwrite>
    abstract member ParamMapRefiner: runParameters seq -> runParameters seq
    abstract member MaxParallel: int



type runHostSpec = {
    DatabaseName: string<databaseName>
    RunName: string<runName>
    RunDescription: string
    Spans: (string * string list) list
    // Logic Callbacks
    Filter: runParameters -> runParameters option
    Enhancer: IRunHost -> runParameters -> runParameters
    // Domain Settings
    AllowOverwrite: bool<allowOverwrite>
    MaxParallel: int
}
