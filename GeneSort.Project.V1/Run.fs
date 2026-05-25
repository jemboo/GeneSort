namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core


type run =
    private
        {
          queryName: string<queryName>
          runName: string<runName>
          description: string
        }
    with

    static member create
            (queryName: string<queryName>)
            (runName: string<runName>)
            (description: string) : run =

        if String.IsNullOrWhiteSpace %queryName then
            failwith "Query name cannot be empty"
        {
          queryName = queryName
          runName = runName
          description = description
        }

    member this.QueryName with get () = this.queryName
    member this.RunName with get () = this.runName
    member this.Description with get () = this.description




module Run =

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

