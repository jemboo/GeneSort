namespace GeneSort.Project.V2.Run

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V2

type RunSortableSet =
    private
        {
          dataBaseName: string<databaseName>
          projName:     string<projectName>
          runName: string<runName>
          description: string
        }
    with
    static member create
            (databaseName: string<databaseName>)
            (projName:     string<projectName>)
            (runName: string<runName>)
            (description: string) : RunSortableSet =
        if String.IsNullOrWhiteSpace %databaseName then
            failwith "Query name cannot be empty"
        {
          dataBaseName = databaseName
          projName     = projName
          runName = runName
          description = description
        }

    member this.DatabaseName with get () = this.dataBaseName
    member this.ProjectName with get () = this.projName
    member this.RunName with get () = this.runName
    member this.Description with get () = this.description