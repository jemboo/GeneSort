module GeneSort.Dispatch.Test.V1.RunTypeTests

open System
open System.Reflection
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Sorting
open GeneSort.Core

[<Fact>]
let ``Every module and run definition has an explicit catalog entry`` () =
    let methods =
        typeof<runHost>.Assembly.GetTypes()
        |> Array.filter (fun t -> ["SortableTests"; "SorterEval"; "SorterMutate"; "SorterSgd"]
                                  |> List.exists (fun name -> t.FullName.StartsWith("GeneSort.Dispatch.V1." + name + ".", StringComparison.Ordinal)))
        |> Array.collect (fun t -> t.GetMethods(BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly))
        |> Array.filter (fun m -> m.ReturnType = typeof<run> && m.GetParameters().Length = 0)
    let expected = methods |> Array.map (fun m -> m.DeclaringType.FullName.Replace("+", ".").Replace("GeneSort.Dispatch.V1.", "") + "." + m.Name) |> Set.ofArray
    let actual = RunTypeCatalog.all |> Array.collect (fun kind -> kind.Examples |> Array.map (fun example -> kind.Name + "." + example.Name))
    Assert.Equal(actual.Length, (Set.ofArray actual).Count)
    Assert.True((expected = Set.ofArray actual), sprintf "Missing: %A; extra: %A" (expected - Set.ofArray actual) (Set.ofArray actual - expected))

[<Fact>]
let ``All examples round trip without changing configurations or identities`` () =
    for kind in RunTypeCatalog.all do
        for example in kind.Examples do
            let original = example.Run
            match RunTypeEditor.build original (RunTypeEditor.fromRun original) with
            | Error errors -> failwithf "%s.%s: %A" kind.Name example.Name (errors |> Array.map (fun e -> e.Field, e.Message))
            | Ok rebuilt -> Assert.Equal<run>(original, rebuilt)

[<Fact>]
let ``Soss editor exposes activator sweep and uses module selections`` () =
    let example = SorterSgd.p32.Msuf4a.SimpleS.Soss.runType.Examples |> Array.find (fun e -> e.Name = "SossS")
    let original = example.Run
    let draft = RunTypeEditor.newDraft original
    Assert.Contains(RunTypeEditor.parameters original, fun p -> p.Key = ParamKeys.activatorMutationRateKey)
    let configured = RunTypeEditor.build original draft |> function Ok value -> value | Error _ -> failwith "Invalid draft"
    Assert.Equal(SorterSgd.p32.Msuf4a.SimpleS.Soss.executionFunctionName, configured.ExecuteFunctionName)
    Assert.Equal(SorterSgd.p32.Msuf4a.SimpleS.Soss.executionQueryCatalogName, configured.RunQueryCatalogName)
    Assert.Equal(SorterSgd.p32.Msuf4a.SimpleS.Soss.executionEnhancerFunctionName, configured.RunEnhancerFunctionName)
    Assert.Equal(SorterSgd.p32.Msuf4a.SimpleS.Soss.setupQueryCatalogName, configured.SetupQueryCatalogName)
    Assert.Equal(SorterSgd.p32.Msuf4a.SimpleS.Soss.setupEnhancerFunctionName, configured.SetupEnhancerFunctionName)
    Assert.Equal<(string * string list) list>(original.RunParameterSpans, configured.RunParameterSpans)

[<Fact>]
let ``Editor validates required spans probabilities and continuation identity`` () =
    let original = SorterSgd.p32.Msuf4a.SimpleS.Soss.ActivatorRates ()
    let args = (RunTypeEditor.fromRun original).ToArgs()
    let missing = runDraft.create { args with SetupParameters = [||] }
    Assert.True(RunTypeEditor.build original missing |> Result.isError)
    let badRows = args.ExecutionParameters |> Array.map (fun row ->
        if row.Key = ParamKeys.activatorMutationRateKey then parameterDraft.create row.Key [|"1.1"|] else row)
    let badRate = runDraft.create { args with ExecutionParameters = badRows }
    Assert.True(RunTypeEditor.build original badRate |> Result.isError)
    let continuation = SorterSgd.p32.Msuf4a.Standard_append.Test ()
    let args = (RunTypeEditor.fromRun continuation).ToArgs()
    let changedRows = args.ExecutionParameters |> Array.map (fun row ->
        if row.Key = ParamKeys.sourceRunNameKey then parameterDraft.create row.Key [|"OtherRun"|] else row)
    let changedSource = runDraft.create { args with ExecutionParameters = changedRows }
    Assert.True(RunTypeEditor.build continuation changedSource |> Result.isError)

