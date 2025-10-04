
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
          reportKeys: string array
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
              reportKeys = reportKeys
              parameterKeys = 
                    if runParametersArray.Length = 0 then
                        [||]
                    else
                        runParametersArray.[0].ParamMap |> Map.toSeq |> Seq.map fst |> Seq.toArray
            }

    member this.Name with get () = this.name
    member this.Description with get () = this.description
    member this.ParameterKeys with get() = this.parameterKeys
    member this.ReportKeys with get() = this.reportKeys
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
                reportKeys

                 
    let filterByParameters (workspace: workspace) (filter: (string * string) array) : runParameters [] =
        workspace.RunParametersArray
        |> Array.filter (
            fun runParameters ->
                filter |> Array.forall (fun (key, value) ->
                    runParameters.ParamMap.ContainsKey key && runParameters.ParamMap.[key] = value
                ))






type workspace2 = 
    private
        { 
          name: string
          description: string
          rootDirectory: string
          runParametersArray: runParameters []
          parameterKeys: string array
          reportKeys: string array
        }
    with
    static member create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (runParametersArray: runParameters []) 
            (reportKeys: string array) : workspace2 =
        if String.IsNullOrWhiteSpace name then
            failwith "Workspace name cannot be empty"
        else
            { name = name
              description = description
              rootDirectory = rootDirectory
              runParametersArray = runParametersArray
              reportKeys = reportKeys
              parameterKeys = 
                    if runParametersArray.Length = 0 then
                        [||]
                    else
                        runParametersArray.[0].ParamMap |> Map.toSeq |> Seq.map fst |> Seq.toArray
            }

    member this.Name with get () = this.name
    member this.Description with get () = this.description
    member this.ParameterKeys with get() = this.parameterKeys
    member this.ReportKeys with get() = this.reportKeys
    member this.RunParametersArray with get () = this.runParametersArray
    member this.RootDirectory with get () = this.rootDirectory
    member this.WorkspaceFolder with get() = Path.Combine(this.RootDirectory, this.Name)


//module Workspace2 =  
//    let create 
//            (name: string) 
//            (description: string) 
//            (rootDirectory: string) 
//            (reportKeys: string array)
//            (parameterSpans: (string * string list) list) 
//            (paramRefiner: runParameters -> runParameters option)        
//             : workspace2 =

//        let refinedParameters = 
//            parameterSpans |> Combinatorics.cartesianProductMaps
//                           |> Seq.map (fun paramMap -> runParameters.create paramMap)
//                           |> Seq.map (paramRefiner)
//                           |> Seq.choose id
//                           |> Seq.toArray
//        workspace2.create 
//                name 
//                description 
//                rootDirectory 
//                refinedParameters
//                reportKeys

                 
//    let filterByParameters (workspace: workspace2) (filter: (string * string) array) : runParameters [] =
//        workspace.RunParametersArray
//        |> Array.filter (
//            fun runParameters ->
//                filter |> Array.forall (fun (key, value) ->
//                    runParameters.ParamMap.ContainsKey key && runParameters.ParamMap.[key] = value
//                ))





