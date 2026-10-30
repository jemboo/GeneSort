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
open GeneSort.Project.V1.ParamBundles
open GeneSort.Model.Sorting.V1

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
    let example = SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.runType.Examples |> Array.find (fun e -> e.Name = "SossS")
    let original = example.Run
    let draft = RunTypeEditor.newDraft original
    Assert.Contains(RunTypeEditor.parameters original, fun p -> p.Key = ParamKeys.activatorMutationRateKey)
    let configured = RunTypeEditor.build original draft |> function Ok value -> value | Error _ -> failwith "Invalid draft"
    Assert.Equal(SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.executionFunctionName, configured.ExecuteFunctionName)
    Assert.Equal(SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.executionQueryCatalogName, configured.RunQueryCatalogName)
    Assert.Equal(SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.runParametersEnhancerName, configured.RunParametersEnhancerName)
    Assert.Equal(SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.setupQueryCatalogName, configured.SetupQueryCatalogName)
    Assert.Equal(SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.setupParametersEnhancerName, configured.SetupParametersEnhancerName)
    Assert.Equal<(string * string list) list>(original.RunParameterSpans, configured.RunParameterSpans)

[<Fact>]
let ``Editor validates required spans probabilities and continuation identity`` () =
    let original = SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Soss.ActivatorRates ()
    let args = (RunTypeEditor.fromRun original).ToArgs()
    let missing = runDraft.create { args with SetupParameters = [||] }
    Assert.True(RunTypeEditor.build original missing |> Result.isError)
    let badRows = args.ExecutionParameters |> Array.map (fun row ->
        if row.Key = ParamKeys.activatorMutationRateKey then parameterDraft.create row.Key [|"1.1"|] else row)
    let badRate = runDraft.create { args with ExecutionParameters = badRows }
    Assert.True(RunTypeEditor.build original badRate |> Result.isError)
    let continuation = SorterSgd.cc32.p4a.Msuf4.Simple.Standard_append.Test ()
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
    let configured = if evaluation then SorterEval.Merge.Channels32.Test () else SortableTests.Merge.Channels32.HiMD_32 ()
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
    let original = if evaluation then SorterEval.Merge.Channels32.Test () else SortableTests.Merge.Channels32.HiMD_32 ()
    let saved = mergeLibId.create 768<channelCount> 2<mergeDimension> sorterLibVariant.VariantA |> MergeLibId.toString
    let replace spans = spans |> List.map (fun (key, values) -> if key = ParamKeys.mergeLibIdKey then key, [saved] else key, values)
    let configured = Run.withParameterSpans (replace original.SetupParameterSpans) (replace original.RunParameterSpans) original
    Assert.Contains(768, RunTypeEditor.mergeChannelCounts configured)
    let choices width = RunTypeEditor.parametersForMergeChannelCount configured width |> Array.find (fun spec -> spec.Key = ParamKeys.mergeLibIdKey) |> fun spec -> spec.Choices
    Assert.Contains(saved, choices 768)
    Assert.DoesNotContain(saved, choices 24)

