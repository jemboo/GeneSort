module GeneSort.Dispatch.Test.V2.SimpleSRunConfigTests

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
open GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a

let private configuration soss sweep =
    match soss, sweep with
    | false, false -> SimpleS.Standard.Test()
    | false, true -> SimpleS.Standard.ActivatorRates()
    | true, false -> SimpleS.Soss.Test()
    | true, true -> SimpleS.Soss.ActivatorRates()

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Msuf4a SimpleS presets expand distinct executions with boolean models and mutators`` soss sweep =
    let configured = configuration soss sweep
    Assert.Equal("SorterSgd.RunCfg.p32.Msuf4a.SimpleS", %configured.ProjectName)
    Assert.Equal((if soss then "Soss" else "Standard"), %configured.DatabaseName)
    Assert.Equal((if sweep then "ActivatorRates" else "Test"), %configured.RunName)
    let host = runHost.Create configured 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber>
    let executions = host.MakeRunParameters 0<replNumber> 1<replNumber>
    Assert.Equal((if sweep then 12 else 4), setup.Length)
    Assert.Equal((if sweep then 36 else 4), executions.Length)
    Assert.Equal(executions.Length, executions |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let probabilities = setup |> Array.map (fun sp -> sp.GetActivationProbability().Value) |> Set.ofArray
    Assert.Equal<Set<float>>((if sweep then set [0.25; 0.5; 0.75] else set [0.5]), probabilities)
    let rates = executions |> Array.map (fun rp -> rp.GetActivatorMutationRate().Value) |> Set.ofArray
    Assert.Equal<Set<float>>((if sweep then set [0.05; 0.1; 0.2] else set [0.1]), rates)
    let seedIds = setup |> Array.map (fun sp -> (host.QueryParamsFromSetupParams sp (geneSortDataType.SorterPoolSet "")).Value.Id)
    Assert.Equal(setup.Length, seedIds |> Array.distinct |> Array.length)
    for rp in executions do
        Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
        Assert.Equal(Some 32<sorterCountPerPool>, rp.GetSorterCountPerPool())
        Assert.Equal(Some simpleSorterModelType.Msuf4, rp.GetSimpleSorterModelType())
        Assert.Equal(Some (UMX.tag (if sweep then 9 else 0)), rp.GetGenerationIntervalLast())
        let query = host.QueryParamsFromRunParams rp (geneSortDataType.Run configured.RunName) |> Option.get
        Assert.Equal(Some query.Id, rp.GetId())
        let generator =
            SeedSorterModelGeneration.getSimpleUniformSorterModelGen (rp.GetRngType().Value) 32<sortingWidth> simpleSorterModelType.Msuf4 (rp.GetExcludeSelfCe().Value)
            |> SeedSorterModelGeneration.applyParameters rp |> Result.defaultWith failwith
        let parent = SorterModelGen.makeSorterModelFromIndex 0<sortingGenIndex> generator
        let underlying = MutatorMakers.makeSimpleSorterModelMutator rp |> Async.RunSynchronously |> Result.defaultWith failwith
        let mutator = MutatorMakers.applyParameters rp (sorterModelMutator.Simple underlying) |> Result.defaultWith failwith
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
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen (rp.GetRngType().Value) 32<sortingWidth> simpleSorterModelType.Msuf4 (rp.GetExcludeSelfCe().Value)
                |> SeedSorterModelGeneration.applyParameters rp |> Result.defaultWith failwith
            let model = SorterModelGen.makeSorterModelFromIndex 0<sortingGenIndex> generator |> SorterModel.asSimpleSorterModelS
            Assert.All(model.Activators, fun active -> Assert.Equal((probability = 1.0), active))
