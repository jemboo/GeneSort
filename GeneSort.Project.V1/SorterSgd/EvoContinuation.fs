namespace GeneSort.Project.V1.SorterSgd

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters

/// Named inputs for constructing a continuation source.
type evoContinuationSourceArgs = {
    SourceRun: run
    GenerationInterval: int
    SourceMutationMods: int list
    SelectedSorterPoolCount: int
}

type evoContinuationSource =
    private {
        _run: sgdRun
        _generationInterval: int
        _sourceMutationMods: int list
        _selectedSorterPoolCount: int
    }

    member this.SourceRun = Run.ofSgd this._run
    member internal this.SourceSgdRun = this._run
    member this.GenerationInterval = this._generationInterval
    member this.SourceMutationMods = this._sourceMutationMods
    member this.SelectedSorterPoolCount = this._selectedSorterPoolCount

    static member public create (args: evoContinuationSourceArgs) : evoContinuationSource =
        let source =
            match args.SourceRun with
            | run.EvolutionRun source | run.ContinuationRun source when List.contains (SgdRun.executeFunctionName source) [ExecuteFunctionNames.standard; ExecuteFunctionNames.soss] -> source
            | _ -> invalidArg "SourceRun" "Continuation requires a Standard or Soss SGD source run."
        if args.GenerationInterval < 0 then
            invalidArg "GenerationInterval" "Source generation interval must be nonnegative."
        if args.SourceMutationMods.IsEmpty || List.distinct args.SourceMutationMods <> args.SourceMutationMods then
            invalidArg "SourceMutationMods" "SourceMutationMods must contain distinct mutation values."
        if args.SelectedSorterPoolCount <= 0 then
            invalidArg "SelectedSorterPoolCount" "SelectedSorterPoolCount must be positive."
        { _run = source; _generationInterval = args.GenerationInterval
          _sourceMutationMods = args.SourceMutationMods; _selectedSorterPoolCount = args.SelectedSorterPoolCount }


/// Named inputs for constructing a continuation configuration.
type evoContinuationConfigArgs = {
    ProjectName: string<projectName>
    DatabaseName: string<databaseName>
    RunName: string<runName>
    RunQueryCatalogName: string
    Source: evoContinuationSource
    ExecutionParameterSpans: (string * string list) list
}

type evoContinuationConfig =
    private {
        _projectName: string<projectName>
        _databaseName: string<databaseName>
        _runName: string<runName>
        _runQueryCatalogName: string
        _source: evoContinuationSource
        _executionParameterSpans: (string * string list) list
    }

    member this.ProjectName = this._projectName
    member this.DatabaseName = this._databaseName
    member this.RunName = this._runName
    member this.RunQueryCatalogName = this._runQueryCatalogName
    member this.Source = this._source
    member this.ExecutionParameterSpans = this._executionParameterSpans

    static member public create (args: evoContinuationConfigArgs) : evoContinuationConfig =
        { _projectName = args.ProjectName
          _databaseName = args.DatabaseName
          _runName = args.RunName
          _runQueryCatalogName = args.RunQueryCatalogName
          _source = args.Source
          _executionParameterSpans = args.ExecutionParameterSpans }

module internal EvoContinuation =
    /// Select source executions independently, then branch each into the execution spans.
    /// Unspecified execution settings are inherited from each selected source.
    let create expectedExecutionFunction setupFunctionName (config: evoContinuationConfig) : run =
        let source = config.Source.SourceRun
        let sourceSgd = config.Source.SourceSgdRun
        if source.ExecuteFunctionName <> expectedExecutionFunction && expectedExecutionFunction = ExecuteFunctionNames.soss then
            for key in [ParamKeys.seedSossKey; ParamKeys.partitionCountKey; ParamKeys.generationsPerSubsetKey; ParamKeys.generationsPerMergedSetKey] do
                match config.ExecutionParameterSpans |> List.tryFind (fst >> (=) key) with
                | Some (_, values) when not values.IsEmpty && values |> List.forall (fun value ->
                    if key = ParamKeys.seedSossKey then System.UInt64.TryParse value |> fst
                    else match System.Int32.TryParse value with true, n -> n > 0 | _ -> false) -> ()
                | _ -> invalidArg "ExecutionParameterSpans" (sprintf "Standard to Soss requires explicit valid %s values (counts and durations must be positive)." key)
        let executionSpans = config.ExecutionParameterSpans
        if executionSpans |> List.exists (snd >> List.isEmpty) then
            invalidArg "ExecutionParameterSpans" "Parameter spans must have at least one value."
        if executionSpans |> List.map fst |> List.distinct |> List.length <> List.length executionSpans then
            invalidArg "ExecutionParameterSpans" "Parameter keys must be unique within each span list."
        let sourceOptions = RunParameterGeneration.effectiveSpans source
        for key, values in config.ExecutionParameterSpans do
            if key <> ParamKeys.mutationModKey then
                if values.Length <> 1 then
                    invalidArg "ExecutionParameterSpans" (sprintf "%s override must contain exactly one value; only MutationMod can expand executions." key)
                match Map.tryFind key sourceOptions with
                | Some sourceValues when sourceValues.Length <> 1 ->
                    invalidArg "ExecutionParameterSpans" (sprintf "%s has multiple source values and must be inherited, not overridden." key)
                | _ -> ()
        let reserved =
            [ParamKeys.idKey; ParamKeys.replKey; ParamKeys.selectedSorterPoolCountKey; ParamKeys.sourceExecutionIdKey; ParamKeys.sourceMutationModKey
             ParamKeys.sourceProjectNameKey; ParamKeys.sourceDbNameKey; ParamKeys.sourceRunNameKey
             ParamKeys.sourceGenerationIntervalKey]
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
                FunctionName = setupFunctionName
                SortableTestsModule = source.SgdSetup.Value.SortableTestsModule
                SorterEvalModule = source.SgdSetup.Value.SorterEvalModule
                ParameterSpans = []
                QueryCatalogName = QueryCatalogNamesSetup.importedStart
                EnhancerFunctionName = SetupEnhancerFunctionNames.noOp
            }
            Execution = {
                FunctionName = expectedExecutionFunction
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
            MakerNames = SgdRun.makerNames sourceSgd
            GenSaveIntervalsName = SgdRun.genSaveIntervalsName sourceSgd
            GenSaveSubIntervalsName = SgdRun.genSaveSubIntervalsName sourceSgd
        }
        |> SgdRun.withCheckpointIntervals (SgdRun.checkpointIntervalsName sourceSgd)
        |> Run.ofSgd
        |> Run.withParameterSource source [
            ParamKeys.sourceMutationModKey, config.Source.SourceMutationMods |> List.map string
            ParamKeys.selectedSorterPoolCountKey, [string config.Source.SelectedSorterPoolCount]
        ]
