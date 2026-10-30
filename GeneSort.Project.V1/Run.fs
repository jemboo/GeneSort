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
                |> Seq.map ParameterValidation.resolveSelection
                |> Seq.map ParameterValidation.resolveEvaluationMeasures
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
    ParametersEnhancerName: string
}

/// Setup settings for evaluation and mutation runs.
type preparedSetupConfig = {
    FunctionName: string
    ParameterSpans: (string * string list) list
    QueryCatalogName: string
    ParametersEnhancerName: string
}

/// Complete configuration validated by SimpleRun.create.
type simpleRunConfig = {
    DatabaseName: string<databaseName>
    ProjectName: string<projectName>
    RunName: string<runName>
    Description: string
    AllowOverwrite: bool
    Execution: executionConfig
    Setup: preparedSetupConfig
}

/// A publication run has one parameter sweep and no preparation phase.
type directRunConfig = {
    DatabaseName: string<databaseName>
    ProjectName: string<projectName>
    RunName: string<runName>
    Description: string
    AllowOverwrite: bool
    Execution: executionConfig
}

/// Evolution setup and its source libraries, independent of the output database.
type sgdSetupConfig = {
    FunctionName: string
    SortableTestsModule: string<runModuleName>
    SorterEvalModule: string<runModuleName>
    ParameterSpans: (string * string list) list
    QueryCatalogName: string
    ParametersEnhancerName: string
}

/// The run database stores prepared inputs and execution results.
type sgdBaseConfig = {
    DatabaseName: string<databaseName>
    ProjectName: string<projectName>
    RunName: string<runName>
    Description: string
    AllowOverwrite: bool
    Execution: executionConfig
    Setup: sgdSetupConfig
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
          setupFunctionName: string option
          sortableTestsModule: string<runModuleName> option
          sorterEvalModule: string<runModuleName> option
          runParametersEnhancerName: string
          setupParametersEnhancerName: string
          allowOverwrite: bool
          executeFunctionName: string
          parameterSource: (simpleRun * (string * string list) list) option
        }

