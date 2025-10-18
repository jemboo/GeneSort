
namespace GeneSort.Runs

open System
open System.IO
open GeneSort.Core
open GeneSort.Runs.Params

type project = 
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
            (reportKeys: string array) : project =
        if String.IsNullOrWhiteSpace name then
            failwith "Project name cannot be empty"
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
        project.create 
            "FullBoolEvals" 
            "A test project" 
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
    member this.ProjectFolder with get() = Path.Combine(this.RootDirectory, this.Name)


module Project =  

    let create 
            (name: string) 
            (description: string) 
            (rootDirectory: string) 
            (reportKeys: string array)
            (parameterSpans: (string * string list) list) 
            (paramRefiner: runParameters seq -> runParameters seq)        
             : project =

        let refinedParameters = 
            parameterSpans |> Combinatorics.cartesianProductMaps
                           |> Seq.map (fun paramMap -> runParameters.create paramMap)
                           |> paramRefiner
                           |> Seq.toArray

        project.create 
                name 
                description 
                rootDirectory 
                refinedParameters
                reportKeys


    let repl1s() : string*string list =
            (runParameters.replKey, [0;] |> List.map(fun d -> d.ToString()))

    let repl4s() : string*string list =
            (runParameters.replKey, [0; 1; 2; 3;] |> List.map(fun d -> d.ToString()))