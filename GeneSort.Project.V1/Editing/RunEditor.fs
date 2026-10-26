namespace GeneSort.Project.V1.Editing

open System
open System.Text.RegularExpressions
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.Project.V1.Workflows

/// Names are path components in a saved database, including when authored outside WPF.
module RunIdentity =
    let validate field (value: string) =
        let invalid message = [| validationIssue.create field message |]
        if String.IsNullOrWhiteSpace value then invalid "Enter a name."
        elif value <> value.Trim() || value.EndsWith('.') then invalid "Names cannot begin or end with spaces, or end with a dot."
        elif value = "." || value = ".." || value |> Seq.exists (fun c -> int c < 32 || "<>:\"/\\|?*".Contains c) then
            invalid "Use a single folder name without path separators or reserved filename characters."
        elif field = "RunName" && value.StartsWith("RunParameters_", StringComparison.OrdinalIgnoreCase) then
            invalid "Run names cannot start with 'RunParameters_', which is reserved for parameter folders."
        elif Regex.IsMatch(value, @"^(CON|PRN|AUX|NUL|COM[1-9¹²³]|LPT[1-9¹²³])(\.|$)", RegexOptions.IgnoreCase) then
            invalid "This name is reserved by Windows."
        else [||]

/// Presentation policy is separate from the parameter's shared domain rule.
type parameterDescription =
    private { rule: parameterRule; requiredInSetup: bool; executionAllowed: bool }
    member this.Key = this.rule.Key
    member this.Description = this.rule.Description
    member this.RequiredInSetup = this.requiredInSetup
    member this.ExecutionAllowed = this.executionAllowed
    member this.Choices = this.rule.Choices
    member this.ValueKind = this.rule.ValueKind
    member this.Minimum = this.rule.Minimum
    member this.Maximum = this.rule.Maximum
    static member internal create key requiredInSetup executionAllowed =
        { rule = ParameterValidation.describe key; requiredInSetup = requiredInSetup; executionAllowed = executionAllowed }

/// Immutable snapshots; the UI owns mutable text fields, not domain records.
type parameterDraft =
    private { key: string; values: string list }
    member this.Key = this.key
    member this.Values = List.toArray this.values
    static member create key (values: string[]) =
        { key = key; values = values |> Option.ofObj |> Option.defaultValue [||] |> Array.toList }

[<CLIMutable>]
type runDraftArgs = {
    ProjectName: string
    DatabaseName: string
    RunName: string
    Description: string
    AllowOverwrite: bool
    GenSaveIntervalsName: string
    GenSaveSubIntervalsName: string
    SetupParameters: parameterDraft[]
    ExecutionParameters: parameterDraft[]
}

type runDraft =
    private { settings: runDraftArgs }
    member this.ProjectName = this.settings.ProjectName
    member this.DatabaseName = this.settings.DatabaseName
    member this.RunName = this.settings.RunName
    member this.Description = this.settings.Description
    member this.AllowOverwrite = this.settings.AllowOverwrite
    member this.GenSaveIntervalsName = this.settings.GenSaveIntervalsName
    member this.GenSaveSubIntervalsName = this.settings.GenSaveSubIntervalsName
    member this.SetupParameters = Array.copy this.settings.SetupParameters
    member this.ExecutionParameters = Array.copy this.settings.ExecutionParameters
    member this.ToArgs() =
        { this.settings with SetupParameters = this.SetupParameters; ExecutionParameters = this.ExecutionParameters }
    static member create (args: runDraftArgs) =
        let copy values = values |> Option.ofObj |> Option.defaultValue [||] |> Array.copy
        { settings = { args with SetupParameters = copy args.SetupParameters; ExecutionParameters = copy args.ExecutionParameters } }

