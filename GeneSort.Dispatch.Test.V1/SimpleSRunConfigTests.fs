module GeneSort.Dispatch.Test.V1.SimpleSRunConfigTests

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a

let private configuration soss sweep =
    match soss, sweep with
    | false, false -> SimpleS.Standard.Test()
    | false, true -> GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleS.Standard.ActivatorRates32()
    | true, false -> SimpleS.Soss.Test()
    | true, true -> SimpleS.Soss.ActivatorRates()

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Msuf4a SimpleS presets expand distinct executions with boolean models and mutators`` soss sweep =
    let configured = configuration soss sweep
    Assert.Equal((if soss then "SorterSgd.p32.Msuf4a.SimpleS.Soss" else "SorterSgd.p32.Msuf4a.SimpleS.Standard"), %configured.ProjectName)
    Assert.Equal((if soss then "Soss" else "Standard"), %configured.DatabaseName)
    Assert.Equal((if sweep then (if soss then "ActivatorRates" else "ActivatorRates32") else "Test"), %configured.RunName)
    let host = runHost.Create configured 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber>
    let executions = host.MakeRunParameters 0<replNumber> 1<replNumber>
    let spanCount spans = spans |> List.fold (fun count (_, values: string list) -> count * values.Length) 1
    let values key fallback spans = spans |> List.tryFind (fst >> (=) key) |> Option.map snd |> Option.defaultValue fallback
    let setupCount = spanCount configured.SetupParameterSpans
    Assert.Equal(setupCount, setup.Length)
    Assert.Equal(setupCount * spanCount configured.RunParameterSpans, executions.Length)
    Assert.Equal(executions.Length, executions |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let probabilities = setup |> Array.map (fun sp -> sp.GetActivationProbability().Value) |> Set.ofArray
    let expectedProbabilities = configured.SetupParameterSpans |> values ParamKeys.activationProbabilityKey ["0.5"] |> List.map float |> Set.ofList
    Assert.Equal<Set<float>>(expectedProbabilities, probabilities)
    let rates = executions |> Array.map (fun rp -> rp.GetActivatorMutationRate().Value) |> Set.ofArray
    let expectedRates = configured.RunParameterSpans |> values ParamKeys.activatorMutationRateKey ["0.1"] |> List.map float |> Set.ofList
    Assert.Equal<Set<float>>(expectedRates, rates)
    let seedIds = setup |> Array.map (fun sp -> (host.QueryParamsFromSetupParams sp (geneSortDataType.SorterPoolSet "")).Value.Id)
    Assert.Equal(setup.Length, seedIds |> Array.distinct |> Array.length)
    let poolSizes = configured.SetupParameterSpans |> values ParamKeys.sorterCountPerPoolKey [] |> List.map int
    let poolCounts = configured.SetupParameterSpans |> values ParamKeys.sorterPoolCountKey [] |> List.map int
    let lastIntervals = configured.RunParameterSpans |> values ParamKeys.generationIntervalLastKey [] |> List.map int
    for rp in executions do
        Assert.Contains(int (rp.GetSorterCountPerPool().Value), poolSizes)
        Assert.Contains(int (rp.GetSorterPoolCount().Value), poolCounts)
        Assert.False(rp.ParamMap.ContainsKey "SorterCountPerPoolSet")
        Assert.Equal(Some simpleSorterModelType.Msuf4, rp.GetSimpleSorterModelType())
        Assert.Contains(int (rp.GetGenerationIntervalLast().Value), lastIntervals)
        let query = host.QueryParamsFromRunParams rp (geneSortDataType.Run configured.RunName) |> Option.get
        Assert.Equal(Some query.Id, rp.GetId())
        let generator =
            SeedSorterModelGeneration.getSimpleUniformSorterModelGen (rp.GetRngType().Value) 32<channelCount> simpleSorterModelType.Msuf4 (rp.GetExcludeSelfCe().Value)
            |> SeedSorterModelGeneration.applyParameters rp |> Result.defaultWith failwith
        let parent = SorterModelGen.makeSorterModelFromIndex 0<sortingGenIndex> generator
        let path, _ = EvoPath.fromParameters configured rp
        let underlying = MutatorMakers.makeSimpleSorterModelMutator path |> Async.RunSynchronously |> Result.defaultWith failwith
        let mutator = MutatorMakers.applyPath path (sorterModelMutator.Simple underlying) |> Result.defaultWith failwith
        match parent, mutator with
        | sorterModel.SimpleS model, sorterModelMutator.SimpleS mutation ->
            Assert.Equal(rp.GetActivatorMutationRate().Value, mutation.ActivatorMutationRate)
            let child = mutation.MakeSorterModelFromIndexAndMod model (UMX.tag 0) (UMX.tag 0)
            Assert.Equal(SimpleSorterModel.getLength child.Model, child.Activators.Length)
            Assert.Equal(SimpleSorterModel.getLength model.Model, model.Activators.Length)
        | _ -> failwith "Expected SimpleS seed and mutator."

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleS catalog entries reject malformed and conflicting activation parameters`` soss =
    let host = runHost.Create (configuration soss false) 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber> |> Array.head
    let setupContext = { Run = host.Run; QueryParamsFromSetupParams = host.QueryParamsFromSetupParams }
    let enhanceSetup = SetupEnhancerCatalog.get host.Run.SetupEnhancerFunctionName
    for key, value in [ParamKeys.activationProbabilityKey, "-0.1"; ParamKeys.activationProbabilityKey, "1.1";
                          ParamKeys.activationProbabilityKey, "NaN"; ParamKeys.activationProbabilityKey, "Infinity";
                          ParamKeys.activationProbabilityKey, "bad"; ParamKeys.aMaxKey, "3"] do
        let bad = setupParameters.create (Map.add key value setup.ParamMap)
        Assert.Throws<ArgumentException>(fun () -> enhanceSetup setupContext bad |> ignore) |> ignore
    let parameters = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let context = runParamEnhancerContext.createForSetup host.QueryParamsFromRunParams host.Run setup
    let enhanceRun = RunParamEnhancerBuilders.get host.Run.RunEnhancerFunctionName
    for value in ["-0.1"; "1.1"; "NaN"; "Infinity"; "bad"] do
        let bad = runParameters.create (Map.add ParamKeys.activatorMutationRateKey value parameters.ParamMap)
        Assert.Throws<ArgumentException>(fun () -> enhanceRun context bad |> ignore) |> ignore
    for key, value in [ParamKeys.activationProbabilityKey, "-0.1"; ParamKeys.activationProbabilityKey, "NaN"; ParamKeys.aMaxKey, "3"] do
        let bad = runParameters.create (Map.add key value parameters.ParamMap)
        Assert.Throws<ArgumentException>(fun () -> enhanceRun context bad |> ignore) |> ignore

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleS catalog entries accept probability and mutation rate endpoints`` soss =
    let host = runHost.Create (configuration soss false) 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber> |> Array.head
    let parameters = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let setupContext = { Run = host.Run; QueryParamsFromSetupParams = host.QueryParamsFromSetupParams }
    let enhanceSetup = SetupEnhancerCatalog.get host.Run.SetupEnhancerFunctionName
    let enhanceRun = RunParamEnhancerBuilders.get host.Run.RunEnhancerFunctionName
    for probability in [0.0; 1.0] do
        let prepared = enhanceSetup setupContext (setup.WithActivationProbability(Some probability))
        let context = runParamEnhancerContext.createForSetup host.QueryParamsFromRunParams host.Run prepared
        for rate in [0.0; 1.0] do
            let rp = enhanceRun context (parameters.WithActivationProbability(Some probability).WithActivatorMutationRate(Some rate))
            Assert.Equal(Some probability, rp.GetActivationProbability())
            Assert.Equal(Some rate, rp.GetActivatorMutationRate())
            Assert.True(rp.GetAMax().IsNone)
            let generator =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen (rp.GetRngType().Value) 32<channelCount> simpleSorterModelType.Msuf4 (rp.GetExcludeSelfCe().Value)
                |> SeedSorterModelGeneration.applyParameters rp |> Result.defaultWith failwith
            let model = SorterModelGen.makeSorterModelFromIndex 0<sortingGenIndex> generator |> SorterModel.asSimpleSorterModelS
            Assert.All(model.Activators, fun active -> Assert.Equal((probability = 1.0), active))

[<Fact>]
let ``split pool-size presets preserve their original explicit population shapes`` () =
    let cases = [
        GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleS.Standard.ActivatorRates32(), 16, 32
        GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.SimpleS.Standard.ActivatorRates64(), 8, 64
        GeneSort.Dispatch.V1.SorterSgd.p24.Msrs3a.PoolModComp.PoolsSelSzTest_2_vs_256.PoolSz_2(), 256, 2
        GeneSort.Dispatch.V1.SorterSgd.p24.Msrs3a.PoolModComp.PoolsSelSzTest_2_vs_256.PoolSz_256(), 2, 256
    ]
    for configured, poolCount, perPool in cases do
        let spans = configured.SetupParameterSpans |> Map.ofList
        Assert.Equal<string list>([string poolCount], spans[ParamKeys.sorterPoolCountKey])
        Assert.Equal<string list>([string perPool], spans[ParamKeys.sorterCountPerPoolKey])
        Assert.False(spans.ContainsKey "SorterCountPerPoolSet")
        Assert.False(configured.RunParameterSpans |> List.exists (fun (key, _) -> key = "SorterCountPerPoolSet"))
