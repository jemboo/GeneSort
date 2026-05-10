namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open GeneSort.Sorting.Sortable


type runHostSpec = {
    ProjectName: string<projectName>
    RunName: string<runName>
    RunDescription: string
    DataFolder: string
    Spans: (string * string list) list
    // Logic Callbacks
    Filter: runParameters -> runParameters option
    Enhancer: IRunHost -> runParameters -> runParameters
    // Domain Settings
    AllowOverwrite: bool<allowOverwrite>
}
