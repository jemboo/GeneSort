namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters


module private ParameterGeneration =

    let make
            (minReplica: int<replNumber>)
            (maxReplica: int<replNumber>)
            (parameterSpans: (string * string list) list)
            (paramRefiner: parameterSet<'kind> seq -> parameterSet<'kind> seq) : parameterSet<'kind> [] =

        let replicateSpan = [ ParamKeys.replKey, [%minReplica .. %maxReplica - 1] |> List.map string ]

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


/// Settings for the execution phase.
type executionConfig = {
    FunctionName: string
    ParameterSpans: (string * string list) list
    QueryCatalogName: string
    EnhancerFunctionName: string
}

/// Settings for the setup phase.
type setupConfig = {
    FunctionName: string
    ParameterSpans: (string * string list) list
    QueryCatalogName: string
    EnhancerFunctionName: string
}

/// Complete configuration validated by SimpleRun.create.
type simpleRunConfig = {
    DatabaseName: string<databaseName>
    ProjectName: string<projectName>
    RunName: string<runName>
    Description: string
    AllowOverwrite: bool
    Execution: executionConfig
    Setup: setupConfig
}

/// Additional settings validated by SgdRun.create.
type sgdRunConfig = {
    MakerNames: evolutionMakerNames
    GenSaveIntervalsName: string
    GenSaveSubIntervalsName: string
}

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
          allowOverwrite: bool
          executeFunctionName: string
          parameterSource: (simpleRun * (string * string list) list) option
        }

module SimpleRun =
    let makeRunParameters minReplica maxReplica refiner (run: simpleRun) : runParameters[] =
        ParameterGeneration.make minReplica maxReplica run.runParameterSpans refiner

    let makeSetupParameters minReplica maxReplica refiner (run: simpleRun) : setupParameters[] =
        ParameterGeneration.make minReplica maxReplica run.setupParameterSpans refiner

    let create (config: simpleRunConfig) : simpleRun =
        let executeFunctionName = config.Execution.FunctionName
        let databaseName = config.DatabaseName
        let projectName = config.ProjectName
        let runName = config.RunName
        let description = config.Description
        let runParameterSpans = config.Execution.ParameterSpans
        let runQueryCatalogName = config.Execution.QueryCatalogName
        let setupFunctionName = config.Setup.FunctionName
        let runEnhancerFunctionName = config.Execution.EnhancerFunctionName
        let allowOverwrite = config.AllowOverwrite
        let setupParameterSpans = config.Setup.ParameterSpans
        let setupQueryCatalogName = config.Setup.QueryCatalogName
        let setupEnhancerFunctionName = config.Setup.EnhancerFunctionName
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
          allowOverwrite = allowOverwrite
          executeFunctionName = executeFunctionName
          parameterSource = None
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
    let allowOverwrite (run: simpleRun) = run.allowOverwrite
    let executeFunctionName (run: simpleRun) = run.executeFunctionName
    let parameterSource (run: simpleRun) = run.parameterSource
    let withParameterSource source spans (run: simpleRun) =
        { run with parameterSource = Some (source, spans) }

type sgdRun =
    private
        {
          baseRun: simpleRun
          genSaveIntervalsName: string
          genSaveSubIntervalsName: string
          makerNames: evolutionMakerNames
        }

module SgdRun =
    let create (config: simpleRunConfig) (sgd: sgdRunConfig) : sgdRun =
        EvolutionMakerNames.validate sgd.MakerNames
        if String.IsNullOrWhiteSpace sgd.GenSaveIntervalsName then
            invalidArg "GenSaveIntervalsName" "Generation save interval name cannot be empty"
        if String.IsNullOrWhiteSpace sgd.GenSaveSubIntervalsName then
            invalidArg "GenSaveSubIntervalsName" "Generation summary interval name cannot be empty"
        {
          baseRun = SimpleRun.create config
          genSaveIntervalsName = sgd.GenSaveIntervalsName
          genSaveSubIntervalsName = sgd.GenSaveSubIntervalsName
          makerNames = sgd.MakerNames
        }

    let baseRun (run: sgdRun) = run.baseRun
    let withParameterSource source spans (run: sgdRun) =
        { run with baseRun = SimpleRun.withParameterSource source spans run.baseRun }
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
    /// Only the source's parameter-generation configuration is needed here.
    member this.ParameterSource =
        SimpleRun.parameterSource this.BaseRun
        |> Option.map (fun (source, spans) -> SimpleRun source, spans)
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
    let withParameterSource (source: run) spans = function
        | SimpleRun value -> SimpleRun (SimpleRun.withParameterSource source.BaseRun spans value)
        | SgdRun value -> SgdRun (SgdRun.withParameterSource source.BaseRun spans value)

    let makeRunParameters minReplica maxReplica spans (refiner: runParameters seq -> runParameters seq) =
        ParameterGeneration.make minReplica maxReplica spans refiner

    let makeSetupParameters minReplica maxReplica spans (refiner: setupParameters seq -> setupParameters seq) =
        ParameterGeneration.make minReplica maxReplica spans refiner