module SimpleRun =
    let makeRunParameters minReplica maxReplica refiner (run: simpleRun) : runParameters[] =
        ParameterGeneration.make minReplica maxReplica run.runParameterSpans refiner

    let makeSetupParameters minReplica maxReplica refiner (run: simpleRun) : setupParameters[] =
        let withSourceModules (parameters: setupParameters) =
            match run.sortableTestsModule, run.sorterEvalModule with
            | Some tests, Some evals ->
                parameters.ParamMap
                |> Map.add ParamKeys.sortableTestsModuleKey (string tests)
                |> Map.add ParamKeys.sorterEvalModuleKey (string evals)
                |> setupParameters.create
            | _ -> parameters
        ParameterGeneration.make minReplica maxReplica run.setupParameterSpans (Seq.map withSourceModules >> refiner)

    let create (config: simpleRunConfig) : simpleRun =
        let executeFunctionName = config.Execution.FunctionName
        let databaseName = config.DatabaseName
        let projectName = config.ProjectName
        let runName = config.RunName
        let description = config.Description
        let runParameterSpans = config.Execution.ParameterSpans
        let runQueryCatalogName = config.Execution.QueryCatalogName
        let setupFunctionName = config.Setup.FunctionName
        let runParametersEnhancerName = config.Execution.ParametersEnhancerName
        let allowOverwrite = config.AllowOverwrite
        let setupParameterSpans = config.Setup.ParameterSpans
        let setupQueryCatalogName = config.Setup.QueryCatalogName
        let setupParametersEnhancerName = config.Setup.ParametersEnhancerName
        if String.IsNullOrWhiteSpace executeFunctionName then
            invalidArg (nameof executeFunctionName) "Execute function name cannot be empty."
        if String.IsNullOrWhiteSpace runQueryCatalogName then
            invalidArg (nameof runQueryCatalogName) "Run query catalog name cannot be empty."
        if String.IsNullOrWhiteSpace setupFunctionName then
            invalidArg (nameof setupFunctionName) "Setup function name cannot be empty."
        for argument, value in [nameof setupQueryCatalogName, setupQueryCatalogName; nameof runParametersEnhancerName, runParametersEnhancerName; nameof setupParametersEnhancerName, setupParametersEnhancerName] do
            if String.IsNullOrWhiteSpace value then
                invalidArg argument "Function names cannot be empty."
        if String.IsNullOrWhiteSpace %databaseName then
            failwith "Database name cannot be empty"
        {
          dataBaseName = databaseName
          projName = projectName
          runName = runName
          description = description
          runParameterSpans = RunParameterRoles.withoutRepeatedSetup executeFunctionName false setupParameterSpans runParameterSpans
          runQueryCatalogName = runQueryCatalogName
          setupParameterSpans = setupParameterSpans
          setupQueryCatalogName = setupQueryCatalogName
          setupFunctionName = Some setupFunctionName
          sortableTestsModule = None
          sorterEvalModule = None
          runParametersEnhancerName = runParametersEnhancerName
          setupParametersEnhancerName = setupParametersEnhancerName
          allowOverwrite = allowOverwrite
          executeFunctionName = executeFunctionName
          parameterSource = None
        }

    /// The shared parameter-generation data for an SGD run or continuation source.
    let createEvolutionSource (config: sgdBaseConfig) : simpleRun =
        for argument, value in ["ExecuteFunctionName", config.Execution.FunctionName
                                "SetupFunctionName", config.Setup.FunctionName
                                "RunQueryCatalogName", config.Execution.QueryCatalogName
                                "RunParametersEnhancerName", config.Execution.ParametersEnhancerName
                                "SetupQueryCatalogName", config.Setup.QueryCatalogName
                                "SetupParametersEnhancerName", config.Setup.ParametersEnhancerName] do
            if String.IsNullOrWhiteSpace value then invalidArg argument "Function names cannot be empty."
        if String.IsNullOrWhiteSpace %config.DatabaseName then
            invalidArg "DatabaseName" "Database name cannot be empty."
        for field, name in ["SortableTestsModule", config.Setup.SortableTestsModule
                            "SorterEvalModule", config.Setup.SorterEvalModule] do
            let key = if field = nameof config.Setup.SortableTestsModule then ParamKeys.sortableTestsModuleKey else ParamKeys.sorterEvalModuleKey
            match ParameterValidation.validateValue key (string name) with
            | Ok _ -> ()
            | Error error -> invalidArg field error
        {
            dataBaseName = config.DatabaseName
            projName = config.ProjectName
            runName = config.RunName
            description = config.Description
            runParameterSpans =
                RunParameterRoles.withoutRepeatedSetup config.Execution.FunctionName
                    (config.Execution.ParameterSpans |> List.exists (fst >> (=) ParamKeys.sourceRunNameKey))
                    config.Setup.ParameterSpans config.Execution.ParameterSpans
            runQueryCatalogName = config.Execution.QueryCatalogName
            setupParameterSpans = config.Setup.ParameterSpans
            setupQueryCatalogName = config.Setup.QueryCatalogName
            setupFunctionName = Some config.Setup.FunctionName
            sortableTestsModule = Some config.Setup.SortableTestsModule
            sorterEvalModule = Some config.Setup.SorterEvalModule
            runParametersEnhancerName = config.Execution.ParametersEnhancerName
            setupParametersEnhancerName = config.Setup.ParametersEnhancerName
            allowOverwrite = config.AllowOverwrite
            executeFunctionName = config.Execution.FunctionName
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
    let sgdSetup (run: simpleRun) : sgdSetupConfig option =
        run.sortableTestsModule |> Option.map (fun tests -> {
            FunctionName = run.setupFunctionName.Value
            SortableTestsModule = tests
            SorterEvalModule = run.sorterEvalModule.Value
            ParameterSpans = run.setupParameterSpans
            QueryCatalogName = run.setupQueryCatalogName
            ParametersEnhancerName = run.setupParametersEnhancerName
        })
    let runParametersEnhancerName (run: simpleRun) = run.runParametersEnhancerName
    let setupParametersEnhancerName (run: simpleRun) = run.setupParametersEnhancerName
    let allowOverwrite (run: simpleRun) = run.allowOverwrite
    let executeFunctionName (run: simpleRun) = run.executeFunctionName
    let parameterSource (run: simpleRun) = run.parameterSource
    let withParameterSource source spans (run: simpleRun) =
        { run with parameterSource = Some (source, spans) }

    /// Import the old two-phase SortableTests definition into a single sweep.
    let asDirect (value: simpleRun) =
        let spans =
            value.setupParameterSpans @ value.runParameterSpans
            |> List.fold (fun acc (key, values) ->
                (acc |> List.filter (fst >> (<>) key)) @ [key, values]) []
        { value with setupParameterSpans = []; runParameterSpans = spans }

    let internal withSpans setup execution value =
        { value with setupParameterSpans = setup; runParameterSpans = execution }

module DirectRun =
    let create (config: directRunConfig) =
        // Retain the historical DTO's catalog fields. They are not workflow choices
        // and are never invoked by direct parameter generation or execution.
        let prefix = config.Execution.QueryCatalogName = Catalogs.Queries.QueryCatalogNamesRun.sortableTestPrefix
        SimpleRun.create {
            DatabaseName = config.DatabaseName; ProjectName = config.ProjectName; RunName = config.RunName
            Description = config.Description; AllowOverwrite = config.AllowOverwrite; Execution = config.Execution
            Setup = {
                FunctionName = if prefix then Catalogs.Setup.SetupCatalogNames.prefix else Catalogs.Setup.SetupCatalogNames.merge
                QueryCatalogName = if prefix then Catalogs.Queries.QueryCatalogNamesSetup.sortableTestPrefix else Catalogs.Queries.QueryCatalogNamesSetup.sortableTestMerge
                ParametersEnhancerName = "setup-parameters-enhancer.sortable-tests"
                ParameterSpans = []
            }
        }

type sgdRun =
    private
        {
          baseRun: simpleRun
          checkpointIntervalsName: string option
          genSaveIntervalsName: string
          genSaveSubIntervalsName: string
          makerNames: evolutionMakerNames
        }

module SgdRun =
    let create (config: sgdBaseConfig) (sgd: sgdRunConfig) : sgdRun =
        EvolutionMakerNames.validate sgd.MakerNames
        if String.IsNullOrWhiteSpace sgd.GenSaveIntervalsName then
            invalidArg "GenSaveIntervalsName" "Generation save interval name cannot be empty"
        if String.IsNullOrWhiteSpace sgd.GenSaveSubIntervalsName then
            invalidArg "GenSaveSubIntervalsName" "Generation summary interval name cannot be empty"
        {
          baseRun = SimpleRun.createEvolutionSource config
          checkpointIntervalsName = None
          genSaveIntervalsName = sgd.GenSaveIntervalsName
          genSaveSubIntervalsName = sgd.GenSaveSubIntervalsName
          makerNames = sgd.MakerNames
        }

    let withCheckpointIntervals name (run: sgdRun) =
        { run with checkpointIntervalsName = name |> Option.filter (fun value -> not (String.IsNullOrWhiteSpace value) && value <> run.genSaveIntervalsName) }
    let checkpointIntervalsName (run: sgdRun) = run.checkpointIntervalsName
    let baseRun (run: sgdRun) = run.baseRun
    let withParameterSource source spans (run: sgdRun) =
        { run with baseRun = SimpleRun.withParameterSource source spans run.baseRun }
    let makerNames (run: sgdRun) = run.makerNames
    let executeFunctionName (run: sgdRun) = SimpleRun.executeFunctionName run.baseRun
    let genSaveIntervalsName (run: sgdRun) = run.genSaveIntervalsName
    let genSaveSubIntervalsName (run: sgdRun) = run.genSaveSubIntervalsName
    let internal withSpans setup execution value =
        { value with baseRun = SimpleRun.withSpans setup execution value.baseRun }

type run =
    | DirectRun of simpleRun
    | PreparedRun of simpleRun
    | EvolutionRun of sgdRun
    | ContinuationRun of sgdRun
    member this.HasSetup = match this with DirectRun _ -> false | _ -> true
    member this.IsEvolution = match this with EvolutionRun _ | ContinuationRun _ -> true | _ -> false
    member this.BaseRun =
        match this with
        | DirectRun value | PreparedRun value -> value
        | EvolutionRun value | ContinuationRun value -> SgdRun.baseRun value
    /// Only the source's parameter-generation configuration is needed here.
    member this.ParameterSource =
        SimpleRun.parameterSource this.BaseRun
        |> Option.map (fun (source, spans) -> PreparedRun source, spans)
    member this.DatabaseName = SimpleRun.databaseName this.BaseRun
    member this.ProjectName = SimpleRun.projectName this.BaseRun
    member this.RunName = SimpleRun.runName this.BaseRun
    member this.Description = SimpleRun.description this.BaseRun
    member this.RunParameterSpans = SimpleRun.runParameterSpans this.BaseRun
    member this.RunQueryCatalogName = SimpleRun.runQueryCatalogName this.BaseRun
    member this.SetupParameterSpans = SimpleRun.setupParameterSpans this.BaseRun
    member this.SetupQueryCatalogName = SimpleRun.setupQueryCatalogName this.BaseRun
    member this.SetupFunctionName with get() = SimpleRun.setupFunctionName this.BaseRun
    member this.SgdSetup = SimpleRun.sgdSetup this.BaseRun
    member this.RunParametersEnhancerName = SimpleRun.runParametersEnhancerName this.BaseRun
    member this.SetupParametersEnhancerName = SimpleRun.setupParametersEnhancerName this.BaseRun
    member this.AllowOverwrite = SimpleRun.allowOverwrite this.BaseRun
    member this.CheckpointIntervalsName =
        match this with
        | EvolutionRun value | ContinuationRun value -> SgdRun.checkpointIntervalsName value
        | _ -> None
    member this.GenSaveIntervalsName =
        match this with
        | DirectRun _ | PreparedRun _ -> None
        | EvolutionRun value | ContinuationRun value -> Some (SgdRun.genSaveIntervalsName value)
    member this.MakerNames =
        match this with
        | DirectRun _ | PreparedRun _ -> None
        | EvolutionRun value | ContinuationRun value -> Some (SgdRun.makerNames value)
    member this.ExecuteFunctionName = SimpleRun.executeFunctionName this.BaseRun
    member this.GenSaveSubIntervalsName =
        match this with
        | DirectRun _ | PreparedRun _ -> None
        | EvolutionRun value | ContinuationRun value -> Some (SgdRun.genSaveSubIntervalsName value)




module Run =
    let ofSimple value =
        if SimpleRun.executeFunctionName value = ExecuteFunctionNames.sortableTests then DirectRun (SimpleRun.asDirect value)
        else PreparedRun value

    let ofSgd value =
        let source = SgdRun.baseRun value
        if (SimpleRun.parameterSource source).IsSome
           || SimpleRun.runParameterSpans source |> List.exists (fst >> (=) ParamKeys.sourceRunNameKey) then ContinuationRun value
        else EvolutionRun value

    let requireSetupFunctionName (configured: run) =
        configured.SetupFunctionName
        |> Option.defaultWith (fun () -> invalidArg "configured" "This run does not specify a setup function name.")

    let withParameterSource (source: run) spans = function
        | PreparedRun value -> PreparedRun (SimpleRun.withParameterSource source.BaseRun spans value)
        | EvolutionRun value | ContinuationRun value -> ContinuationRun (SgdRun.withParameterSource source.BaseRun spans value)
        | DirectRun _ -> invalidArg "source" "A direct run cannot import an evolutionary checkpoint."

    let parameterRole (configured: run) key =
        RunParameterRoles.forWorkflow configured.ExecuteFunctionName
            (configured.IsContinuationRun || configured.ParameterSource.IsSome) key

    let withParameterSpans setup execution = function
        | DirectRun value -> DirectRun (SimpleRun.withSpans [] execution value)
        | PreparedRun value -> PreparedRun (SimpleRun.withSpans setup execution value)
        | EvolutionRun value -> EvolutionRun (SgdRun.withSpans setup execution value)
        | ContinuationRun value -> ContinuationRun (SgdRun.withSpans setup execution value)

    let makeRunParameters minReplica maxReplica spans (refiner: runParameters seq -> runParameters seq) =
        ParameterGeneration.make minReplica maxReplica spans refiner

    let makeSetupParameters minReplica maxReplica spans (refiner: setupParameters seq -> setupParameters seq) =
        ParameterGeneration.make minReplica maxReplica spans refiner
