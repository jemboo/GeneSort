namespace GeneSort.Dispatch.V1.SorterSgd

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters

/// Named inputs for constructing a continuation source.
type evoContinuationSourceArgs = {
    Run: run
    GenerationInterval: int
    ParameterSpans: (string * string list) list
}

type evoContinuationSource =
    private {
        _run: run
        _generationInterval: int
        _parameterSpans: (string * string list) list
    }

    member this.Run = this._run
    member this.GenerationInterval = this._generationInterval
    member this.ParameterSpans = this._parameterSpans

    static member public create (args: evoContinuationSourceArgs) : evoContinuationSource =
        { _run = args.Run; _generationInterval = args.GenerationInterval; _parameterSpans = args.ParameterSpans }

/// Named inputs for constructing a continuation configuration.
type evoContinuationConfigArgs = {
    ProjectName: string<projectName>
    DatabaseName: string<databaseName>
    RunName: string<runName>
    SetupQueryCatalogName: string
    RunQueryCatalogName: string
    Source: evoContinuationSource
    ExecutionParameterSpans: (string * string list) list
}

type evoContinuationConfig =
    private {
        _projectName: string<projectName>
        _databaseName: string<databaseName>
        _runName: string<runName>
        _setupQueryCatalogName: string
        _runQueryCatalogName: string
        _source: evoContinuationSource
        _executionParameterSpans: (string * string list) list
    }

    member this.ProjectName = this._projectName
    member this.DatabaseName = this._databaseName
    member this.RunName = this._runName
    member this.SetupQueryCatalogName = this._setupQueryCatalogName
    member this.RunQueryCatalogName = this._runQueryCatalogName
    member this.Source = this._source
    member this.ExecutionParameterSpans = this._executionParameterSpans

    static member public create (args: evoContinuationConfigArgs) : evoContinuationConfig =
        { _projectName = args.ProjectName
          _databaseName = args.DatabaseName
          _runName = args.RunName
          _setupQueryCatalogName = args.SetupQueryCatalogName
          _runQueryCatalogName = args.RunQueryCatalogName
          _source = args.Source
          _executionParameterSpans = args.ExecutionParameterSpans }

module internal EvoContinuation =
    /// Select source executions independently, then branch each into the execution spans.
    /// Unspecified execution settings are inherited from each selected source.
    let create expectedExecutionFunction (config: evoContinuationConfig) : run =
        let source = config.Source.Run
        if source.ExecuteFunctionName <> expectedExecutionFunction then
            invalidArg "Source" (sprintf "This workflow requires a %s source." expectedExecutionFunction)
        if config.Source.GenerationInterval < 0 then
            invalidArg "Source" "Source interval must be nonnegative."
        for spans in [config.Source.ParameterSpans; config.ExecutionParameterSpans] do
            if spans |> List.exists (snd >> List.isEmpty) then
                invalidArg "ParameterSpans" "Parameter spans must have at least one value."
            if spans |> List.map fst |> List.distinct |> List.length <> List.length spans then
                invalidArg "ParameterSpans" "Parameter keys must be unique within each span list."
        let reserved =
            [ParamKeys.idKey; ParamKeys.replKey; ParamKeys.sourceExecutionIdKey
             ParamKeys.sourceProjectNameKey; ParamKeys.sourceDbNameKey; ParamKeys.sourceRunNameKey
             ParamKeys.sourceGenerationIntervalKey
             ParamKeys.sourcePoolFileIdKey; ParamKeys.sourceTestsFileIdKey]
            @ SetupFileReferences.keys
        if config.ExecutionParameterSpans |> List.exists (fun (key, _) -> List.contains key reserved) then
            invalidArg "ExecutionParameterSpans" "Source identity and replica settings cannot be overridden."
        match config.ExecutionParameterSpans |> List.tryFind (fst >> (=) ParamKeys.generationIntervalLastKey) with
        | Some (_, values) when values |> List.forall (fun value ->
            match System.Int32.TryParse value with
            | true, interval -> interval > config.Source.GenerationInterval
            | _ -> false) -> ()
        | _ ->
            let finalIntervals =
                config.ExecutionParameterSpans
                |> List.tryFind (fst >> (=) ParamKeys.generationIntervalLastKey)
                |> Option.map (snd >> String.concat ", ")
                |> Option.defaultValue "<missing>"
            invalidArg "ExecutionParameterSpans"
                (sprintf "Append run '%s': %s [%s] must contain only intervals greater than Source.GenerationInterval (%d), from source run '%s'."
                    %config.RunName ParamKeys.generationIntervalLastKey finalIntervals config.Source.GenerationInterval %source.RunName)
        SgdRun.create {
            ProjectName = config.ProjectName
            DatabaseName = config.DatabaseName
            RunName = config.RunName
            Description = sprintf "Continue %s/%s/%s from interval %d" %source.ProjectName %source.DatabaseName %source.RunName config.Source.GenerationInterval
            AllowOverwrite = false
            Setup = {
                FunctionName = SetupCatalogNames.evoAppend
                ParameterSpans = []
                QueryCatalogName = config.SetupQueryCatalogName
                EnhancerFunctionName = SetupEnhancerFunctionNames.noOp
            }
            Execution = {
                FunctionName = source.ExecuteFunctionName
                ParameterSpans = config.ExecutionParameterSpans @ [
                    ParamKeys.sourceProjectNameKey, [%source.ProjectName]
                    ParamKeys.sourceDbNameKey, [%source.DatabaseName]
                    ParamKeys.sourceRunNameKey, [%source.RunName]
                    ParamKeys.sourceGenerationIntervalKey, [string config.Source.GenerationInterval]
                ]
                QueryCatalogName = config.RunQueryCatalogName
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
            }
        } {
            MakerNames = source.MakerNames.Value
            GenSaveIntervalsName = source.GenSaveIntervalsName.Value
            GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
        }
        |> run.SgdRun
        |> Run.withParameterSource source config.Source.ParameterSpans
