namespace GeneSort.Runs

open System
open FSharp.UMX
open GeneSort.Core

type project =
    private
        {
          projectName: string<projectName>
          description: string
          parameterSpans: (string * string list) list
          maxReplicas: int<replNumber>
          runParametersArray: runParameters []
          parameterKeys: string array
          outputDataTypes: outputDataType array
        }
    with
    static member create
            (projectName: string<projectName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (maxReplicate: int<replNumber>)
            (runParametersArray: runParameters [])
            (outputDataTypes: outputDataType array) : project =

        if String.IsNullOrWhiteSpace %projectName then
            failwith "Project name cannot be empty"
        if %maxReplicate < 0 then
            failwith "maxReplicate must not be negative"
        {
          projectName = projectName
          description = description
          parameterSpans = parameterSpans
          maxReplicas = maxReplicate
          runParametersArray = runParametersArray
          outputDataTypes = outputDataTypes
          parameterKeys =
                if runParametersArray.Length = 0 then
                    [||]
                else
                    runParametersArray.[0].ParamMap |> Map.toSeq |> Seq.map fst |> Seq.toArray
        }

    member this.ProjectName with get () = this.projectName
    member this.Description with get () = this.description
    member this.ParameterSpans with get() = this.parameterSpans
    member this.MaxReplicate with get() = this.maxReplicas
    member this.ParameterKeys with get() = this.parameterKeys
    member this.OutputDataTypes with get() = this.outputDataTypes
    member this.RunParametersArray with get () = this.runParametersArray


    static member TestExample =

        let parameterSpans = 
            [ ("Param1", ["Value1"; "Value2"]); ("Param2", ["ValueA"; "ValueB"]) ]

        project.create
            ("FullBoolEvals" |> UMX.tag<projectName>)
            "A test project"
            parameterSpans
            1<replNumber>
            ( [|
                runParameters.create (Map.ofList [ ("Param1", "Value1"); ("Param2", "ValueA"); (runParameters.replKey, "0") ]);
                runParameters.create (Map.ofList [ ("Param1", "Value1"); ("Param2", "ValueB"); (runParameters.replKey, "0") ]);
                runParameters.create (Map.ofList [ ("Param1", "Value2"); ("Param2", "ValueA"); (runParameters.replKey, "0") ]);
                runParameters.create (Map.ofList [ ("Param1", "Value2"); ("Param2", "ValueB"); (runParameters.replKey, "0") ]);
              |] )
            [| TextReport ("Report1" |> UMX.tag<textReportName>); TextReport ("Report2" |> UMX.tag<textReportName>) |]





module Project =
    let create
            (projectName: string<projectName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (maxReplicas: int<replNumber>)
            (outputDataTypes: outputDataType array)
            (paramRefiner: runParameters seq -> runParameters seq)
             : project =

        if %maxReplicas < 0 then
            failwith "maxReplicate must not be negative"
        let replicateSpan = [ runParameters.replKey, [0 .. %maxReplicas - 1] |> List.map string ]

        let fullSpans = replicateSpan @ parameterSpans
        let runParametersArray =
            if fullSpans.IsEmpty then
                [| runParameters.create Map.empty |]
            else
                fullSpans
                |> Combinatorics.cartesianProductMaps
                |> Seq.map runParameters.create
                |> Seq.toArray

        let refinedParameters =
            runParametersArray
            |> Array.toSeq
            |> paramRefiner
            |> Seq.toArray

        project.create
                projectName
                description
                parameterSpans
                maxReplicas
                refinedParameters
                outputDataTypes


    let updateMaxReplicas (project: project) (newMaxReplicate: int<replNumber>) 
            (paramRefiner: runParameters seq -> runParameters seq) : project =
        create
            project.ProjectName
            project.Description
            project.ParameterSpans
            newMaxReplicate
            project.OutputDataTypes
            paramRefiner