[<Fact>]
let ``Prefix choices include the declared IDs and library entries at the same widths`` () =
    let configured = SortableTests.Prefix.Channels24.Channels24 ()
    let spec = RunTypeEditor.parameters configured |> Array.find (fun spec -> spec.Key = ParamKeys.prefixLibIdKey)
    Assert.Equal(parameterValueKind.Choice, spec.ValueKind)
    let expected = PrefixLib.ids () |> Array.filter (fun id -> int id.ChannelCount = 24) |> Array.map PrefixLibId.toString
    Assert.Equal<string>(expected |> Array.sort, spec.Choices |> Array.sort)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Prefix test and evaluation choices can use any registered width without changing the example`` evaluation =
    let configured = if evaluation then SorterEval.Prefix.Channels32 () else SortableTests.Prefix.Channels24.Channels24 ()
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
    let original = SortableTests.Prefix.Channels32.FourStage_32 ()
    let savedIds = ["SorterLibId(channelCount=32, Variant=VariantA)"; "SorterLibId(channelCount=32, Variant=VariantC)"]
    let configured =
        SimpleRun.create {
            DatabaseName = original.DatabaseName; ProjectName = original.ProjectName; RunName = original.RunName
            Description = original.Description; AllowOverwrite = false
            Setup = { FunctionName = Run.requireSetupFunctionName original; QueryCatalogName = original.SetupQueryCatalogName
                      ParametersEnhancerName = original.SetupParametersEnhancerName; ParameterSpans = [ParamKeys.sorterLibIdKey, savedIds] }
            Execution = { FunctionName = original.ExecuteFunctionName; QueryCatalogName = original.RunQueryCatalogName
                          ParametersEnhancerName = original.RunParametersEnhancerName; ParameterSpans = [] }
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


[<Fact>]
let ``Custom drafts preserve module settings and allow only source phase parameters`` () =
    for kind in RunTypeCatalog.all do
        for example in kind.Examples do
            let source = example.Run
            let draft = RunTypeEditor.newDraft source
            Assert.Equal("NewRun", draft.RunName)
            let custom = RunTypeEditor.buildModuleRun source draft |> function Ok value -> value | Error errors -> failwithf "%s: %A" example.Name errors
            Assert.True(source.ProjectName = custom.ProjectName, "Draft changed ProjectName.")
            Assert.True(source.DatabaseName = custom.DatabaseName, "Draft changed DatabaseName.")
            Assert.True(source.SetupParameterSpans = custom.SetupParameterSpans, "Draft changed SetupParameterSpans.")
            Assert.True(source.RunParameterSpans = custom.RunParameterSpans, "Draft changed RunParameterSpans.")
            Assert.Equal(source.MakerNames, custom.MakerNames)
            Assert.Equal(source.ParameterSource, custom.ParameterSource)
            let args = draft.ToArgs()
            Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with RunName = "RenamedDraft" }) |> Result.isOk)
            let described = RunTypeEditor.buildModuleRun source (runDraft.create { args with Description = args.Description + " changed" })
                            |> Result.defaultWith (fun issues -> failwithf "%A" issues)
            Assert.Equal(args.Description + " changed", described.Description)
            Assert.Equal(source.AllowOverwrite, custom.AllowOverwrite)
            Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with DatabaseName = "OtherDatabase" }) |> Result.isError)
            Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with ExecutionParameters = Array.append args.ExecutionParameters [| parameterDraft.create "NotDeclaredBySource" [|"1"|] |] }) |> Result.isError)
            for row in args.SetupParameters do
                if not (args.ExecutionParameters |> Array.exists (fun other -> other.Key = row.Key)) then
                    Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with ExecutionParameters = Array.append args.ExecutionParameters [|row|] }) |> Result.isError)


[<Fact>]
let ``Prefix presets resolve to valid networks with the declared stage count`` () =
    for example in RunTypeCatalog.all |> Array.filter (fun kind -> kind.Name.StartsWith("SortableTests.Prefix.Channels")) |> Array.collect (fun kind -> kind.Examples) do
        for key, values in example.Run.RunParameterSpans do
            if key = ParamKeys.prefixLibIdKey then
                for value in values do
                    let id = PrefixLibId.fromString value
                    let network = PrefixLib.tryGet id |> Option.defaultWith (fun () -> failwithf "Missing prefix: %s" value)
                    let stages = SorterDataParse.parseTo2dCeArray network
                    Assert.Equal(int id.StageLength, stages.Length)
                    for stage in stages do
                        let channels = stage |> Array.collect (fun ce -> [|int ce.LowChannel; int ce.HighChannel|])
                        Assert.Equal(channels.Length, (Array.distinct channels).Length)
                        Assert.All(channels, fun channel -> Assert.InRange(channel, 0, int id.ChannelCount - 1))


[<Fact>]
let ``Every module owns one channel count and all its presets agree`` () =
    let mismatches =
        RunTypeCatalog.all |> Array.collect (fun kind ->
            kind.Examples |> Array.choose (fun example ->
                let actual = RunTypeEditor.channelCounts example.Run
                if actual = [|kind.ChannelCount|] then None
                else Some (sprintf "%s.%s: %A" kind.Name example.Name actual)))
    Assert.True(Array.isEmpty mismatches, String.Join("; ", mismatches))
    for kind in RunTypeCatalog.all do
        Assert.True(kind.ChannelCount > 0, kind.Name)
        for example in kind.Examples do
            let configured = example.Run
            let declaredCount = RunParameterGeneration.effectiveSpans configured |> Map.tryFind ParamKeys.channelCountKey
            Assert.True((declaredCount = Some [string kind.ChannelCount]), kind.Name + "." + example.Name + " must declare its module channel count.")
            Assert.Equal<int>([|kind.ChannelCount|], RunTypeEditor.channelCounts configured)
            let draft = RunTypeEditor.fromRun configured
            Assert.Empty(RunTypeEditor.validateChannelCount kind.ChannelCount configured draft)
            Assert.NotEmpty(RunTypeEditor.validateChannelCount (kind.ChannelCount + 1) configured draft)
            for spec in RunTypeEditor.parametersForChannelCount configured kind.ChannelCount do
                for value in spec.Choices do
                    let count =
                        if spec.Key = ParamKeys.prefixLibIdKey then Some (int (PrefixLibId.fromString value).ChannelCount)
                        elif spec.Key = ParamKeys.mergeLibIdKey then Some (int (MergeLibId.fromString value).ChannelCount)
                        elif spec.Key = ParamKeys.sorterLibIdKey then Some (int (SorterLibId.fromString value).ChannelCount)
                        elif spec.Key = ParamKeys.channelCountKey then Some (int value)
                        else None
                    count |> Option.iter (fun actual -> Assert.Equal(kind.ChannelCount, actual))
                    if spec.Key = ParamKeys.simpleSorterModelTypeKey then
                        Assert.True(SimpleSorterModelType.supportsChannelCount kind.ChannelCount (SimpleSorterModelType.fromString value))

