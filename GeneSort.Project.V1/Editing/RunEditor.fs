namespace GeneSort.Project.V1.Editing

open System
open System.Globalization
open System.Text.RegularExpressions
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters

type validationIssue =
    private { field: string; message: string }
    member this.Field = this.field
    member this.Message = this.message
    static member create field message = { field = field; message = message }

/// Names are path components in a saved database, including when authored outside WPF.
module RunIdentity =
    let validate field (value: string) =
        let invalid message = [| validationIssue.create field message |]
        if String.IsNullOrWhiteSpace value then invalid "Enter a name."
        elif value <> value.Trim() || value.EndsWith('.') then invalid "Names cannot begin or end with spaces, or end with a dot."
        elif value = "." || value = ".." || value |> Seq.exists (fun c -> int c < 32 || "<>:\"/\\|?*".Contains c) then
            invalid "Use a single folder name without path separators or reserved filename characters."
        elif Regex.IsMatch(value, @"^(CON|PRN|AUX|NUL|COM[1-9¹²³]|LPT[1-9¹²³])(\.|$)", RegexOptions.IgnoreCase) then
            invalid "This name is reserved by Windows."
        else [||]

type catalogChoice =
    private { role: string; name: string; description: string }
    member this.Role = this.role
    member this.Name = this.name
    member this.Description = this.description
    static member create role name description = { role = role; name = name; description = description }

type internal valueRule =
    | Integer of minimum: int
    | Number of minimum: float * maximum: float option
    | Choice of string list
    | Text

