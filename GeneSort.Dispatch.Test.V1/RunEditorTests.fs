namespace GeneSort.Dispatch.Test.V1

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Standard
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Workflows
open GeneSort.Project.Mp.V1

module RunEditorTests =
    let private ok = function Ok value -> value | Error error -> failwithf "%A" error
    let private change update = RunEditor.newDraft().ToArgs() |> update |> runDraft.create
    let private replace key values (rows: parameterDraft[]) =
        rows |> Array.map (fun row -> if row.Key = key then parameterDraft.create key values else row)

    [<Fact>]
    let ``default editor definition generates real Standard parameters`` () =
        let configured = RunEditor.newDraft() |> RunEditor.build |> ok
        let parameters = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
        Assert.Equal(4, parameters.Length)
        Assert.Equal<int>([|0; 1; 2; 3|], parameters |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Array.sort)
        for rp in parameters do
            Assert.Equal(1, int (rp.GetSorterPoolCount().Value))
            Assert.Equal(32, int (rp.GetSorterCountPerPool().Value))
            Assert.True(rp.GetId().IsSome)

    [<Fact>]
    let ``catalog selections resolve to registered implementations`` () =
        ProjectCatalogs.registerAll ()
        let configured = RunEditor.newDraft() |> RunEditor.build |> ok
        Assert.Equal(Some GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.sgdStandard, configured.SetupFunctionName)
        SetupEnhancerCatalog.get configured.SetupEnhancerFunctionName |> ignore
        QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName |> ignore
        QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName |> ignore
        RunParamEnhancerBuilders.get configured.RunEnhancerFunctionName |> ignore
        ExecuteFunctionCatalog.get configured.ExecuteFunctionName |> ignore
        SortableTestsMakerCatalog.get configured.MakerNames.Value.SortableTests |> ignore
        SeedSorterPoolSetMakerCatalog.get configured.MakerNames.Value.SeedPoolSet |> ignore
        MutatorMakerCatalog.get configured.MakerNames.Value.Mutator |> ignore
        Assert.All(RunEditor.catalogChoices(), fun choice -> Assert.False(String.IsNullOrWhiteSpace choice.Description))

    [<Fact>]
    let ``typed editors expose domain bounds and choices`` () =
        let spec key = RunEditor.parameters() |> Array.find (fun parameter -> parameter.Key = key)
        let count = spec ParamKeys.sorterCountPerPoolKey
        Assert.Equal(parameterValueKind.Integer, count.ValueKind)
        Assert.Equal(Some 1.0, count.Minimum)
        Assert.Equal(Some (float Int32.MaxValue), count.Maximum)
        let probability = spec ParamKeys.modificationRateKey
        Assert.Equal(parameterValueKind.Number, probability.ValueKind)
        Assert.Equal(Some 0.0, probability.Minimum)
        Assert.Equal(Some 1.0, probability.Maximum)
        let weight = spec ParamKeys.paraRateKey
        Assert.Equal(parameterValueKind.Number, weight.ValueKind)
        Assert.Equal(None, weight.Maximum)
        Assert.True(ParameterValidation.validateValue weight.Key "1.75" |> Result.isOk)
        let variant = spec ParamKeys.mutatorVariantKey
        Assert.Equal(parameterValueKind.Choice, variant.ValueKind)
        Assert.All(variant.Choices, fun choice -> Assert.True(ParameterValidation.validateValue variant.Key choice |> Result.isOk))
        Assert.Equal(parameterValueKind.Text, (spec ParamKeys.codeModKey).ValueKind)

    [<Fact>]
    let ``code presets round trip without changing automatic names or parameter phases`` () =
        let presets = [
            GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.Standard.Pool_32.Test()
            GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.Standard.Pool_32.V1()
        ]
        for configured in presets do
            let roundTrip = configured |> RunEditor.fromRun |> ok |> RunEditor.build |> ok
            Assert.True(RunDto.fromDomain configured = RunDto.fromDomain roundTrip)

    [<Fact>]
    let ``execution overrides survive editing and apply during parameter generation`` () =
        let draft = change (fun args ->
            { args with
                ExecutionParameters = [|
                    parameterDraft.create ParamKeys.modificationRateKey [|"0.05"; "0.1"|]
                    parameterDraft.create ParamKeys.generationIntervalLastKey [|"0"|]
                |] })
        let configured = RunEditor.build draft |> ok
        let reopened = configured |> RunEditor.fromRun |> ok |> RunEditor.build |> ok
        Assert.True(RunDto.fromDomain configured = RunDto.fromDomain reopened)
        let parameters = RunParameterGeneration.generate reopened 0<replNumber> 1<replNumber>
        Assert.Equal(8, parameters.Length)
        Assert.Equal<float>([|0.05; 0.1|], parameters |> Array.map (fun rp -> float (rp.GetModificationRate().Value)) |> Array.distinct |> Array.sort)
        Assert.All(parameters, fun rp -> Assert.Equal(0, int (rp.GetGenerationIntervalLast().Value)))

    [<Theory>]
    [<InlineData("SorterCountPerPool", "0")>]
    [<InlineData("SorterCountPerPoolSet", "33")>]
    [<InlineData("GenerationIntervalLast", "-1")>]
    [<InlineData("AdjustCeLengthFrequency", "0")>]
    [<InlineData("ParaRate", "NaN")>]
    [<InlineData("SelfSym", "Infinity")>]
    [<InlineData("ModificationRate", "1.1")>]
    [<InlineData("SeedModificationRate", "-0.1")>]
    [<InlineData("MutatorVariantKey", "V9")>]
    [<InlineData("MutationMod", "not a number")>]
    let ``invalid values produce field errors instead of throwing`` key value =
        let draft = change (fun args -> { args with SetupParameters = replace key [|value|] args.SetupParameters })
        let issues = RunEditor.validate draft
        Assert.Contains(issues, fun issue -> issue.Field.Contains key)
        Assert.True(RunEditor.build draft |> Result.isError)

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("../escape")>]
    [<InlineData("..")>]
    [<InlineData("C:\\escape")>]
    [<InlineData("name.")>]
    [<InlineData(" name")>]
    [<InlineData("CON")>]
    [<InlineData("LPT1.txt")>]
    let ``unsafe identities are rejected`` name =
        let issues = change (fun args -> { args with RunName = name }) |> RunEditor.validate
        Assert.Contains(issues, fun issue -> issue.Field = "RunName")

    [<Fact>]
    let ``duplicates empty spans unknown keys and missing setup requirements are reported`` () =
        let draft = change (fun args ->
            { args with
                SetupParameters = [|
                    parameterDraft.create ParamKeys.sorterCountPerPoolKey [|"32"; "032"|]
                    parameterDraft.create ParamKeys.sorterCountPerPoolKey [||]
                    parameterDraft.create "Unknown" [|"1"|]
                |] })
        let messages = RunEditor.validate draft |> Array.map (fun issue -> issue.Message) |> String.concat "\n"
        for text in ["duplicated"; "more than once"; "at least one"; "not supported"; "required"] do
            Assert.Contains(text, messages)

    [<Fact>]
    let ``pool divisibility is checked across every setup combination`` () =
        let draft = change (fun args ->
            { args with
                SetupParameters =
                    args.SetupParameters |> replace ParamKeys.sorterCountPerPoolKey [|"32"; "64"|]
                                         |> replace ParamKeys.sorterCountPerPoolSetKey [|"32"; "128"|] })
        Assert.Contains(RunEditor.validate draft, fun issue -> issue.Message.Contains "Total 32 is not divisible by per-pool count 64")

    [<Fact>]
    let ``execution cannot replace setup pool sizes or supply missing setup query fields`` () =
        let draft = change (fun args ->
            { args with
                SetupParameters = args.SetupParameters |> Array.filter (fun row -> row.Key <> ParamKeys.paraRateKey)
                ExecutionParameters = [| parameterDraft.create ParamKeys.paraRateKey [|"1.15"|]; parameterDraft.create ParamKeys.sorterCountPerPoolKey [|"64"|] |] })
        Assert.Contains(RunEditor.validate draft, fun issue -> issue.Field = "Setup.ParaRate")
        Assert.Contains(RunEditor.validate draft, fun issue -> issue.Message = "Set pool sizes during setup.")

    [<Fact>]
    let ``missing schedules and out of range generation indices are rejected`` () =
        let invalid = change (fun args -> { args with GenSaveIntervalsName = "missing" })
        Assert.Contains(RunEditor.validate invalid, fun issue -> issue.Field = "GenSaveIntervalsName")
        let empty = change (fun args -> { args with GenSaveIntervalsName = GenIntervalNames.emptyInterval })
        Assert.Contains(RunEditor.validate empty, fun issue -> issue.Field = ParamKeys.generationIntervalLastKey)

    [<Fact>]
    let ``continuations and incompatible catalogs cannot be silently converted`` () =
        let configured = RunEditor.newDraft() |> RunEditor.build |> ok
        let continuation = configured |> Run.withParameterSource configured []
        Assert.True(RunEditor.fromRun continuation |> Result.isError)
        let incompatible = { RunDto.fromDomain configured with SetupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32R } |> RunDto.toDomain
        Assert.True(RunEditor.fromRun incompatible |> Result.isError)

    [<Fact>]
    let ``editable argument arrays cannot mutate a draft snapshot`` () =
        let args = RunEditor.newDraft().ToArgs()
        let original = args.SetupParameters[0]
        let draft = runDraft.create args
        args.SetupParameters[0] <- parameterDraft.create "Changed" [||]
        draft.SetupParameters[0] <- parameterDraft.create "ChangedAgain" [||]
        Assert.Equal(original.Key, draft.SetupParameters[0].Key)

    [<Fact>]
    let ``one workflow definition supplies the displayed saved and recognized catalog selections`` () =
        let workflow = WorkflowDefinitions.standardMsuf32
        let configured = RunEditor.newDraft() |> RunEditor.build |> ok
        let dto = RunDto.fromDomain configured
        let selected = [| dto.SetupFunctionName; dto.SetupQueryCatalogName; dto.SetupEnhancerFunctionName
                          dto.ExecuteFunctionName; dto.RunQueryCatalogName; dto.RunEnhancerFunctionName
                          dto.SortableTestsMakerName; dto.SeedPoolSetMakerName; dto.MutatorMakerName |]
        Assert.Equal<string>(selected, RunEditor.catalogChoices() |> Array.map (fun choice -> choice.Name))
        Assert.True(workflow.Matches configured)
        let mismatches = [
            { dto with SetupFunctionName = "another-setup" }
            { dto with SetupQueryCatalogName = "another-query" }
            { dto with SetupEnhancerFunctionName = "another-enhancer" }
            { dto with ExecuteFunctionName = "another-executor" }
            { dto with RunQueryCatalogName = "another-query" }
            { dto with RunEnhancerFunctionName = "another-enhancer" }
            { dto with SortableTestsMakerName = "another-tests-maker" }
            { dto with SeedPoolSetMakerName = "another-seed-maker" }
            { dto with MutatorMakerName = "another-mutator" }
        ]
        for mismatch in mismatches do
            let other = RunDto.toDomain mismatch
            Assert.False(workflow.Matches other)
            Assert.True(RunEditor.fromRun other |> Result.isError)

    [<Theory>]
    [<InlineData("SorterCountPerPool", "0")>]
    [<InlineData("SorterCountPerPoolSet", "33")>]
    [<InlineData("ParaRate", "NaN")>]
    [<InlineData("SelfSym", "Infinity")>]
    [<InlineData("SeedModificationRate", "-0.01")>]
    [<InlineData("ModificationRate", "1.01")>]
    [<InlineData("MutatorVariantKey", "V9")>]
    let ``editor and parameter generation report the same invalid setup values`` key value =
        let draft = change (fun args -> { args with SetupParameters = replace key [|value|] args.SetupParameters })
        let issue = RunEditor.validate draft |> Array.find (fun issue -> issue.Field.Contains key)
        let original = RunEditor.newDraft() |> RunEditor.build |> ok |> RunDto.fromDomain
        let configured =
            { original with
                SetupParameterSpans = original.SetupParameterSpans |> Array.map (fun row ->
                    if row.Key = key then { row with Values = [|value|] } else row) } |> RunDto.toDomain
        let error = Assert.Throws<ArgumentException>(fun () -> RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> ignore)
        Assert.Contains(issue.Message, error.Message)

    [<Fact>]
    let ``missing setup requirements are shared even when execution supplies a value`` () =
        let original = RunEditor.newDraft() |> RunEditor.build |> ok |> RunDto.fromDomain
        let configured =
            { original with
                SetupParameterSpans = original.SetupParameterSpans |> Array.filter (fun row -> row.Key <> ParamKeys.paraRateKey)
                RunParameterSpans = [| { Key = ParamKeys.paraRateKey; Values = [|"1.15"|] } |]
            } |> RunDto.toDomain
        let issue = configured |> RunEditor.fromRun |> ok |> RunEditor.validate |> Array.find (fun issue -> issue.Field = "Setup.ParaRate")
        let error = Assert.Throws<ArgumentException>(fun () -> RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> ignore)
        Assert.Contains(issue.Field + ": " + issue.Message, error.Message)

    [<Fact>]
    let ``runtime empty products remain valid while the editor requires values`` () =
        let original = RunEditor.newDraft() |> RunEditor.build |> ok |> RunDto.fromDomain
        let spans = [| { Key = ParamKeys.modificationRateKey; Values = [||] } |]
        let configured = { original with RunParameterSpans = spans } |> RunDto.toDomain
        let issues = configured |> RunEditor.fromRun |> ok |> RunEditor.validate
        Assert.Contains(issues, fun issue -> issue.Message = "Enter at least one value.")
        Assert.Empty(RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>)

    [<Fact>]
    let ``fresh runs reject fixed pool overrides in both editor and runtime`` () =
        let original = RunEditor.newDraft() |> RunEditor.build |> ok |> RunDto.fromDomain
        let configured =
            { original with
                RunParameterSpans = [|
                    { Key = ParamKeys.sorterCountPerPoolKey; Values = [|"64"|] }
                    { Key = ParamKeys.sorterCountPerPoolSetKey; Values = [|"64"|] }
                    { Key = ParamKeys.sorterPoolExpansionRateKey; Values = [|"1.1"|] }
                |] } |> RunDto.toDomain
        let issues = configured |> RunEditor.fromRun |> ok |> RunEditor.validate
        Assert.Contains(issues, fun issue -> issue.Message = "Set pool sizes during setup.")
        Assert.Contains(issues, fun issue -> issue.Message.Contains "not supported")
        let error = Assert.Throws<ArgumentException>(fun () -> RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> ignore)
        Assert.Contains("fixed by Setup", error.Message)
        let growthOnly = { original with RunParameterSpans = [| { Key = ParamKeys.sorterPoolExpansionRateKey; Values = [|"1.1"|] } |] } |> RunDto.toDomain
        Assert.All(RunParameterGeneration.generate growthOnly 0<replNumber> 1<replNumber>, fun rp ->
            Assert.Equal("1.1", rp.ParamMap[ParamKeys.sorterPoolExpansionRateKey]))

    [<Fact>]
    let ``Soss seed setup does not require Standard mutation settings`` () =
        let configured = RunEditor.newDraft() |> RunEditor.build |> ok
        let context = { Run = configured; QueryParamsFromSetupParams = fun _ _ -> failwith "Seed validation must not query files." }
        let parameters = setupParameters.create (Map.ofList [ParamKeys.sorterCountPerPoolKey, "32"; ParamKeys.sorterCountPerPoolSetKey, "64"])
        let enhanced = SetupEnhancerCatalog.sossMsuf32 context parameters
        Assert.Equal(2, int (enhanced.GetSorterPoolCount().Value))

    [<Theory>]
    [<InlineData("AdjustCeLengthFrequency", "0")>]
    [<InlineData("GenerationIntervalLast", "5")>]
    let ``Standard execution and editor share evolution bounds before accessing storage`` key value =
        let draft = change (fun args ->
            { args with
                GenSaveIntervalsName = GenIntervalNames.uniformInterval5_L5
                SetupParameters = replace key [|value|] args.SetupParameters })
        let issue = RunEditor.validate draft |> Array.find (fun issue -> issue.Field.Contains key)
        let configured = RunEditor.newDraft() |> RunEditor.build |> ok
        let original = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> Array.head
        let parameters = runParameters.create (original.ParamMap.Add(key, value))
        let path, progress = EvoPath.fromParameters configured parameters
        let intervals = GenIntervalRegistry.genIntervalConfigsDict[GenIntervalNames.uniformInterval5_L5]
        // Invalid settings must be rejected before any storage, pool or mutator is accessed.
        let result =
            EvoOrch_Standard.runStandardEvolutionAsync Unchecked.defaultof<_> intervals intervals path progress (UMX.tag false)
                Unchecked.defaultof<_> Unchecked.defaultof<_> Unchecked.defaultof<_> Unchecked.defaultof<_>
                Threading.CancellationToken.None ignore |> Async.RunSynchronously
        match result with
        | Error message -> Assert.Contains(issue.Message, message)
        | Ok _ -> failwith "Invalid evolution settings were accepted."
