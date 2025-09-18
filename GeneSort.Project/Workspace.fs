
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
          runParametersArray: runParameters []
        }
    with
    static member create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (runParametersArray: runParameters []) : workspace =
        if String.IsNullOrWhiteSpace name then
            failwith "Workspace name cannot be empty"
        else
            { name = name
              description = description
              rootDirectory = rootDirectory
              runParametersArray = runParametersArray
            }

    member this.Name with get () = this.name
    member this.Description with get () = this.description
    member this.RunParametersArray with get () = this.runParametersArray
    member this.RootDirectory with get () = this.rootDirectory
    member this.WorkspaceFolder with get() = Path.Combine(this.RootDirectory, this.Name)


module Workspace =  
    let create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (parameterSpans: (string * string list) list) 
            (paramRefiner: runParameters -> runParameters option)        
             : workspace =

        let refinedParameters = 
            parameterSpans |> Combinatorics.cartesianProductMaps
                           |> Seq.map (fun paramMap -> runParameters.create paramMap)
                           |> Seq.map (paramRefiner)
                           |> Seq.choose id
                           |> Seq.toArray
        workspace.create 
                name 
                description 
                rootDirectory 
                refinedParameters

                 
    let filterByParameters (workspace: workspace) (filter: (string * string) array) : runParameters [] =
        workspace.RunParametersArray
        |> Array.filter (
            fun runParameters ->
                filter |> Array.forall (fun (key, value) ->
                    runParameters.ParamMap.ContainsKey key && runParameters.ParamMap.[key] = value
                ))