type parameterDescription =
    private { key: string; description: string; requiredInSetup: bool; executionAllowed: bool; rule: valueRule }
    member this.Key = this.key
    member this.Description = this.description
    member this.RequiredInSetup = this.requiredInSetup
    member this.ExecutionAllowed = this.executionAllowed
    member this.Choices = match this.rule with Choice values -> List.toArray values | _ -> [||]
    static member internal create key description requiredInSetup executionAllowed rule =
        { key = key; description = description; requiredInSetup = requiredInSetup; executionAllowed = executionAllowed; rule = rule }
    member internal this.Rule = this.rule

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
    let workflowName = "Standard · Msuf32p4a"
    let workflowDescription = "Simple sorter models, width 32, prefix p4a, Standard evolution."

    let catalogChoices () = [|
        catalogChoice.create "Setup" SetupCatalogNames.standard "Create the initial Standard pools and prefixed sortable tests."
        catalogChoice.create "Setup query" QueryCatalogNamesSetup.sorterSgdMsufStandard "Identify Standard Msuf setup inputs."
        catalogChoice.create "Setup parameters" SetupEnhancerFunctionNames.standardMsuf32 "Supply width 32, prefix p4a and initial pool settings."
        catalogChoice.create "Execution" ExecuteFunctionNames.standard "Evolve Standard sorter pools."
        catalogChoice.create "Execution query" QueryCatalogNamesRun.sorterSgdMsufStandard "Identify Standard Msuf execution results."
        catalogChoice.create "Execution parameters" RunParamBuilderNames.Enhancer.evoMsuf32 "Inherit setup parameters and apply execution overrides."
        catalogChoice.create "Tests maker" EvolutionMakerNames.prefix.SortableTests "Load sortable tests with the selected prefix."
        catalogChoice.create "Seed maker" EvolutionMakerNames.prefix.SeedPoolSet "Create Standard pools from the prefix seed library."
        catalogChoice.create "Mutator" EvolutionMakerNames.prefix.Mutator "Mutate the underlying simple sorter model."
    |]

    let private schema = [|
        parameterDescription.create ParamKeys.sorterCountPerPoolKey "Positive number of sorters in each pool; set during setup." true false (Integer 1)
        parameterDescription.create ParamKeys.sorterCountPerPoolSetKey "Total sorters; must be divisible by every per-pool option." true false (Integer 1)
        parameterDescription.create ParamKeys.codeModKey "Nonempty configuration label used in query identity." true true Text
        parameterDescription.create ParamKeys.adjustCeLengthFrequencyKey "Positive generation frequency for adjusting comparator length." false true (Integer 1)
        parameterDescription.create ParamKeys.generationIntervalLastKey "Zero-based index into the generation save schedule (0 is its first sample)." false true (Integer 0)
        parameterDescription.create ParamKeys.paraRateKey "Nonnegative parallel weight; values above 1 are allowed." true true (Number (0.0, None))
        parameterDescription.create ParamKeys.selfSymRateKey "Nonnegative self-symmetry weight; values above 1 are allowed." true true (Number (0.0, None))
        parameterDescription.create ParamKeys.mutationModKey "Nonnegative mutation variant used in query identity and seeding." true true (Integer 0)
        parameterDescription.create ParamKeys.seedModificationRateKey "Seed modification probability, from 0 to 1." true true (Number (0.0, Some 1.0))
        parameterDescription.create ParamKeys.modificationRateKey "Modification probability, from 0 to 1." true true (Number (0.0, Some 1.0))
        parameterDescription.create ParamKeys.mutatorVariantKey "Mutation algorithm: V1, V2 or V3." true true (Choice ["V1"; "V2"; "V3"])
    |]

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

    let private parseInteger (value: string) = Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)
    let private parseNumber (value: string) = Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture)

    let validate (draft: runDraft) =
        let issues = ResizeArray<validationIssue>()
        let add field message = issues.Add(validationIssue.create field message)
        for field, value in ["ProjectName", draft.ProjectName; "DatabaseName", draft.DatabaseName; "RunName", draft.RunName] do
            issues.AddRange(RunIdentity.validate field value)
        for field, name in ["GenSaveIntervalsName", draft.GenSaveIntervalsName; "GenSaveSubIntervalsName", draft.GenSaveSubIntervalsName] do
            if String.IsNullOrWhiteSpace name || not (GenIntervalRegistry.genIntervalConfigsDict.ContainsKey name) then
                add field "Choose a registered generation interval."
        for phase, rows in ["Setup", draft.SetupParameters; "Execution", draft.ExecutionParameters] do
            let seen = Collections.Generic.HashSet<string>(StringComparer.Ordinal)
            for index, row in Array.indexed rows do
                let field = sprintf "%s[%d].%s" phase (index + 1) row.Key
                if not (seen.Add row.Key) then add field "This parameter is listed more than once in this phase."
                match schema |> Array.tryFind (fun spec -> spec.Key = row.Key) with
                | None -> add field "This parameter is not supported by this editor profile."
                | Some spec ->
                    if phase = "Execution" && not spec.ExecutionAllowed then add field "Set pool sizes during setup."
                    if row.Values.Length = 0 then add field "Enter at least one value."
                    let canonical = Collections.Generic.HashSet<string>(StringComparer.Ordinal)
                    for value in row.Values do
                        let valid, normalized =
                            match spec.Rule with
                            | Integer minimum ->
                                let ok, number = parseInteger value
                                ok && number >= minimum, string number
                            | Number (minimum, maximum) ->
                                let ok, number = parseNumber value
                                ok && Double.IsFinite number && number >= minimum && (maximum |> Option.forall (fun max -> number <= max)), number.ToString("R", CultureInfo.InvariantCulture)
                            | Choice choices -> List.contains value choices, value
                            | Text -> not (String.IsNullOrWhiteSpace value), value
                        if not valid then add field (sprintf "Invalid value '%s'. %s" value spec.Description)
                        elif not (canonical.Add normalized) then add field (sprintf "Value '%s' is duplicated." value)
        let find key rows = rows |> Array.tryFind (fun (row: parameterDraft) -> row.Key = key)
        let effective key = find key draft.ExecutionParameters |> Option.orElseWith (fun () -> find key draft.SetupParameters)
        for spec in schema do
            let found = if spec.RequiredInSetup then find spec.Key draft.SetupParameters else effective spec.Key
            if found.IsNone then add (sprintf "%s.%s" (if spec.RequiredInSetup then "Setup" else "Parameters") spec.Key) "This parameter is required."
        let integers key =
            find key draft.SetupParameters |> Option.map (fun row -> row.Values |> Array.choose (fun value ->
                match parseInteger value with true, number when number > 0 -> Some number | _ -> None)) |> Option.defaultValue [||]
        for size in integers ParamKeys.sorterCountPerPoolKey do
            for total in integers ParamKeys.sorterCountPerPoolSetKey do
                if total % size <> 0 then add ("Setup." + ParamKeys.sorterCountPerPoolSetKey) (sprintf "Total %d is not divisible by per-pool count %d." total size)
        if not (String.IsNullOrWhiteSpace draft.GenSaveIntervalsName) then
            match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue draft.GenSaveIntervalsName, effective ParamKeys.generationIntervalLastKey with
            | (true, config), Some row ->
                for value in row.Values do
                    match parseInteger value with
                    | true, index when index >= 0 ->
                        if config.MaxCount |> Option.exists (fun count -> index >= count) then
                            add ParamKeys.generationIntervalLastKey (sprintf "Index %d is outside schedule '%s'." index config.Name)
                    | _ -> ()
            | _ -> ()
        issues.ToArray()

    let build (draft: runDraft) =
        let issues = validate draft
        if issues.Length > 0 then Error issues
        else
            let spans rows = rows |> Array.map (fun (row: parameterDraft) -> row.Key, Array.toList row.Values) |> Array.toList
            SgdRun.create {
                DatabaseName = UMX.tag draft.DatabaseName; ProjectName = UMX.tag draft.ProjectName; RunName = UMX.tag draft.RunName
                Description = draft.Description; AllowOverwrite = draft.AllowOverwrite
                Setup = {
                    FunctionName = SetupCatalogNames.standard; ParameterSpans = spans draft.SetupParameters
                    QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard; EnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.standard; ParameterSpans = spans draft.ExecutionParameters
                    QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard; EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = draft.GenSaveIntervalsName; GenSaveSubIntervalsName = draft.GenSaveSubIntervalsName
            } |> run.SgdRun |> Ok

    /// Reject incompatible catalogs rather than silently changing an opened run's behavior.
    let fromRun (configured: run) =
        if configured.ParameterSource.IsSome
           || configured.SetupFunctionName <> SetupCatalogNames.standard
           || configured.SetupQueryCatalogName <> QueryCatalogNamesSetup.sorterSgdMsufStandard
           || configured.SetupEnhancerFunctionName <> SetupEnhancerFunctionNames.standardMsuf32
           || configured.ExecuteFunctionName <> ExecuteFunctionNames.standard
           || configured.RunQueryCatalogName <> QueryCatalogNamesRun.sorterSgdMsufStandard
           || configured.RunEnhancerFunctionName <> RunParamBuilderNames.Enhancer.evoMsuf32
           || configured.MakerNames <> Some EvolutionMakerNames.prefix then
            Error "This editor supports Standard Msuf32p4a runs with the prefix makers. Other workflows and continuation runs cannot be edited here yet."
        else
            let rows spans = spans |> List.map (fun (key, values) -> parameterDraft.create key (List.toArray values)) |> List.toArray
            runDraft.create {
                ProjectName = %configured.ProjectName; DatabaseName = %configured.DatabaseName; RunName = %configured.RunName
                Description = configured.Description; AllowOverwrite = configured.AllowOverwrite
                GenSaveIntervalsName = configured.GenSaveIntervalsName.Value; GenSaveSubIntervalsName = configured.GenSaveSubIntervalsName.Value
                SetupParameters = rows configured.SetupParameterSpans; ExecutionParameters = rows configured.RunParameterSpans
            } |> Ok