[<Fact>]
let ``Merge choices come from available networks and preserve all preset values`` () =
    for kind in RunTypeCatalog.all do
        for example in kind.Examples do
            let specs = RunTypeEditor.parameters example.Run
            for key, values in example.Run.SetupParameterSpans @ example.Run.RunParameterSpans do
                if key = ParamKeys.mergeLibIdKey then
                    let spec = specs |> Array.find (fun spec -> spec.Key = key)
                    Assert.Equal(parameterValueKind.Choice, spec.ValueKind)
                    for value in values do Assert.Contains(value, spec.Choices)
                    for choice in spec.Choices do
                        Assert.True(ParameterValidation.validateValue key choice |> Result.isOk)
                        let id = MergeLibId.fromString choice
                        let declared = example.Run.SetupParameterSpans @ example.Run.RunParameterSpans |> List.filter (fst >> (=) key) |> List.collect snd
                        Assert.True(List.contains choice declared || (SorterLib.tryGet (sorterLibId.create id.FactorChannelCount id.SorterLibVariant) |> Option.isSome))

[<Fact>]
let ``Equivalent merge identities and numeric values cannot create duplicate sweep entries`` () =
    let value = "MergeLibId(channelCount=32, mergeDimension=2, Variant=VariantA)"
    let equivalent = "MergeLibId(channelCount=032, mergeDimension=02, Variant=VariantA)"
    for key, values in [ParamKeys.mergeLibIdKey, [value; equivalent]
                        ParamKeys.activatorMutationRateKey, ["0.01"; "0.010"]
                        ParamKeys.codeModKey, ["same"; "same"]] do
        let errors = ParameterValidation.validateSpans "Setup" [key, values]
        Assert.Contains(errors, fun error -> error.Message.Contains("duplicated"))
    for invalid in ["not an ID"; "MergeLibId(channelCount=32, mergeDimension=0, Variant=VariantA)"
                    "MergeLibId(channelCount=32, mergeDimension=3, Variant=VariantA)"
                    "MergeLibId(channelCount=32, mergeDimension=16, Variant=Unknown)"] do
        Assert.True(ParameterValidation.validateValue ParamKeys.mergeLibIdKey invalid |> Result.isError)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Merge width filtering finds component networks for test and evaluation runs`` evaluation =
    let configured = if evaluation then SorterEval.Merge.Test () else SortableTests.Merge.HiMD_32 ()
    let before = RunTypeEditor.fromRun configured
    Assert.Contains(24, RunTypeEditor.mergeChannelCounts configured)
    Assert.Contains(32, RunTypeEditor.mergeChannelCounts configured)
    for width in [24; 32; 64; 96; 37] do
        let spec = RunTypeEditor.parametersForMergeChannelCount configured width |> Array.find (fun spec -> spec.Key = ParamKeys.mergeLibIdKey)
        let expected = MergeLibId.availableForChannelCount (FSharp.UMX.UMX.tag width) |> Array.map MergeLibId.toString
        Assert.Equal<string>(expected |> Array.sort, spec.Choices |> Array.sort)
        Assert.Equal(evaluation, spec.RequiredInSetup)
        Assert.Equal(not evaluation, spec.ExecutionAllowed)
        Assert.DoesNotContain(spec.Choices, fun value -> int (MergeLibId.fromString value).ChannelCount <> width)
    Assert.Equal(before, RunTypeEditor.fromRun configured)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Merge width suggestions and filtering preserve unavailable saved library identities`` evaluation =
    let original = if evaluation then SorterEval.Merge.Test () else SortableTests.Merge.HiMD_32 ()
    let saved = mergeLibId.create 768<channelCount> 2<mergeDimension> sorterLibVariant.VariantA |> MergeLibId.toString
    let replace spans = spans |> List.map (fun (key, values) -> if key = ParamKeys.mergeLibIdKey then key, [saved] else key, values)
    let configured = Run.withParameterSpans (replace original.SetupParameterSpans) (replace original.RunParameterSpans) original
    Assert.Contains(768, RunTypeEditor.mergeChannelCounts configured)
    let choices width = RunTypeEditor.parametersForMergeChannelCount configured width |> Array.find (fun spec -> spec.Key = ParamKeys.mergeLibIdKey) |> fun spec -> spec.Choices
    Assert.Contains(saved, choices 768)
    Assert.DoesNotContain(saved, choices 24)

