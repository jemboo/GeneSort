namespace GeneSort.Project.V1.Parameters

open System
open System.Globalization
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter

type validationIssue =
    private { field: string; message: string }
    member this.Field = this.field
    member this.Message = this.message
    static member create field message = { field = field; message = message }

type internal valueRule =
    | Integer of minimum: int
    | Number of minimum: float * maximum: float option
    | Choice of string list
    | Text

/// Value shape exposed to editors; the validation rules remain the authority for valid input.
type parameterValueKind =
    | Integer = 0
    | Number = 1
    | Choice = 2
    | Text = 3

/// A parameter's meaning and value constraints, independent of any editor.
type parameterRule =
    private { key: string; description: string; rule: valueRule }
    member this.Key = this.key
    member this.Description = this.description
    member this.Choices = match this.rule with Choice values -> List.toArray values | _ -> [||]
    member this.ValueKind =
        match this.rule with
        | Integer _ -> parameterValueKind.Integer
        | Number _ -> parameterValueKind.Number
        | Choice _ -> parameterValueKind.Choice
        | Text -> parameterValueKind.Text
    member this.Minimum =
        match this.rule with Integer minimum -> Some (float minimum) | Number (minimum, _) -> Some minimum | _ -> None
    member this.Maximum =
        match this.rule with Integer _ -> Some (float Int32.MaxValue) | Number (_, maximum) -> maximum | _ -> None
    member internal this.Rule = this.rule
    static member internal create key description rule = { key = key; description = description; rule = rule }

