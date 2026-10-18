namespace GeneSort.Project.V1

open GeneSort.Project.V1.Catalogs.Evolution
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1.Parameters


type simpleRun =
    private
        {
          dataBaseName: string<databaseName>
          projName: string<projectName>
          runName: string<runName>
          description: string
          parameterSpans: (string * string list) list
          queryCatalogName: string
          setupCatalogName: string
          enhancerCatalogName: string
          allowOverwrite: bool
          executeFunctionName: string
        }

module SimpleRun =
    let create
            (executeFunctionName: string)
            (databaseName: string<databaseName>)
            (projectName: string<projectName>)
            (runName: string<runName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (queryCatalogName: string)
            (setupCatalogName: string)
            (enhancerCatalogName: string)
            (allowOverwrite: bool) : simpleRun =
        if String.IsNullOrWhiteSpace executeFunctionName then
            invalidArg (nameof executeFunctionName) "Execute function name cannot be empty."
        if String.IsNullOrWhiteSpace setupCatalogName then
            invalidArg (nameof setupCatalogName) "Setup catalog name cannot be empty."
        if String.IsNullOrWhiteSpace %databaseName then
            failwith "Database name cannot be empty"
        {
          dataBaseName = databaseName
          projName = projectName
          runName = runName
          description = description
          parameterSpans = parameterSpans
          queryCatalogName = queryCatalogName
          setupCatalogName = setupCatalogName
          enhancerCatalogName = enhancerCatalogName
          allowOverwrite = allowOverwrite
          executeFunctionName = executeFunctionName
        }

    let databaseName (run: simpleRun) = run.dataBaseName
    let projectName (run: simpleRun) = run.projName
    let runName (run: simpleRun) = run.runName
    let description (run: simpleRun) = run.description
    let parameterSpans (run: simpleRun) = run.parameterSpans
    let queryCatalogName (run: simpleRun) = run.queryCatalogName
    let setupCatalogName (run: simpleRun) = run.setupCatalogName
    let withSetupCatalogName name (run: simpleRun) =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Setup catalog name cannot be empty."
        { run with setupCatalogName = name }
    let enhancerCatalogName (run: simpleRun) = run.enhancerCatalogName
    let allowOverwrite (run: simpleRun) = run.allowOverwrite
    let executeFunctionName (run: simpleRun) = run.executeFunctionName
    let withExecuteFunctionName name (run: simpleRun) =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Execute function name cannot be empty."
        { run with executeFunctionName = name }

type sgdRun =
    private
        {
          baseRun: simpleRun
          genSaveIntervalsName: string
          genSaveSubIntervalsName: string
          makerNames: evolutionMakerNames
        }

module SgdRun =
    let create
            (makerNames: evolutionMakerNames)
            (executeFunctionName: string)
            (databaseName: string<databaseName>)
            (projectName: string<projectName>)
            (runName: string<runName>)
            (description: string)
            (parameterSpans: (string * string list) list)
            (genSaveIntervalsName: string)
            (genSaveSubIntervalsName: string)
            (queryCatalogName: string)
            (setupCatalogName: string)
            (enhancerCatalogName: string)
            (allowOverwrite: bool) : sgdRun =
        EvolutionMakerNames.validate makerNames
        if String.IsNullOrWhiteSpace genSaveIntervalsName then
            invalidArg (nameof genSaveIntervalsName) "Generation save interval name cannot be empty"
        if String.IsNullOrWhiteSpace genSaveSubIntervalsName then
            invalidArg (nameof genSaveSubIntervalsName) "Generation summary interval name cannot be empty"
        {
          baseRun =
              SimpleRun.create
                  executeFunctionName
                  databaseName
                  projectName
                  runName
                  description
                  parameterSpans
                  queryCatalogName
                  setupCatalogName
                  enhancerCatalogName
                  allowOverwrite
          genSaveIntervalsName = genSaveIntervalsName
          genSaveSubIntervalsName = genSaveSubIntervalsName
          makerNames = makerNames
        }

    let baseRun (run: sgdRun) = run.baseRun
    let makerNames (run: sgdRun) = run.makerNames
    let executeFunctionName (run: sgdRun) = SimpleRun.executeFunctionName run.baseRun
    let withExecuteFunctionName name (run: sgdRun) =
        { run with baseRun = SimpleRun.withExecuteFunctionName name run.baseRun }
    let genSaveIntervalsName (run: sgdRun) = run.genSaveIntervalsName
    let genSaveSubIntervalsName (run: sgdRun) = run.genSaveSubIntervalsName
    let withSetupCatalogName name (run: sgdRun) =
        { run with baseRun = SimpleRun.withSetupCatalogName name run.baseRun }

type run =
    | SimpleRun of simpleRun
    | SgdRun of sgdRun
    member this.BaseRun =
        match this with
        | SimpleRun value -> value
        | SgdRun value -> SgdRun.baseRun value
    member this.DatabaseName = SimpleRun.databaseName this.BaseRun
    member this.ProjectName = SimpleRun.projectName this.BaseRun
    member this.RunName = SimpleRun.runName this.BaseRun
    member this.Description = SimpleRun.description this.BaseRun
    member this.ParameterSpans = SimpleRun.parameterSpans this.BaseRun
    member this.QueryCatalogName = SimpleRun.queryCatalogName this.BaseRun
    member this.SetupCatalogName with get() = SimpleRun.setupCatalogName this.BaseRun
    member this.WithSetupCatalogName(name) =
        match this with
        | SimpleRun value -> SimpleRun (SimpleRun.withSetupCatalogName name value)
        | SgdRun value -> SgdRun (SgdRun.withSetupCatalogName name value)
    member this.EnhancerCatalogName = SimpleRun.enhancerCatalogName this.BaseRun
    member this.AllowOverwrite = SimpleRun.allowOverwrite this.BaseRun
    member this.GenSaveIntervalsName =
        match this with
        | SimpleRun _ -> None
        | SgdRun value -> Some (SgdRun.genSaveIntervalsName value)
    member this.MakerNames =
        match this with
        | SimpleRun _ -> None
        | SgdRun value -> Some (SgdRun.makerNames value)
    member this.ExecuteFunctionName = SimpleRun.executeFunctionName this.BaseRun
    member this.WithExecuteFunctionName(name) =
        match this with
        | SimpleRun value -> SimpleRun (SimpleRun.withExecuteFunctionName name value)
        | SgdRun value -> SgdRun (SgdRun.withExecuteFunctionName name value)
    member this.GenSaveSubIntervalsName =
        match this with
        | SimpleRun _ -> None
        | SgdRun value -> Some (SgdRun.genSaveSubIntervalsName value)




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

