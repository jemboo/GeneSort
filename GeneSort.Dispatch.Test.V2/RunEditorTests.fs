namespace GeneSort.Dispatch.Test.V2

open System
open Xunit
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
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
        SetupCatalog.get configured.SetupFunctionName |> ignore
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
    let ``code presets round trip without changing automatic names or parameter phases`` () =
        let presets = [
            GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Standard.Pool_32.Test()
            GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Standard.Pool_32.V1()
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
