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
          parameterKeys: string array
          outputDataTypes: outputDataType array
        }
    with

    static member create
            (projectName: string<projectName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (outputDataTypes: outputDataType array) : project =

        if String.IsNullOrWhiteSpace %projectName then
            failwith "Project name cannot be empty"
        {
          projectName = projectName
          description = description
          parameterSpans = parameterSpans
          outputDataTypes = outputDataTypes
          parameterKeys =
                if parameterSpans.Length = 0 then
                    [||]
                else
                    parameterSpans |> List.map(fst) |> List.toArray
        }

    member this.ProjectName with get () = this.projectName
    member this.Description with get () = this.description
    member this.ParameterSpans with get() = this.parameterSpans
    member this.ParameterKeys with get() = this.parameterKeys
    member this.OutputDataTypes with get() = this.outputDataTypes




module Project =

    let makeRunParameters 
                (minReplica: int<replNumber>) 
                (maxReplica: int<replNumber>)
                (parameterSpans: (string * string list) list)
                (paramRefiner: runParameters seq -> runParameters seq) : runParameters [] =

        let replicateSpan = [ runParameters.replKey, [%minReplica .. %maxReplica - 1] |> List.map string ]

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

        refinedParameters