[<Fact>]
let ``Module validation rejects channel changes encoded in library and mutator parameters`` () =
    let source = SortableTests.Prefix.Channels24.Channels24 ()
    let args = (RunTypeEditor.newDraft source).ToArgs()
    let changed = args.ExecutionParameters |> Array.map (fun row ->
        if row.Key = ParamKeys.prefixLibIdKey then parameterDraft.create row.Key [|PrefixLibId.toString (prefixLibId.create 32<channelCount> 4<stageLength> prefixLibVariant.PrefixA)|]
        else row)
    Assert.NotEmpty(RunTypeEditor.validateChannelCount 24 source (runDraft.create { args with ExecutionParameters = changed }))
    let standard = SorterEval.Standard.Channels16.Test ()
    let args = (RunTypeEditor.newDraft standard).ToArgs()
    let changed = args.SetupParameters |> Array.map (fun row -> if row.Key = ParamKeys.channelCountKey then parameterDraft.create row.Key [|"16"; "24"|] else row)
    Assert.NotEmpty(RunTypeEditor.validateChannelCount 16 standard (runDraft.create { args with SetupParameters = changed }))
    let mutant = GeneSort.Dispatch.V1.SorterMutate.Standard.Msrs ()
    let args = (RunTypeEditor.newDraft mutant).ToArgs()
    let changed = args.SetupParameters |> Array.map (fun row ->
        if row.Key = ParamKeys.mutatorParamsKey then
            parameterDraft.create row.Key [|MutatorParams.msrsParamsRL 24<channelCount> rngType.Lcg mutatorVariant.V1 |> MutatorParams.toString|]
        else row)
    Assert.NotEmpty(RunTypeEditor.validateChannelCount 16 mutant (runDraft.create { args with SetupParameters = changed }))

[<Fact>]
let ``Seed enhancer preserves explicit module settings`` () =
    let context : GeneSort.Project.V1.Catalogs.SetupParameters.setupParametersEnhancerContext = {
        Run = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple.Soss.Pool_32.Gens100 ()
        QueryParamsFromSetupParams = fun _ _ -> None
    }
    let parameters = setupParameters.create (Map.ofList [
        ParamKeys.channelCountKey, "16"
        ParamKeys.sorterCountPerPoolKey, "4"
        ParamKeys.sorterPoolCountKey, "1"
        ParamKeys.prefixLibIdKey, (prefixLibId.create 16<channelCount> 2<stageLength> prefixLibVariant.PrefixA |> PrefixLibId.toString)
        ParamKeys.simpleSorterModelTypeKey, "Msrs"
        ParamKeys.rngTypeKey, "LCG"
        ParamKeys.sortableDataFormatKey, "BitVector512"
        ParamKeys.sorterEvalMeasureKey, "explicit-evaluation-marker"
        ParamKeys.sorterEvalMeasureInitialKey, "explicit-initial-marker"
        ParamKeys.excludeSelfCeKey, "false"
        ParamKeys.collectSortableTestsKey, "true"
    ])
    let enhanced = GeneSort.Project.V1.Catalogs.SetupParameters.SetupParametersEnhancers.sossMsuf32 context parameters
    Assert.Equal(16, int (enhanced.GetChannelCount().Value))
    Assert.Equal(16, int (enhanced.GetPrefixLibId().Value.ChannelCount))
    Assert.Equal(2, int (enhanced.GetPrefixLibId().Value.StageLength))
    for key in [ParamKeys.excludeSelfCeKey; ParamKeys.collectSortableTestsKey; ParamKeys.prefixLibIdKey; ParamKeys.simpleSorterModelTypeKey; ParamKeys.rngTypeKey;
                ParamKeys.sortableDataFormatKey; ParamKeys.sorterEvalMeasureKey; ParamKeys.sorterEvalMeasureInitialKey] do
        Assert.Equal(parameters.ParamMap[key], enhanced.ParamMap[key])

[<Fact>]
let ``Evolution drafts cannot change or remove module owned settings`` () =
    let source = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple.Standard.TestStandard ()
    let draft = RunTypeEditor.newDraft source
    let args = draft.ToArgs()
    for row in args.SetupParameters |> Array.filter (fun row -> RunTypeEditor.isModuleOwnedParameter source row.Key) do
        let changed = args.SetupParameters |> Array.map (fun other -> if other.Key = row.Key then parameterDraft.create row.Key [|"changed"|] else other)
        Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with SetupParameters = changed }) |> Result.isError)
        let removed = args.SetupParameters |> Array.filter (fun other -> other.Key <> row.Key)
        Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with SetupParameters = removed }) |> Result.isError)

