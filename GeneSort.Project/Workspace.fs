
namespace GeneSort.Project

open System
open System.IO
open GeneSort.Core

type workspace = 
    private
        { 
          name: string
          description: string
          rootDirectory: string
          runParametersArray: runParameters []
          parameterKeys: string array
          reportNames: string array
        }
    with
    static member create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (runParametersArray: runParameters []) 
            (reportKeys: string array) : workspace =
        if String.IsNullOrWhiteSpace name then
            failwith "Workspace name cannot be empty"
        else
            { name = name
              description = description
              rootDirectory = rootDirectory
              runParametersArray = runParametersArray
              reportNames = reportKeys
              parameterKeys = 
                    if runParametersArray.Length = 0 then
                        [||]
                    else
                        runParametersArray.[0].ParamMap |> Map.toSeq |> Seq.map fst |> Seq.toArray
            }

    static member Test = 
        workspace.create 
            "FullBoolEvals" 
            "A test workspace" 
            $"C:\Projects"
            [| runParameters.create (Map.ofList [ ("Param1", "Value1"); ("Param2", "ValueA") ])
               runParameters.create (Map.ofList [ ("Param1", "Value2"); ("Param2", "ValueB") ]) |]
            [| "Report1"; "Report2" |]

    member this.Name with get () = this.name
    member this.Description with get () = this.description
    member this.ParameterKeys with get() = this.parameterKeys
    member this.ReportNames with get() = this.reportNames
    member this.RunParametersArray with get () = this.runParametersArray
    member this.RootDirectory with get () = this.rootDirectory
    member this.WorkspaceFolder with get() = Path.Combine(this.RootDirectory, this.Name)


module Workspace =  
    let create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (reportKeys: string array)
            (parameterSpans: (string * string list) list) 
            (paramRefiner: runParameters seq -> runParameters seq)        
             : workspace =

        let refinedParameters = 
            parameterSpans |> Combinatorics.cartesianProductMaps
                           |> Seq.map (fun paramMap -> runParameters.create paramMap)
                           |> paramRefiner
                           |> Seq.toArray

        workspace.create 
                name 
                description 
                rootDirectory 
                refinedParameters
                reportKeys

                 
    let filterByParameters (workspace: workspace) (filter: (string * string) array) : runParameters [] =
        workspace.RunParametersArray
        |> Array.filter (
            fun runParameters ->
                filter |> Array.forall (fun (key, value) ->
                    runParameters.ParamMap.ContainsKey key && runParameters.ParamMap.[key] = value
                ))