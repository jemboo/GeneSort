namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core


type run =
    private
        {
          dataBaseName: string<databaseName>
          runName: string<runName>
          description: string
        }
    with

    static member create
            (databaseName: string<databaseName>)
            (runName: string<runName>)
            (description: string) : run =

        if String.IsNullOrWhiteSpace %databaseName then
            failwith "Query name cannot be empty"
        {
          dataBaseName = databaseName
          runName = runName
          description = description
        }

    member this.DatabaseName with get () = this.dataBaseName
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