/// The first editor profile deliberately exposes only the Standard Msuf32p4a workflow.
/// Catalog selections form a compatible group; new workflows can supply their own group and schema.
module RunEditor =
    let private workflow = WorkflowDefinitions.standardMsuf32
    let workflowName = workflow.Name
    let workflowDescription = workflow.Description
    let catalogChoices () = workflow.CatalogChoices

    let private schema =
        [| ParamKeys.sorterCountPerPoolKey; ParamKeys.sorterCountPerPoolSetKey; ParamKeys.codeModKey
           ParamKeys.adjustCeLengthFrequencyKey; ParamKeys.generationIntervalLastKey; ParamKeys.paraRateKey
           ParamKeys.selfSymRateKey; ParamKeys.mutationModKey; ParamKeys.seedModificationRateKey
           ParamKeys.modificationRateKey; ParamKeys.mutatorVariantKey |]
        |> Array.map (fun key ->
            parameterDescription.create key
                (List.contains key SgdParameterValidation.msufQueryKeys)
                (not (List.contains key SgdParameterValidation.seedPoolKeys)))

    let parameters () = Array.copy schema
    let intervals () =
        GenIntervalRegistry.genIntervalConfigsDict.Values
        |> Seq.sortBy (fun config -> config.Name)
        |> Seq.map (fun config -> catalogChoice.create "Save interval" config.Name (GenIntervalConfig.toString config))
        |> Seq.toArray

    let newDraft () =
        let parameter key values = parameterDraft.create key values
        runDraft.create {
            ProjectName = "SorterSgd.p32.Msuf4a.Standard"; DatabaseName = "Test"; RunName = "NewRun"
            Description = "Standard Msuf32p4a run"; AllowOverwrite = false
            GenSaveIntervalsName = GenIntervalNames.uniformInterval10
            GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
            SetupParameters = [|
                parameter ParamKeys.sorterCountPerPoolKey [|"32"|]
                parameter ParamKeys.sorterCountPerPoolSetKey [|"32"|]
                parameter ParamKeys.codeModKey [|"NoMods"|]
                parameter ParamKeys.adjustCeLengthFrequencyKey [|"10"|]
                parameter ParamKeys.generationIntervalLastKey [|"3"|]
                parameter ParamKeys.paraRateKey [|"1.15"|]
                parameter ParamKeys.selfSymRateKey [|"1.75"|]
                parameter ParamKeys.mutationModKey [|"0"; "1"; "2"; "3"|]
                parameter ParamKeys.seedModificationRateKey [|"0.020"|]
                parameter ParamKeys.modificationRateKey [|"0.035"|]
                parameter ParamKeys.mutatorVariantKey [|"V2"|]
            |]
            ExecutionParameters = [||]
        }

    let private spans rows = rows |> Array.map (fun (row: parameterDraft) -> row.Key, Array.toList row.Values) |> Array.toList

    let validate (draft: runDraft) =
        let issues = ResizeArray<validationIssue>()
        let add field message = issues.Add(validationIssue.create field message)
        issues.AddRange(SgdParameterValidation.standardMsuf (spans draft.SetupParameters) (spans draft.ExecutionParameters))
        for field, value in ["ProjectName", draft.ProjectName; "DatabaseName", draft.DatabaseName; "RunName", draft.RunName] do
            issues.AddRange(RunIdentity.validate field value)
        for field, name in ["GenSaveIntervalsName", draft.GenSaveIntervalsName; "GenSaveSubIntervalsName", draft.GenSaveSubIntervalsName] do
            if String.IsNullOrWhiteSpace name || not (GenIntervalRegistry.genIntervalConfigsDict.ContainsKey name) then
                add field "Choose a registered generation interval."
        for phase, rows in ["Setup", draft.SetupParameters; "Execution", draft.ExecutionParameters] do
            for index, row in Array.indexed rows do
                let field = sprintf "%s[%d].%s" phase (index + 1) row.Key
                if row.Values.Length = 0 then add field "Enter at least one value."
                match schema |> Array.tryFind (fun spec -> spec.Key = row.Key) with
                | None -> add field "This parameter is not supported by this editor profile."
                | Some spec ->
                    if phase = "Execution" && not spec.ExecutionAllowed then add field "Set pool sizes during setup."
        let find key rows = rows |> Array.tryFind (fun (row: parameterDraft) -> row.Key = key)
        let effective key = find key draft.ExecutionParameters |> Option.orElseWith (fun () -> find key draft.SetupParameters)
        if not (String.IsNullOrWhiteSpace draft.GenSaveIntervalsName) then
            match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue draft.GenSaveIntervalsName, effective ParamKeys.generationIntervalLastKey with
            | (true, config), Some row ->
                for value in row.Values do
                    match ParameterValidation.tryInteger value with
                    | Some index ->
                        match ParameterValidation.intervalIndex config index with
                        | Error message -> add ParamKeys.generationIntervalLastKey message
                        | Ok () -> ()
                    | _ -> ()
            | _ -> ()
        issues.ToArray()

    let build (draft: runDraft) =
        let issues = validate draft
        if issues.Length > 0 then Error issues
        else
            SgdRun.create {
                DatabaseName = UMX.tag draft.DatabaseName; ProjectName = UMX.tag draft.ProjectName; RunName = UMX.tag draft.RunName
                Description = draft.Description; AllowOverwrite = draft.AllowOverwrite
                Setup = workflow.SetupConfig (spans draft.SetupParameters)
                Execution = workflow.ExecutionConfig (spans draft.ExecutionParameters)
            } {
                MakerNames = workflow.MakerNames
                GenSaveIntervalsName = draft.GenSaveIntervalsName; GenSaveSubIntervalsName = draft.GenSaveSubIntervalsName
            } |> run.SgdRun |> Ok

    /// Reject incompatible catalogs rather than silently changing an opened run's behavior.
    let fromRun (configured: run) =
        if configured.ParameterSource.IsSome
           || not (workflow.Matches configured) then
            Error "This editor supports Standard Msuf32p4a runs with the prefix makers. Other workflows and continuation runs cannot be edited here yet."
        else
            let rows spans = spans |> List.map (fun (key, values) -> parameterDraft.create key (List.toArray values)) |> List.toArray
            runDraft.create {
                ProjectName = %configured.ProjectName; DatabaseName = %configured.DatabaseName; RunName = %configured.RunName
                Description = configured.Description; AllowOverwrite = configured.AllowOverwrite
                GenSaveIntervalsName = configured.GenSaveIntervalsName.Value; GenSaveSubIntervalsName = configured.GenSaveSubIntervalsName.Value
                SetupParameters = rows configured.SetupParameterSpans; ExecutionParameters = rows configured.RunParameterSpans
            } |> Ok
