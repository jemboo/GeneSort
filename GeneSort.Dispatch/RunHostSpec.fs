namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1


type runHostSpec = {
    QueryName: string<queryName>
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
