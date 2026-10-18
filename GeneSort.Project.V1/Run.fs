namespace GeneSort.Project.V1

open GeneSort.Project.V1.Catalogs.Evolution
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1.Parameters


module private ParameterGeneration =

    let make
                (minReplica: int<replNumber>)
                (maxReplica: int<replNumber>)
                (parameterSpans: (string * string list) list)
                (paramRefiner: parameterSet<'kind> seq -> parameterSet<'kind> seq) : parameterSet<'kind> [] =

        let replicateSpan = [ parameterSet<'kind>.replKey, [%minReplica .. %maxReplica - 1] |> List.map string ]

        let fullSpans = replicateSpan @ parameterSpans
        let parametersArray =
            if fullSpans.IsEmpty then
                [| parameterSet<'kind>.create Map.empty |]
            else
                fullSpans
                |> Combinatorics.cartesianProductMaps
                |> Seq.map parameterSet<'kind>.create
                |> Seq.toArray

        let refinedParameters =
            parametersArray
            |> Array.toSeq
            |> paramRefiner
            |> Seq.toArray

        refinedParameters


type simpleRun =
    private
        {
          dataBaseName: string<databaseName>
          projName: string<projectName>
          runName: string<runName>
          description: string
          runParameterSpans: (string * string list) list
          runQueryCatalogName: string
          setupParameterSpans: (string * string list) list
          setupQueryCatalogName: string
          setupFunctionName: string
          runEnhancerFunctionName: string
          setupEnhancerFunctionName: string
          setupOutputTypes: geneSortDataType list
          allowOverwrite: bool
          executeFunctionName: string
        }

module SimpleRun =
    let makeRunParameters minReplica maxReplica refiner (run: simpleRun) : runParameters[] =
        ParameterGeneration.make minReplica maxReplica run.runParameterSpans refiner

    let makeSetupParameters minReplica maxReplica refiner (run: simpleRun) : setupParameters[] =
        ParameterGeneration.make minReplica maxReplica run.setupParameterSpans refiner

    let create
            (executeFunctionName: string)
            (databaseName: string<databaseName>)
            (projectName: string<projectName>)
            (runName: string<runName>)
            (description: string)
            (runParameterSpans: (string * string list) list)
            (runQueryCatalogName: string)
            (setupFunctionName: string)
            (runEnhancerFunctionName: string)
            (allowOverwrite: bool)
            (setupParameterSpans: (string * string list) list)
            (setupQueryCatalogName: string)
            (setupEnhancerFunctionName: string)
            (setupOutputTypes: geneSortDataType list) : simpleRun =
        if String.IsNullOrWhiteSpace executeFunctionName then
            invalidArg (nameof executeFunctionName) "Execute function name cannot be empty."
        if String.IsNullOrWhiteSpace runQueryCatalogName then
            invalidArg (nameof runQueryCatalogName) "Run query catalog name cannot be empty."
        if String.IsNullOrWhiteSpace setupFunctionName then
            invalidArg (nameof setupFunctionName) "Setup function name cannot be empty."
        for argument, value in [nameof setupQueryCatalogName, setupQueryCatalogName; nameof runEnhancerFunctionName, runEnhancerFunctionName; nameof setupEnhancerFunctionName, setupEnhancerFunctionName] do
            if String.IsNullOrWhiteSpace value then
                invalidArg argument "Function names cannot be empty."
        if String.IsNullOrWhiteSpace %databaseName then
            failwith "Database name cannot be empty"
        {
          dataBaseName = databaseName
          projName = projectName
          runName = runName
          description = description
          runParameterSpans = runParameterSpans
          runQueryCatalogName = runQueryCatalogName
          setupParameterSpans = setupParameterSpans
          setupQueryCatalogName = setupQueryCatalogName
          setupFunctionName = setupFunctionName
          runEnhancerFunctionName = runEnhancerFunctionName
          setupEnhancerFunctionName = setupEnhancerFunctionName
          setupOutputTypes = setupOutputTypes
          allowOverwrite = allowOverwrite
          executeFunctionName = executeFunctionName
        }

    let databaseName (run: simpleRun) = run.dataBaseName
    let projectName (run: simpleRun) = run.projName
    let runName (run: simpleRun) = run.runName
    let description (run: simpleRun) = run.description
    let runParameterSpans (run: simpleRun) = run.runParameterSpans
    let runQueryCatalogName (run: simpleRun) = run.runQueryCatalogName
    let setupParameterSpans (run: simpleRun) = run.setupParameterSpans
    let setupQueryCatalogName (run: simpleRun) = run.setupQueryCatalogName
    let setupFunctionName (run: simpleRun) = run.setupFunctionName
    let runEnhancerFunctionName (run: simpleRun) = run.runEnhancerFunctionName
    let setupEnhancerFunctionName (run: simpleRun) = run.setupEnhancerFunctionName
    let setupOutputTypes (run: simpleRun) = run.setupOutputTypes
    let allowOverwrite (run: simpleRun) = run.allowOverwrite
    let executeFunctionName (run: simpleRun) = run.executeFunctionName

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
            (runParameterSpans: (string * string list) list)
            (genSaveIntervalsName: string)
            (genSaveSubIntervalsName: string)
            (runQueryCatalogName: string)
            (setupFunctionName: string)
            (runEnhancerFunctionName: string)
            (allowOverwrite: bool)
            (setupParameterSpans: (string * string list) list)
            (setupQueryCatalogName: string)
            (setupEnhancerFunctionName: string)
            (setupOutputTypes: geneSortDataType list) : sgdRun =
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
                  runParameterSpans
                  runQueryCatalogName
                  setupFunctionName
                  runEnhancerFunctionName
                  allowOverwrite
                  setupParameterSpans
                  setupQueryCatalogName
                  setupEnhancerFunctionName
                  setupOutputTypes
          genSaveIntervalsName = genSaveIntervalsName
          genSaveSubIntervalsName = genSaveSubIntervalsName
          makerNames = makerNames
        }

    let baseRun (run: sgdRun) = run.baseRun
    let makerNames (run: sgdRun) = run.makerNames
    let executeFunctionName (run: sgdRun) = SimpleRun.executeFunctionName run.baseRun
    let genSaveIntervalsName (run: sgdRun) = run.genSaveIntervalsName
    let genSaveSubIntervalsName (run: sgdRun) = run.genSaveSubIntervalsName

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
    member this.RunParameterSpans = SimpleRun.runParameterSpans this.BaseRun
    member this.RunQueryCatalogName = SimpleRun.runQueryCatalogName this.BaseRun
    member this.SetupParameterSpans = SimpleRun.setupParameterSpans this.BaseRun
    member this.SetupQueryCatalogName = SimpleRun.setupQueryCatalogName this.BaseRun
    member this.SetupFunctionName with get() = SimpleRun.setupFunctionName this.BaseRun
    member this.RunEnhancerFunctionName = SimpleRun.runEnhancerFunctionName this.BaseRun
    member this.SetupEnhancerFunctionName = SimpleRun.setupEnhancerFunctionName this.BaseRun
    member this.SetupOutputTypes = SimpleRun.setupOutputTypes this.BaseRun
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
    member this.GenSaveSubIntervalsName =
        match this with
        | SimpleRun _ -> None
        | SgdRun value -> Some (SgdRun.genSaveSubIntervalsName value)




module Run =
    let makeRunParameters minReplica maxReplica spans (refiner: runParameters seq -> runParameters seq) =
        ParameterGeneration.make minReplica maxReplica spans refiner

    let makeSetupParameters minReplica maxReplica spans (refiner: setupParameters seq -> setupParameters seq) =
        ParameterGeneration.make minReplica maxReplica spans refiner