[<Fact>]
let ``Prefix choices include the declared IDs and library entries at the same widths`` () =
    let configured = SortableTests.Prefix.Channels24 ()
    let spec = RunTypeEditor.parameters configured |> Array.find (fun spec -> spec.Key = ParamKeys.prefixLibIdKey)
    Assert.Equal(parameterValueKind.Choice, spec.ValueKind)
    let expected = PrefixLib.ids () |> Array.filter (fun id -> int id.ChannelCount = 24) |> Array.map PrefixLibId.toString
    Assert.Equal<string>(expected |> Array.sort, spec.Choices |> Array.sort)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Prefix test and evaluation choices can use any registered width without changing the example`` evaluation =
    let configured = if evaluation then SorterEval.Prefix.Channels32 () else SortableTests.Prefix.Channels24 ()
    let before = RunTypeEditor.fromRun configured
    let widths = RunTypeEditor.prefixChannelCounts configured
    Assert.Contains(8, widths)
    Assert.Contains(24, widths)
    Assert.Contains(32, widths)
    for width in widths do
        let spec = RunTypeEditor.parametersForPrefixChannelCount configured width |> Array.find (fun spec -> spec.Key = ParamKeys.prefixLibIdKey)
        let expected = PrefixLib.ids () |> Array.filter (fun id -> int id.ChannelCount = width) |> Array.map PrefixLibId.toString
        Assert.Equal<string>(expected |> Array.sort, spec.Choices |> Array.sort)
        Assert.DoesNotContain(spec.Choices, fun value -> int (PrefixLibId.fromString value).ChannelCount <> width)
        Assert.Equal(evaluation, spec.RequiredInSetup)
        Assert.Equal(not evaluation, spec.ExecutionAllowed)
    Assert.Equal(before, RunTypeEditor.fromRun configured)

[<Fact>]
let ``Prefix evaluation width choices preserve saved Setup declarations outside the current library`` () =
    let original = SorterEval.Prefix.Channels32 ()
    let savedPrefix = prefixLibId.create 12<channelCount> 3<stageLength> prefixLibVariant.PrefixC |> PrefixLibId.toString
    let setup = original.SetupParameterSpans |> List.map (fun (key, values) -> if key = ParamKeys.prefixLibIdKey then key, [savedPrefix] else key, values)
    let configured = Run.withParameterSpans setup original.RunParameterSpans original
    Assert.Contains(12, RunTypeEditor.prefixChannelCounts configured)
    let spec = RunTypeEditor.parametersForPrefixChannelCount configured 12 |> Array.find (fun spec -> spec.Key = ParamKeys.prefixLibIdKey)
    Assert.Equal<string>([|savedPrefix|], spec.Choices)

[<Fact>]
let ``Sorter choices preserve saved IDs and offer other registered variants at that width`` () =
    let original = SortableTests.Prefix.Channels32 ()
    let savedIds = ["SorterLibId(channelCount=32, Variant=VariantA)"; "SorterLibId(channelCount=32, Variant=VariantC)"]
    let configured =
        SimpleRun.create {
            DatabaseName = original.DatabaseName; ProjectName = original.ProjectName; RunName = original.RunName
            Description = original.Description; AllowOverwrite = false
            Setup = { FunctionName = Run.requireSetupFunctionName original; QueryCatalogName = original.SetupQueryCatalogName
                      EnhancerFunctionName = original.SetupEnhancerFunctionName; ParameterSpans = [ParamKeys.sorterLibIdKey, savedIds] }
            Execution = { FunctionName = original.ExecuteFunctionName; QueryCatalogName = original.RunQueryCatalogName
                          EnhancerFunctionName = original.RunEnhancerFunctionName; ParameterSpans = [] }
        } |> Run.ofSimple
    let spec = RunTypeEditor.parameters configured |> Array.find (fun spec -> spec.Key = ParamKeys.sorterLibIdKey)
    Assert.Equal(parameterValueKind.Choice, spec.ValueKind)
    for value in savedIds do Assert.Contains(value, spec.Choices)
    Assert.Contains("SorterLibId(channelCount=32, Variant=VariantB)", spec.Choices)
    Assert.DoesNotContain(spec.Choices, fun value -> int (SorterLibId.fromString value).ChannelCount <> 32)
    match RunTypeEditor.build configured (RunTypeEditor.fromRun configured) with
    | Ok rebuilt -> Assert.Equal<run>(configured, rebuilt)
    | Error _ -> failwith "Saved sorter IDs must survive editing."

[<Fact>]
let ``Prefix and sorter duplicates are compared by parsed identity`` () =
    for key, value, equivalent, invalid in [
        ParamKeys.prefixLibIdKey, "PrefixLibId(channelCount=24, stageLength=4, Variant=PrefixA)",
            "PrefixLibId(channelCount=024, stageLength=04, Variant=PrefixA)", "PrefixLibId(channelCount=24, stageLength=0, Variant=PrefixA)"
        ParamKeys.sorterLibIdKey, "SorterLibId(channelCount=32, Variant=VariantA)",
            "SorterLibId(channelCount=032, Variant=VariantA)", "SorterLibId(channelCount=0, Variant=VariantA)"
    ] do
        Assert.True(ParameterValidation.validateValue key value |> Result.isOk)
        Assert.Contains(ParameterValidation.validateSpans "Setup" [key, [value; equivalent]], fun issue -> issue.Message.Contains("duplicated"))
        Assert.True(ParameterValidation.validateValue key invalid |> Result.isError)
        Assert.True(ParameterValidation.validateValue key "not an ID" |> Result.isError)
