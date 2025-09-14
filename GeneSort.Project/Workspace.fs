
namespace GeneSort.Project

open System
open System.IO
open GeneSort.Core

// Workspace type
type workspace = 
    private
        { 
          name: string
          description: string
          rootDirectory: string
          paramMapArray: Map<string, string> []
        }
    with
    static member create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (paramMapArray: Map<string, string> []) : workspace =
        if String.IsNullOrWhiteSpace name then
            failwith "Workspace name cannot be empty"
        else
            { name = name
              description = description
              rootDirectory = rootDirectory
              //parameterArray = parameterSets |> cartesianProductMaps |> Seq.toArray 
              paramMapArray = paramMapArray
            }

    member this.Name with get () = this.name
    member this.Description with get () = this.description
    member this.ParamMapArray with get () = this.paramMapArray
    member this.RootDirectory with get () = this.rootDirectory
    member this.WorkspaceFolder with get() = Path.Combine(this.RootDirectory, this.Name)


module Workspace =  
    let create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (parameterSpans: (string * string list) list) : workspace =
        workspace.create 
                name 
                description 
                rootDirectory 
                (parameterSpans |> Combinatorics.cartesianProductMaps |> Seq.toArray )
                 

    let filterByParameters (workspace: workspace) (filter: (string * string) array) : Map<string, string> [] =
            workspace.ParamMapArray
            |> Array.filter (fun paramMap ->
                filter
                |> Array.forall (fun (key, value) ->
                    paramMap.ContainsKey key && paramMap.[key] = value
                )
            )