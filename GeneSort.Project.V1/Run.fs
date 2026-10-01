namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core


type run =
    private
        {
          dataBaseName: string<databaseName>
          projName:     string<projectName>
          runName: string<runName>
          description: string
          parameterSpans: (string * string list) list
          genSaveIntervalsName: string
          genSaveSubIntervalsName: string
          queryCatalogName: string
        }
    with

    static member create
            (databaseName: string<databaseName>)
            (projName:     string<projectName>)
            (runName: string<runName>)
            (description: string)
            (parameterSpans: (string * string list) list) : run =
        run.createWithIntervalNames databaseName projName runName description parameterSpans "expInterval100_L50ss" "summaryInterval_C.1p5C"

    static member createWithIntervalNames
            (databaseName: string<databaseName>)
            (projName: string<projectName>)
            (runName: string<runName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (genSaveIntervalsName: string)
            (genSaveSubIntervalsName: string) : run =
        run.createWithCatalogAndIntervalNames databaseName projName runName description parameterSpans genSaveIntervalsName genSaveSubIntervalsName (sprintf "%s.%s" %projName %databaseName)

    static member createWithCatalogAndIntervalNames
            (databaseName: string<databaseName>)
            (projName: string<projectName>)
            (runName: string<runName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (genSaveIntervalsName: string)
            (genSaveSubIntervalsName: string)
            (queryCatalogName: string) : run =

        if String.IsNullOrWhiteSpace %databaseName then
            failwith "Query name cannot be empty"
        {
          dataBaseName = databaseName
          projName     = projName
          runName = runName
          description = description
          parameterSpans = parameterSpans
          genSaveIntervalsName = genSaveIntervalsName
          genSaveSubIntervalsName = genSaveSubIntervalsName
          queryCatalogName = queryCatalogName
        }

    member this.DatabaseName with get () = this.dataBaseName
    member this.ProjectName with get () = this.projName
    member this.RunName with get () = this.runName
    member this.Description with get () = this.description
    member this.ParameterSpans with get () = this.parameterSpans
    member this.GenSaveIntervalsName with get () = this.genSaveIntervalsName
    member this.GenSaveSubIntervalsName with get () = this.genSaveSubIntervalsName
    member this.QueryCatalogName with get () = this.queryCatalogName




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

