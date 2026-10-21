namespace GeneSort.Dispatch.V1.SorterSgd

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution

type evoAppendSource = {
    Run: run
    GenerationInterval: int
    ParameterSpans: (string * string list) list
}

type evoAppendConfig = {
    ProjectName: string<projectName>
    DatabaseName: string<databaseName>
    RunName: string<runName>
    Source: evoAppendSource
    ExecutionParameterSpans: (string * string list) list
}

module EvoAppend =
    /// Select source executions independently, then branch each into the execution spans.
    /// Unspecified execution settings are inherited from each selected source.
    let create (config: evoAppendConfig) : run =
        let source = config.Source.Run
        let queryCatalog =
            match source.ExecuteFunctionName, source.RunQueryCatalogName with
            | ExecuteFunctionNames.soss, (QueryCatalogNamesRun.sorterSgdMsufSoss | QueryCatalogNamesRun.sorterSgdMsufSossAppend) ->
                QueryCatalogNamesRun.sorterSgdMsufSossAppend
            | ExecuteFunctionNames.standard, (QueryCatalogNamesRun.sorterSgdMsufStandard | QueryCatalogNamesRun.sorterSgdMsufStandardAppend) ->
                QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            | _ -> invalidArg "Source" "Expected an Msuf Standard or Soss source run."
        if config.Source.GenerationInterval < 0 then
            invalidArg "Source" "Source interval must be nonnegative."
        if config.ExecutionParameterSpans |> List.exists (fst >> (=) ParamKeys.selectedSorterPoolCountKey) then
            invalidArg "ExecutionParameterSpans" "SelectedSorterPoolCount belongs in Source.ParameterSpans."
        match config.Source.ParameterSpans |> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey) with
        | None -> ()
        | Some (_, [value]) when source.ExecuteFunctionName = ExecuteFunctionNames.standard ->
            match System.Int32.TryParse value with
            | true, count when count > 0 ->
                match config.Source.ParameterSpans |> List.tryFind (fst >> (=) ParamKeys.mutationModKey) with
                | Some (_, values) when not values.IsEmpty && List.distinct values = values && count <= values.Length -> ()
                | _ -> invalidArg "Source.ParameterSpans" "Condense requires distinct MutationMod values and SelectedSorterPoolCount <= their count."
            | _ -> invalidArg "SelectedSorterPoolCount" "SelectedSorterPoolCount must be a positive integer."
        | Some _ -> invalidArg "SelectedSorterPoolCount" "Condense requires a Standard source and a single selection count."
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
                QueryCatalogName = source.SetupQueryCatalogName
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
                QueryCatalogName = queryCatalog
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
            }
        } {
            MakerNames = source.MakerNames.Value
            GenSaveIntervalsName = source.GenSaveIntervalsName.Value
            GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
        }
        |> run.SgdRun
        |> Run.withParameterSource source config.Source.ParameterSpans
