namespace GeneSort.Project.V1.Editing

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.Project.V1.Workflows
open GeneSort.SortingLib.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1.ParamBundles

/// Edit a module's example without copying or reselecting its workflow configuration.
/// The runType catalog belongs to Dispatch; this editor only needs the selected run.
module RunTypeEditor =
    let isEvolution (configured: run) = configured.IsEvolution
    /// Evolution families own these settings. Existing comparison workflows keep their sweeps.
    let isModuleOwnedParameter (configured: run) key =
        let spans = RunParameterGeneration.effectiveSpans configured
        key = ParamKeys.channelCountKey
        || List.contains key [ParamKeys.excludeSelfCeKey; ParamKeys.collectSortableTestsKey]
        || (List.contains key [ParamKeys.rngTypeKey; ParamKeys.sortableDataFormatKey]
            && (Map.tryFind key spans |> Option.exists (fun values -> values.Length = 1)))
        || (configured.IsEvolution
            && List.contains key [ParamKeys.rngTypeKey; ParamKeys.simpleSorterModelTypeKey;
                                  ParamKeys.sortableDataFormatKey; ParamKeys.prefixLibIdKey;
                                  ParamKeys.sorterEvalMeasureInitialKey; ParamKeys.sorterEvalMeasureKey]
            && not (key = ParamKeys.sorterEvalMeasureKey
                    && Map.tryFind ParamKeys.sorterEvalMeasureInitialKey spans = Some ["SameAsEvaluation"]))
    let private spans rows = rows |> Array.map (fun (row: parameterDraft) -> row.Key, Array.toList row.Values) |> Array.toList
    let private rows spans = spans |> List.map (fun (key, values) -> parameterDraft.create key (List.toArray values)) |> List.toArray

    let fromRun (configured: run) =
        runDraft.create {
            ProjectName = %configured.ProjectName; DatabaseName = %configured.DatabaseName; RunName = %configured.RunName
            SortableTestsModule = configured.SgdSetup |> Option.map (fun setup -> string setup.SortableTestsModule) |> Option.defaultValue ""
            SorterEvalModule = configured.SgdSetup |> Option.map (fun setup -> string setup.SorterEvalModule) |> Option.defaultValue ""
            Description = configured.Description; AllowOverwrite = configured.AllowOverwrite
            CheckpointIntervalsName = configured.CheckpointIntervalsName |> Option.orElse configured.GenSaveIntervalsName |> Option.defaultValue ""
            GenSaveIntervalsName = configured.GenSaveIntervalsName |> Option.defaultValue ""
            GenSaveSubIntervalsName = configured.GenSaveSubIntervalsName |> Option.defaultValue ""
            SetupParameters = rows configured.SetupParameterSpans
            ExecutionParameters = rows configured.RunParameterSpans
        }

    let newDraft configured =
        let args = (fromRun configured).ToArgs()
        runDraft.create { args with RunName = "NewRun" }

    let canEditSetup (configured: run) key =
        configured.HasSetup && not configured.IsContinuationRun
        && (Run.parameterRole configured key <> parameterRole.ExecutionOnly
            || configured.SetupParameterSpans |> List.exists (fst >> (=) key))

    let private valueChannelCount key value =
        try
            if key = ParamKeys.channelCountKey then Some (Int32.Parse value)
            elif key = ParamKeys.prefixLibIdKey then Some (int (PrefixLibId.fromString value).ChannelCount)
            elif key = ParamKeys.mergeLibIdKey then Some (int (MergeLibId.fromString value).ChannelCount)
            elif key = ParamKeys.sorterLibIdKey then Some (int (SorterLibId.fromString value).ChannelCount)
            elif key = ParamKeys.mutatorParamsKey then
                match MutatorParams.fromString value with
                | Ok (mutatorParams.SimpleMutatorParams simple) -> Some (int simple.ChannelCount)
                | _ -> None
            else None
        with _ -> None

    /// Resolve module counts from saved spans, including continuation sources.
    let rec channelCounts (configured: run) =
        let declared =
            configured.SetupParameterSpans @ configured.RunParameterSpans
            |> List.collect (fun (key, values) -> values |> List.choose (valueChannelCount key))
            |> List.distinct |> List.sort |> List.toArray
        if declared.Length > 0 then declared
        else
            match configured.ParameterSource with
            | Some (source, _) -> channelCounts source
            | None -> [||]

    let validateChannelCount channelCount (template: run) (draft: runDraft) =
        let issues = ResizeArray<validationIssue>()
        if channelCount <= 0 then
            issues.Add(validationIssue.create "ChannelCount" "Choose a module with one fixed channel count.")
        for phase, rows in ["Setup", draft.SetupParameters; "Execution", draft.ExecutionParameters] do
            for row in rows do
                for value in row.Values do
                    if row.Key = ParamKeys.simpleSorterModelTypeKey then
                        try
                            if not (SimpleSorterModelType.supportsChannelCount channelCount (SimpleSorterModelType.fromString value)) then
                                issues.Add(validationIssue.create (phase + "." + row.Key) (sprintf "%s does not support %d channels." value channelCount))
                        with _ -> issues.Add(validationIssue.create (phase + "." + row.Key) "Choose a supported sorter model type.")
                    if row.Key = ParamKeys.mutatorParamsKey then
                        match MutatorParams.fromString value with
                        | Error error -> issues.Add(validationIssue.create (phase + "." + row.Key) error)
                        | _ -> ()
                    match valueChannelCount row.Key value with
                    | Some actual when actual <> channelCount ->
                        issues.Add(validationIssue.create (phase + "." + row.Key) (sprintf "This module requires %d channels." channelCount))
                    | _ -> ()
                if row.Key = ParamKeys.channelCountKey && row.Values <> [|string channelCount|] then
                    issues.Add(validationIssue.create (phase + "." + row.Key) "Channel count is fixed by the module; it cannot be a parameter sweep.")
        for actual in channelCounts template do
            if actual <> channelCount then
                issues.Add(validationIssue.create "ChannelCount" "The source definition does not belong to this module's channel count.")
        issues.ToArray()

    let parameters (configured: run) =
        let allSpans = configured.SetupParameterSpans @ configured.RunParameterSpans
        allSpans
        |> List.map fst |> List.distinct
        |> List.map (fun key ->
            let required = configured.SetupParameterSpans |> List.exists (fst >> (=) key)
            let executionAllowed = not (RunParameterRoles.isFixed (Run.parameterRole configured key))
            if key = ParamKeys.mergeLibIdKey then
                let declared = allSpans |> List.filter (fst >> (=) key) |> List.collect snd
                let choices =
                    declared
                    |> List.choose (fun value -> try Some (MergeLibId.fromString value).ChannelCount with _ -> None)
                    |> List.distinct |> List.sort
                    |> List.collect (MergeLibId.availableForChannelCount >> Array.map MergeLibId.toString >> Array.toList)
                // Preserve existing declarations even when this library version lacks their network.
                let choices = (choices @ declared) |> List.distinct
                parameterDescription.forChoices key choices required executionAllowed
            elif key = ParamKeys.prefixLibIdKey then
                let declared = allSpans |> List.filter (fst >> (=) key) |> List.collect snd
                let widths = declared |> List.choose (fun value -> try Some (PrefixLibId.fromString value).ChannelCount with _ -> None)
                let choices =
                    PrefixLib.ids ()
                    |> Array.filter (fun id -> List.contains id.ChannelCount widths)
                    |> Array.map PrefixLibId.toString |> Array.toList
                parameterDescription.forChoices key ((choices @ declared) |> List.distinct) required executionAllowed
            elif key = ParamKeys.sorterLibIdKey then
                let declared = allSpans |> List.filter (fst >> (=) key) |> List.collect snd
                let widths = declared |> List.choose (fun value -> try Some (SorterLibId.fromString value).ChannelCount with _ -> None)
                let choices =
                    SorterLib.ids ()
                    |> Array.filter (fun id -> List.contains id.ChannelCount widths)
                    |> Array.map SorterLibId.toString |> Array.toList
                parameterDescription.forChoices key ((choices @ declared) |> List.distinct) required executionAllowed
            else parameterDescription.forKey key required executionAllowed)
        |> List.toArray

    /// The module count, rather than the edited values, determines compatible choices.
    let parametersForChannelCount (configured: run) (channelCount: int) =
        let count = UMX.tag<GeneSort.Sorting.channelCount> channelCount
        parameters configured |> Array.map (fun spec ->
            let available =
                if spec.Key = ParamKeys.prefixLibIdKey then
                    Some (PrefixLib.ids () |> Array.filter (fun id -> id.ChannelCount = count) |> Array.map PrefixLibId.toString)
                elif spec.Key = ParamKeys.mergeLibIdKey then
                    Some (MergeLibId.availableForChannelCount count |> Array.map MergeLibId.toString)
                elif spec.Key = ParamKeys.sorterLibIdKey then
                    Some (SorterLib.ids () |> Array.filter (fun id -> id.ChannelCount = count) |> Array.map SorterLibId.toString)
                elif spec.Key = ParamKeys.channelCountKey then Some [|string channelCount|]
                else None
            match available with
            | None when spec.Key = ParamKeys.simpleSorterModelTypeKey ->
                let choices = spec.Choices |> Array.filter (fun value ->
                    try SimpleSorterModelType.supportsChannelCount channelCount (SimpleSorterModelType.fromString value) with _ -> false)
                parameterDescription.forChoices spec.Key (Array.toList choices) spec.RequiredInSetup spec.ExecutionAllowed
            | None -> spec
            | Some values ->
                let declared = spec.Choices |> Array.filter (fun value -> valueChannelCount spec.Key value = Some channelCount)
                parameterDescription.forChoices spec.Key (Array.append values declared |> Array.distinct |> Array.toList) spec.RequiredInSetup spec.ExecutionAllowed)

    /// The width picker is editor state. PrefixLibId already records it in the saved run.
    let prefixChannelCounts (configured: run) =
        let declared =
            configured.SetupParameterSpans @ configured.RunParameterSpans
            |> List.filter (fst >> (=) ParamKeys.prefixLibIdKey) |> List.collect snd
            |> List.choose (fun value -> try Some (int (PrefixLibId.fromString value).ChannelCount) with _ -> None)
        Array.append (PrefixLib.ids () |> Array.map (fun id -> int id.ChannelCount)) (List.toArray declared)
        |> Array.distinct |> Array.sort

    let parametersForPrefixChannelCount (configured: run) (width: int) =
        parameters configured |> Array.map (fun spec ->
            if spec.Key <> ParamKeys.prefixLibIdKey then spec
            else
                let available =
                    PrefixLib.ids () |> Array.filter (fun id -> int id.ChannelCount = width)
                    |> Array.map PrefixLibId.toString |> Array.toList
                let declared =
                    configured.SetupParameterSpans @ configured.RunParameterSpans
                    |> List.filter (fst >> (=) ParamKeys.prefixLibIdKey) |> List.collect snd
                    |> List.filter (fun value -> try int (PrefixLibId.fromString value).ChannelCount = width with _ -> false)
                parameterDescription.forChoices spec.Key ((available @ declared) |> List.distinct) spec.RequiredInSetup spec.ExecutionAllowed)

    /// Merge widths can be larger than their component sorter widths. These are
    /// suggestions; the editor also accepts a typed width supported by availableForChannelCount.
    let mergeChannelCounts (configured: run) =
        let declared =
            configured.SetupParameterSpans @ configured.RunParameterSpans
            |> List.filter (fst >> (=) ParamKeys.mergeLibIdKey) |> List.collect snd
            |> List.choose (fun value -> try Some (int (MergeLibId.fromString value).ChannelCount) with _ -> None)
        Array.append (SorterLib.ids () |> Array.map (fun id -> int id.ChannelCount)) (List.toArray declared)
        |> Array.distinct |> Array.sort

    let parametersForMergeChannelCount (configured: run) (width: int) =
        parameters configured |> Array.map (fun spec ->
            if spec.Key <> ParamKeys.mergeLibIdKey then spec
            else
                let available = MergeLibId.availableForChannelCount (UMX.tag width) |> Array.map MergeLibId.toString |> Array.toList
                let declared =
                    configured.SetupParameterSpans @ configured.RunParameterSpans
                    |> List.filter (fst >> (=) ParamKeys.mergeLibIdKey) |> List.collect snd
                    |> List.filter (fun value -> try int (MergeLibId.fromString value).ChannelCount = width with _ -> false)
                parameterDescription.forChoices spec.Key ((available @ declared) |> List.distinct) spec.RequiredInSetup spec.ExecutionAllowed)

    /// Display only: these rows never become execution sweep dimensions. Resolve
    /// one representative combination to include enhancer-supplied defaults.
    let inheritedParameters (configured: run) (draft: runDraft) =
        if not configured.HasSetup then [||]
        else
            let setup = spans draft.SetupParameters
            let source = configured.ParameterSource |> Option.map fst
            let declared =
                (source |> Option.map RunParameterGeneration.effectiveSpans |> Option.defaultValue Map.empty)
                |> fun inherited -> setup |> List.fold (fun map (key, values) -> Map.add key values map) inherited
            let preview =
                let one spans = spans |> List.map (fun (key, values) -> key, values |> List.truncate 1)
                let rec initial (definition: run) =
                    definition.ParameterSource |> Option.map (fst >> initial) |> Option.defaultValue definition
                let definition = source |> Option.map initial |> Option.defaultValue configured
                let setup, execution =
                    if source.IsSome then definition.SetupParameterSpans, definition.RunParameterSpans
                    else setup, spans draft.ExecutionParameters
                Run.withParameterSpans (one setup) (one execution) definition
            let defaults =
                try
                    if preview.ParameterSource.IsSome then Map.empty
                    else RunParameterGeneration.generate preview 0<replNumber> 1<replNumber>
                         |> Array.tryHead |> Option.map (fun rp -> rp.ParamMap |> Map.map (fun _ value -> [value]))
                         |> Option.defaultValue Map.empty
                with _ -> Map.empty // Invalid edits retain declared values and normal validation errors.
            let values = Map.fold (fun map key values -> Map.add key values map) defaults declared
            let values =
                if configured.IsContinuationRun then
                    configured.RunParameterSpans |> List.fold (fun map (key, values) ->
                        if key.StartsWith("Source") then Map.add key values map else map) values
                else values
            values |> Map.toList
            |> List.filter (fun (key, _) -> Run.parameterRole configured key = parameterRole.InheritedReadOnly)
            |> rows

    let catalogChoices (configured: run) =
        [| yield catalogChoice.create "Execution.FunctionName" configured.ExecuteFunctionName "Execution function"
           yield catalogChoice.create "Execution.QueryCatalogName" configured.RunQueryCatalogName "Result file identity"
           yield catalogChoice.create "Execution.ParametersEnhancerName" configured.RunParametersEnhancerName "Execution parameters"
           if configured.HasSetup then
               match configured.SetupFunctionName with
               | Some name -> yield catalogChoice.create "Setup.FunctionName" name "Setup function"
               | None -> ()
               yield catalogChoice.create "Setup.QueryCatalogName" configured.SetupQueryCatalogName "Shared input file identity"
               yield catalogChoice.create "Setup.ParametersEnhancerName" configured.SetupParametersEnhancerName "Setup parameters" |]

    /// Module identity is not stored in the existing file format. Match the complete
    /// workflow, then let the UI prefer the original project/database when available.
    let matches (example: run) (configured: run) =
        isEvolution example = isEvolution configured
        && example.ExecuteFunctionName = configured.ExecuteFunctionName
        && example.RunQueryCatalogName = configured.RunQueryCatalogName
        && example.RunParametersEnhancerName = configured.RunParametersEnhancerName
        && example.SetupFunctionName = configured.SetupFunctionName
        && example.SetupQueryCatalogName = configured.SetupQueryCatalogName
        && example.SetupParametersEnhancerName = configured.SetupParametersEnhancerName
        && example.MakerNames = configured.MakerNames
        && example.ParameterSource.IsSome = configured.ParameterSource.IsSome

    let validate (template: run) (draft: runDraft) =
        let setup, execution = spans draft.SetupParameters, spans draft.ExecutionParameters
        let issues = ResizeArray<validationIssue>()
        let add field message = issues.Add(validationIssue.create field message)
        if not template.HasSetup && not setup.IsEmpty then add "Setup" "A direct run has no Setup phase."
        for key, _ in setup do
            if not (canEditSetup template key) then add ("Setup." + key) "This workflow supplies this parameter in Execution or imports it from its source."
        for key, _ in execution do
            if RunParameterRoles.isFixed (Run.parameterRole template key) && not (key.StartsWith("Source")) then
                add ("Execution." + key) "This value is fixed by Setup and cannot be overridden."
        for field, value in ["ProjectName", draft.ProjectName; "DatabaseName", draft.DatabaseName; "RunName", draft.RunName] do
            issues.AddRange(RunIdentity.validate field value)
        for phase, values, defaults in ["Setup", setup, template.SetupParameterSpans; "Execution", execution, template.RunParameterSpans] do
            issues.AddRange(ParameterValidation.validateSpans phase values)
            issues.AddRange(ParameterValidation.requireKeys phase (List.map fst defaults) values)
            for key, choices in values do
                if choices.IsEmpty || choices |> List.exists String.IsNullOrWhiteSpace then add (phase + "." + key) "Enter at least one nonempty value."
                if not (parameters template |> Array.exists (fun p -> p.Key = key)) then add (phase + "." + key) "This parameter is not part of the selected example."
        let effective = SgdParameterValidation.effectiveSpans setup execution
        issues.AddRange(ParameterValidation.poolSizes "Setup" setup)
        issues.AddRange(ParameterValidation.poolSizes "Execution" effective)
        if isEvolution template then
            for key, value in [ParamKeys.sortableTestsModuleKey, draft.SortableTestsModule; ParamKeys.sorterEvalModuleKey, draft.SorterEvalModule] do
                match ParameterValidation.validateValue key value with
                | Ok _ -> ()
                | Error error -> issues.Add(validationIssue.create ("Setup." + key) error)
            if not (String.IsNullOrWhiteSpace draft.CheckpointIntervalsName) && not (GenIntervalRegistry.genIntervalConfigsDict.ContainsKey draft.CheckpointIntervalsName) then
                add "CheckpointIntervalsName" "Choose a registered generation interval."
            for field, name in ["GenSaveIntervalsName", draft.GenSaveIntervalsName; "GenSaveSubIntervalsName", draft.GenSaveSubIntervalsName] do
                if String.IsNullOrWhiteSpace name || not (GenIntervalRegistry.genIntervalConfigsDict.ContainsKey name) then
                    add field "Choose a registered generation interval."
            match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue(if isNull draft.GenSaveIntervalsName then "" else draft.GenSaveIntervalsName), List.tryFind (fst >> (=) ParamKeys.generationIntervalLastKey) effective with
            | (true, config), Some (_, values) ->
                for value in values do
                    match ParameterValidation.tryInteger value with
                    | Some index -> match ParameterValidation.intervalIndex config index with Error message -> add ParamKeys.generationIntervalLastKey message | Ok () -> ()
                    | None -> ()
            | _ -> ()
        if template.ParameterSource.IsSome then
            if draft.SortableTestsModule <> string template.SgdSetup.Value.SortableTestsModule
               || draft.SorterEvalModule <> string template.SgdSetup.Value.SorterEvalModule then
                add "Setup" "A continuation imports its prepared inputs from the selected source run; its original library databases must be preserved."
            // The source is a complete saved configuration, not just three editable path strings.
            for key in [ParamKeys.sourceProjectNameKey; ParamKeys.sourceDbNameKey; ParamKeys.sourceRunNameKey; ParamKeys.sourceGenerationIntervalKey] do
                if List.tryFind (fst >> (=) key) execution <> List.tryFind (fst >> (=) key) template.RunParameterSpans then
                    add key "The example's continuation source must be preserved."
            for key, values in execution do
                if key <> ParamKeys.mutationModKey && values.Length <> 1 then add key "Only MutationMod may expand continuation executions."
            let sourceInterval = template.RunParameterSpans |> List.tryFind (fst >> (=) ParamKeys.sourceGenerationIntervalKey) |> Option.bind (snd >> List.tryHead) |> Option.bind ParameterValidation.tryInteger
            match sourceInterval, effective |> List.tryFind (fst >> (=) ParamKeys.generationIntervalLastKey) with
            | Some first, Some (_, values) ->
                for value in values do
                    if ParameterValidation.tryInteger value |> Option.exists (fun last -> last <= first) then
                        add ParamKeys.generationIntervalLastKey "The final interval must be after the source interval."
            | _ -> ()
        issues.ToArray()

    let build (template: run) (draft: runDraft) =
        let issues = validate template draft
        if issues.Length > 0 then Error issues
        else
            let execution: executionConfig = {
                FunctionName = template.ExecuteFunctionName; QueryCatalogName = template.RunQueryCatalogName
                ParametersEnhancerName = template.RunParametersEnhancerName; ParameterSpans = spans draft.ExecutionParameters }
            let configured =
                match template with
                | run.DirectRun _ ->
                    DirectRun.create {
                        ProjectName = UMX.tag draft.ProjectName; DatabaseName = UMX.tag draft.DatabaseName; RunName = UMX.tag draft.RunName
                        Description = draft.Description; AllowOverwrite = draft.AllowOverwrite; Execution = execution
                    } |> run.DirectRun
                | run.PreparedRun _ ->
                    SimpleRun.create {
                        ProjectName = UMX.tag draft.ProjectName; DatabaseName = UMX.tag draft.DatabaseName; RunName = UMX.tag draft.RunName
                        Description = draft.Description; AllowOverwrite = draft.AllowOverwrite; Execution = execution
                        Setup = { FunctionName = Run.requireSetupFunctionName template; QueryCatalogName = template.SetupQueryCatalogName
                                  ParametersEnhancerName = template.SetupParametersEnhancerName; ParameterSpans = spans draft.SetupParameters }
                    } |> Run.ofSimple
                | run.EvolutionRun _ | run.ContinuationRun _ ->
                    SgdRun.create {
                        ProjectName = UMX.tag draft.ProjectName; DatabaseName = UMX.tag draft.DatabaseName; RunName = UMX.tag draft.RunName
                        Description = draft.Description; AllowOverwrite = draft.AllowOverwrite; Execution = execution
                        Setup = { FunctionName = template.SgdSetup.Value.FunctionName
                                  SortableTestsModule = UMX.tag draft.SortableTestsModule
                                  SorterEvalModule = UMX.tag draft.SorterEvalModule
                                  QueryCatalogName = template.SetupQueryCatalogName; ParametersEnhancerName = template.SetupParametersEnhancerName
                                  ParameterSpans = spans draft.SetupParameters }
                    } { MakerNames = template.MakerNames.Value; GenSaveIntervalsName = draft.GenSaveIntervalsName
                        GenSaveSubIntervalsName = draft.GenSaveSubIntervalsName } |> SgdRun.withCheckpointIntervals (Option.ofObj draft.CheckpointIntervalsName) |> Run.ofSgd
            match template.ParameterSource with
            | Some (source, selections) -> Run.withParameterSource source selections configured |> Ok
            | None -> Ok configured


    /// Module-owned settings and phase membership remain fixed while editing a leaf.
    let buildModuleRun (template: run) (draft: runDraft) =
        let original = (fromRun template).ToArgs()
        let edited = draft.ToArgs()
        let issues = ResizeArray<validationIssue>()
        match channelCounts template with
        | [|count|] -> issues.AddRange(validateChannelCount count template draft)
        | _ -> issues.Add(validationIssue.create "ChannelCount" "Choose a source run with one fixed channel count.")
        let add field = issues.Add(validationIssue.create field "This property is owned by the module and cannot be edited.")
        if edited.ProjectName <> original.ProjectName then add "ProjectName"
        if edited.DatabaseName <> original.DatabaseName then add "DatabaseName"
        if edited.AllowOverwrite <> original.AllowOverwrite then add "AllowOverwrite"
        if edited.SortableTestsModule <> original.SortableTestsModule then add "SortableTestsModule"
        if edited.SorterEvalModule <> original.SorterEvalModule then add "SorterEvalModule"
        if edited.CheckpointIntervalsName <> original.CheckpointIntervalsName then add "CheckpointIntervalsName"
        if edited.GenSaveIntervalsName <> original.GenSaveIntervalsName then add "GenSaveIntervalsName"
        if edited.GenSaveSubIntervalsName <> original.GenSaveSubIntervalsName then add "GenSaveSubIntervalsName"
        for phase, rows, declared in ["Setup", draft.SetupParameters, template.SetupParameterSpans
                                      "Execution", draft.ExecutionParameters, template.RunParameterSpans] do
            let keys = declared |> List.map fst |> Set.ofList
            for key, values in declared do
                if isModuleOwnedParameter template key then
                    let actual = rows |> Array.filter (fun row -> row.Key = key) |> Array.collect (fun row -> row.Values) |> Array.toList
                    if actual <> values then add (phase + "." + key)
                elif List.contains key [ParamKeys.aMaxKey; ParamKeys.activationProbabilityKey]
                     && not (rows |> Array.exists (fun row -> row.Key = key && row.Values.Length > 0)) then
                    issues.Add(validationIssue.create (phase + "." + key) "This model requires an explicit activation setting.")
            for row in rows do
                if not (Set.contains row.Key keys) then
                    issues.Add(validationIssue.create (phase + "." + row.Key) "Only parameters declared in this phase of the source run may be edited.")
        if issues.Count > 0 then Error (issues.ToArray()) else build template draft