[<Fact>]
let ``Activation settings are explicit editable draft parameters and affect setup identity`` () =
    for source, key, defaultValue, changedValue in [
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleR.Standard.Test (), ParamKeys.aMaxKey, "3", "7"
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS.SimpleS.Standard.Test (), ParamKeys.activationProbabilityKey, "0.5", "0.25"
    ] do
        let args = (RunTypeEditor.newDraft source).ToArgs()
        let original = args.SetupParameters |> Array.find (fun row -> row.Key = key)
        Assert.Equal<string>([|defaultValue|], original.Values)
        Assert.False(RunTypeEditor.isModuleOwnedParameter source key)
        let changed = args.SetupParameters |> Array.map (fun row -> if row.Key = key then parameterDraft.create key [|changedValue|] else row)
        let custom = RunTypeEditor.buildModuleRun source (runDraft.create { args with SetupParameters = changed }) |> Result.defaultWith (fun issues -> failwithf "%A" issues)
        let before = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
        let after = RunParameterGeneration.generate custom 0<replNumber> 1<replNumber>
        Assert.Equal(before.Length, after.Length)
        for row in after do
            Assert.Equal(changedValue, row.ParamMap[key])
            Assert.DoesNotContain(before, fun prior -> prior.GetSetupSorterPoolSetId() = row.GetSetupSorterPoolSetId())
        let removed = args.SetupParameters |> Array.filter (fun row -> row.Key <> key)
        Assert.True(RunTypeEditor.buildModuleRun source (runDraft.create { args with SetupParameters = removed }) |> Result.isError)

[<Fact>]
let ``Initial measure follows each evaluation sweep choice without a Cartesian product`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple.MutationRate.VarModR_32.Test ()
    let setup = configured.SetupParameterSpans |> List.map (fun (key, values) ->
        key, if key = ParamKeys.sorterEvalMeasureKey then values else List.take 1 values)
    let execution = configured.RunParameterSpans |> List.map (fun (key, values) -> key, List.take 1 values)
    let sample = Run.withParameterSpans setup execution configured
    let rows = SimpleRun.makeSetupParameters 0<replNumber> 1<replNumber> id sample.BaseRun
    Assert.Equal(4, rows.Length)
    for row in rows do Assert.Equal(row.ParamMap[ParamKeys.sorterEvalMeasureKey], row.ParamMap[ParamKeys.sorterEvalMeasureInitialKey])

[<Fact>]
let ``Explicit evolution controls survive enhancement and separate execution identities`` () =
    let source = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR.SimpleR.Standard.Standard ()
    let first spans = spans |> List.map (fun (key, values) -> key, List.take 1 values)
    let replace key values spans = (key, values) :: (spans |> List.filter (fst >> (<>) key))
    let setup = first source.SetupParameterSpans
                |> replace ParamKeys.sorterCountPerPoolKey ["32"; "64"]
                |> replace ParamKeys.sorterPoolCountKey ["4"]
                |> replace ParamKeys.seedSorterPoolSelectionTypeKey ["GuidOrder:16"]
    let execution = first source.RunParameterSpans
                    |> replace ParamKeys.orthoRateKey ["1.25"]
                    |> replace ParamKeys.modificationRateKey ["0.25"]
                    |> replace ParamKeys.activatorMutationRateKey ["0.2"]
                    |> replace ParamKeys.selectedSorterCountPerPoolKey ["All"]
    let generate execution = Run.withParameterSpans setup execution source |> fun run -> RunParameterGeneration.generate run 0<replNumber> 1<replNumber>
    let rows = generate execution
    Assert.Equal(2, rows.Length)
    for row in rows do
        Assert.Equal(int (row.GetSorterCountPerPool().Value), int (row.GetSelectedSorterCountPerPool().Value))
        Assert.Equal(4, int (row.GetSorterPoolCount().Value))
        Assert.Equal(1.25, float (row.GetOrthoRate().Value))
        Assert.Equal(0.25, float (row.GetModificationRate().Value))
        Assert.Equal("0.2", row.ParamMap[ParamKeys.activatorMutationRateKey])
        Assert.Equal("GuidOrder:16", row.ParamMap[ParamKeys.seedSorterPoolSelectionTypeKey])
    let changed = generate (execution |> replace ParamKeys.orthoRateKey ["2.5"] |> replace ParamKeys.selectedSorterCountPerPoolKey ["8"])
    for row in changed do
        Assert.Equal(8, int (row.GetSelectedSorterCountPerPool().Value))
        Assert.DoesNotContain(rows, fun original -> original.GetId() = row.GetId())
