
namespace GeneSort.Runs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Runs.Params

type project = 
    private
        { 
          projectName: string<projectName>
          description: string
          runParametersArray: runParameters []
          parameterKeys: string array
          reportNames: string array
        }
    with
    static member create 
            (projectName: string<projectName>) 
            (description: string) 
            (runParametersArray: runParameters []) 
            (reportKeys: string array) : project =
        if String.IsNullOrWhiteSpace %projectName then
            failwith "Project name cannot be empty"
        else
            { 
              projectName = projectName
              description = description
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
            ("FullBoolEvals" |> UMX.tag<projectName>)
            "A test project"
            [| runParameters.create (Map.ofList [ ("Param1", "Value1"); ("Param2", "ValueA") ])
               runParameters.create (Map.ofList [ ("Param1", "Value2"); ("Param2", "ValueB") ]) |]
            [| "Report1"; "Report2" |]

    member this.ProjectName with get () = this.projectName
    member this.Description with get () = this.description
    member this.ParameterKeys with get() = this.parameterKeys
    member this.ReportNames with get() = this.reportNames
    member this.RunParametersArray with get () = this.runParametersArray


module Project =  

    let create 
            (projectName: string<projectName>) 
            (description: string) 
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
                projectName 
                description
                refinedParameters
                reportKeys


    let repl1s() : string*string list =
            (runParameters.replKey, [0;] |> List.map(fun d -> d.ToString()))

    let repl4s() : string*string list =
            (runParameters.replKey, [0; 1; 2; 3;] |> List.map(fun d -> d.ToString()))