module ParameterValidation =
    let private rules =
        [
            parameterRule.create ParamKeys.sorterEvalModuleKey "Producing SorterEval module; supplies its database and execution query catalog." Text
            parameterRule.create ParamKeys.sortableTestsModuleKey "Producing module, for example SortableTests.Prefix.Channels32. Its metadata supplies the database and execution query catalog." Text
            parameterRule.create ParamKeys.excludeSelfCeKey "Module policy: exclude self comparators." (Choice ["true"; "false"])
            parameterRule.create ParamKeys.collectSortableTestsKey "Module policy: collect new sortable tests." (Choice ["true"; "false"])
            parameterRule.create ParamKeys.orthoRateKey "Nonnegative orthogonal mutation weight." (Number (0.0, None))
            parameterRule.create ParamKeys.sorterChildCountKey "Positive number of children per sorter." (Integer 1)
            parameterRule.create ParamKeys.selectedSorterCountPerPoolKey "Positive selected count, or All to select the current pool capacity." Text
            parameterRule.create ParamKeys.seedSorterPoolSelectionTypeKey "Seed selection strategy and positive count, for example GuidOrder:512." Text
            parameterRule.create ParamKeys.sortedFractionKey "Finite sorted fraction, from 0 to 1." (Number (0.0, Some 1.0))
            parameterRule.create ParamKeys.distinctSorterHashesKey "Require distinct sorter hashes." (Choice ["true"; "false"])
            parameterRule.create ParamKeys.prioritizeNewMutantsKey "Prefer new mutants during selection." (Choice ["true"; "false"])
            parameterRule.create ParamKeys.mergeLibIdKey "Merge library identity with a positive channel count divisible by its dimension." Text
            parameterRule.create ParamKeys.prefixLibIdKey "Prefix library identity with a positive channel count and stage length." Text
            parameterRule.create ParamKeys.sorterLibIdKey "Sorter library identity with a positive channel count." Text
            parameterRule.create ParamKeys.sorterCountPerPoolKey "Positive number of sorters in each pool." (Integer 1)
            parameterRule.create ParamKeys.sorterPoolCountKey "Positive number of sorter pools." (Integer 1)
            parameterRule.create ParamKeys.codeModKey "Nonempty configuration label used in query identity." Text
            parameterRule.create ParamKeys.adjustCeLengthFrequencyKey "Positive generation frequency for adjusting comparator length." (Integer 1)
            parameterRule.create ParamKeys.generationIntervalLastKey "Nonnegative zero-based index into the generation save schedule." (Integer 0)
            parameterRule.create ParamKeys.paraRateKey "Nonnegative parallel weight; values above 1 are allowed." (Number (0.0, None))
            parameterRule.create ParamKeys.selfSymRateKey "Nonnegative self-symmetry weight; values above 1 are allowed." (Number (0.0, None))
            parameterRule.create ParamKeys.mutationModKey "Nonnegative mutation variant used in query identity and seeding." (Integer 0)
            parameterRule.create ParamKeys.seedModificationRateKey "Finite seed modification probability, from 0 to 1." (Number (0.0, Some 1.0))
            parameterRule.create ParamKeys.modificationRateKey "Finite modification probability, from 0 to 1." (Number (0.0, Some 1.0))
            parameterRule.create ParamKeys.mutatorVariantKey "Mutation algorithm: V1, V2 or V3." (Choice ([mutatorVariant.V1; mutatorVariant.V2; mutatorVariant.V3] |> List.map MutatorVariant.toString))
            parameterRule.create ParamKeys.aMaxKey "Nonnegative integer activator bound." (Integer 0)
            parameterRule.create ParamKeys.activationProbabilityKey "Finite activation probability, from 0 to 1." (Number (0.0, Some 1.0))
            parameterRule.create ParamKeys.activatorMutationRateKey "Finite activator mutation probability, from 0 to 1." (Number (0.0, Some 1.0))
        ] |> List.map (fun rule -> rule.Key, rule) |> Map.ofList

    let describe key = Map.find key rules
    /// Other workflows may define keys without a specialized numeric or choice rule.
    let describeOrText key =
        Map.tryFind key rules
        |> Option.defaultWith (fun () -> parameterRule.create key (sprintf "Value for %s." key) Text)
    let private parseInteger (value: string) = Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)
    let tryInteger value = match parseInteger value with true, number -> Some number | _ -> None

    /// Return a canonical value for duplicate detection; leave unregistered parameters to their own workflows.
    let validateValue key (value: string) =
        match Map.tryFind key rules with
        | _ when List.contains key [ParamKeys.excludeSelfCeKey; ParamKeys.collectSortableTestsKey; ParamKeys.distinctSorterHashesKey; ParamKeys.prioritizeNewMutantsKey] ->
            match Boolean.TryParse value with
            | true, parsed -> Ok (if parsed then "true" else "false")
            | _ -> Error "Choose true or false."
        | _ when key = ParamKeys.selectedSorterCountPerPoolKey ->
            if System.String.Equals(value, "All", StringComparison.OrdinalIgnoreCase) then Ok "All"
            else match tryInteger value with
                 | Some count when count > 0 -> Ok (string count)
                 | _ -> Error "Enter a positive selected count or All."
        | _ when key = ParamKeys.seedSorterPoolSelectionTypeKey ->
            try
                let selection = GeneSort.Eval.V1.SorterSelectionType.fromString value
                let canonical = GeneSort.Eval.V1.SorterSelectionType.toString selection
                if int (canonical.Split(':')[1]) <= 0 then Error "Seed selection count must be positive."
                else Ok canonical
            with _ -> Error "Use a seed selection strategy and positive count, such as GuidOrder:512."
        | _ when key = ParamKeys.prefixLibIdKey ->
            try
                let id = PrefixLibId.fromString value
                if int id.ChannelCount <= 0 || int id.StageLength <= 0 then
                    Error "Choose a PrefixLibId with a positive channel count and stage length."
                else Ok (PrefixLibId.toString id)
            with _ -> Error "Choose a valid PrefixLibId."
        | _ when key = ParamKeys.sorterLibIdKey ->
            try
                let id = SorterLibId.fromString value
                if int id.ChannelCount <= 0 then Error "Choose a SorterLibId with a positive channel count."
                else Ok (SorterLibId.toString id)
            with _ -> Error "Choose a valid SorterLibId."
        | _ when key = ParamKeys.mergeLibIdKey ->
            try
                let id = MergeLibId.fromString value
                if int id.ChannelCount <= 0 || int id.MergeDimension <= 0
                   || int id.ChannelCount % int id.MergeDimension <> 0 then
                    Error "Choose a MergeLibId with a positive channel count divisible by its positive dimension."
                else Ok (MergeLibId.toString id)
            with _ -> Error "Choose a valid MergeLibId."
        | _ when List.contains key [ParamKeys.sortableTestsModuleKey; ParamKeys.sorterEvalModuleKey] ->
            let parts = if isNull value then [||] else value.Split('.')
            if parts.Length < 2 || parts |> Array.exists (fun part ->
                String.IsNullOrWhiteSpace part || not (Char.IsLetter part[0] || part[0] = '_')
                || part |> Seq.exists (fun c -> not (Char.IsLetterOrDigit c || c = '_'))) then
                Error "Use a module name such as SortableTests.Prefix.Channels32."
            else Ok value
        | None -> Ok value
        | Some spec ->
            let valid, canonical =
                match spec.Rule with
                | Integer minimum ->
                    let ok, number = parseInteger value
                    ok && number >= minimum, string number
                | Number (minimum, maximum) ->
                    let ok, number = Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture)
                    ok && Double.IsFinite number && number >= minimum && (maximum |> Option.forall (fun max -> number <= max)),
                    (if number = 0.0 then "0" else number.ToString("R", CultureInfo.InvariantCulture))
                | Choice choices -> List.contains value choices, value
                | Text -> not (String.IsNullOrWhiteSpace value), value
            if valid then Ok canonical else Error (sprintf "Invalid value '%s'. %s" value spec.Description)

    let private field phase key = if String.IsNullOrEmpty phase then key else phase + "." + key

    let validateSpans phase (spans: (string * string list) list) =
        let issues = ResizeArray<validationIssue>()
        let seen = Collections.Generic.HashSet<string>(StringComparer.Ordinal)
        for index, (key, values) in List.indexed spans do
            let location = sprintf "%s[%d].%s" phase (index + 1) key
            let add message = issues.Add(validationIssue.create location message)
            if String.IsNullOrWhiteSpace key then add "Enter a parameter key."
            if not (seen.Add key) then add "This parameter is listed more than once in this phase."
            let canonical = Collections.Generic.HashSet<string>(StringComparer.Ordinal)
            for value in values do
                match validateValue key value with
                | Error message -> add message
                | Ok normalized when not (canonical.Add normalized) -> add (sprintf "Value '%s' is duplicated." value)
                | Ok _ -> ()
        issues.ToArray()

    let requireKeys phase keys spans =
        keys |> List.choose (fun key ->
            if spans |> List.exists (fst >> (=) key) then None
            else Some (validationIssue.create (field phase key) "This parameter is required.")) |> List.toArray

    /// Check only the requested runtime values; other workflows may have additional parameters.
    let validateValues keys (parameters: Map<string, string>) =
        keys |> List.choose (fun key ->
            match Map.tryFind key parameters with
            | None -> Some (validationIssue.create key "This parameter is required.")
            | Some value ->
                match validateValue key value with
                | Ok _ -> None
                | Error message -> Some (validationIssue.create key message)) |> List.toArray

    let ofMap parameters = parameters |> Map.toList |> List.map (fun (key, value) -> key, [value])

    /// Effective collections must no longer contain unresolved symbolic choices.
    let resolvedValues phase parameters =
        [| if Map.tryFind ParamKeys.selectedSorterCountPerPoolKey parameters
                  |> Option.exists (fun value -> String.Equals(value, "All", StringComparison.OrdinalIgnoreCase)) then
               yield validationIssue.create (field phase ParamKeys.selectedSorterCountPerPoolKey) "All requires SorterCountPerPool."
           if Map.tryFind ParamKeys.sorterEvalMeasureInitialKey parameters = Some "SameAsEvaluation" then
               yield validationIssue.create (field phase ParamKeys.sorterEvalMeasureInitialKey) "SameAsEvaluation requires SorterEvalMeasure." |]

    /// Check every declared pool-size combination without expanding unrelated parameters.
    let poolSizes phase spans =
        let positiveIntegers key =
            spans |> List.filter (fst >> (=) key) |> List.collect snd
            |> List.choose (fun value -> match parseInteger value with true, number when number > 0 -> Some number | _ -> None)
        [| for size in positiveIntegers ParamKeys.sorterCountPerPoolKey do
               for count in positiveIntegers ParamKeys.sorterPoolCountKey do
                   if int64 size * int64 count > int64 System.Int32.MaxValue then
                       yield validationIssue.create (field phase ParamKeys.sorterPoolCountKey)
                           "SorterPoolCount * SorterCountPerPool exceeds the supported capacity." |]

    let intervalIndex (config: genIntervalConfig) index =
        if index < 0 || (config.MaxCount |> Option.exists (fun count -> index >= count)) then
            Error (sprintf "Index %d is outside schedule '%s'." index config.Name)
        else Ok ()

    let toResult (issues: validationIssue[]) =
        if Array.isEmpty issues then Ok ()
        else Error (issues |> Array.map (fun issue -> issue.Field + ": " + issue.Message) |> String.concat "\n")

    let ensureValid argument issues =
        match toResult issues with Ok () -> () | Error message -> invalidArg argument message
