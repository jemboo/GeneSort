
namespace GeneSort.Project

open System
open FSharp.UMX
open GeneSort.Sorter
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System.IO
open GeneSort.Core.Combinatorics
open System.Threading.Tasks

// Workspace type
type Workspace = 
    private
        { 
          name: string
          description: string
          rootDirectory: string
          parameterSets: list<string * list<string>>
        }
    with
    static member create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (parameterSets: list<string * list<string>>) : Workspace =
        if String.IsNullOrWhiteSpace name then
            failwith "Workspace name cannot be empty"
        else if String.IsNullOrWhiteSpace rootDirectory then
            failwith "Root directory cannot be empty"
        else if parameterSets.Length = 0 then
            failwith "Parameter sets cannot be empty"
        else if parameterSets |> List.exists (fun (_, values) -> values.Length = 0) then
            failwith "Each parameter must have at least one value"
        else
            { name = name
              description = description
              rootDirectory = rootDirectory
              parameterSets = parameterSets }

    member this.Name with get () = this.name
    member this.Description with get () = this.description
    member this.RootDirectory with get () = this.rootDirectory
    member this.ParameterSets with get () = this.parameterSets
    member this.WorkspaceFolder with get() = Path.Combine(this.RootDirectory, this.Name)