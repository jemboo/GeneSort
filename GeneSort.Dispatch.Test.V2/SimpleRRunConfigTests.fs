module GeneSort.Dispatch.Test.V2.SimpleRRunConfigTests

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
    | false, false -> SimpleR.Standard.Test()
    | false, true -> SimpleR.Standard.ActivatorRates()
    | true, false -> SimpleR.Soss.Test()
    | true, true -> SimpleR.Soss.ActivatorRates()

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Msuf4a SimpleR presets expand distinct executions with bounded models and mutators`` soss sweep =
    let configured = configuration soss sweep
    Assert.Equal("SorterSgd.RunCfg.p32.Msuf4a.SimpleR", %configured.ProjectName)
    Assert.Equal((if soss then "Soss" else "Standard"), %configured.DatabaseName)
    Assert.Equal((if sweep then "ActivatorRates" else "Test"), %configured.RunName)
    let host = runHost.Create configured 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber>
    let executions = host.MakeRunParameters 0<replNumber> 1<replNumber>
    Assert.Equal((if sweep then 12 else 4), setup.Length)
    Assert.Equal((if sweep then 36 else 4), executions.Length)
    Assert.Equal(executions.Length, executions |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let bounds = setup |> Array.map (fun sp -> sp.GetAMax().Value) |> Set.ofArray
    Assert.Equal<Set<int>>((if sweep then set [1; 3; 7] else set [3]), bounds)
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
        let path, _ = EvoPath.fromParameters configured rp
        let underlying = MutatorMakers.makeSimpleSorterModelMutator path |> Async.RunSynchronously |> Result.defaultWith failwith
        let mutator = MutatorMakers.applyPath path (sorterModelMutator.Simple underlying) |> Result.defaultWith failwith
        match parent, mutator with
        | sorterModel.SimpleR model, sorterModelMutator.SimpleR mutation ->
            Assert.Equal(rp.GetAMax().Value, model.AMax)
            Assert.Equal(rp.GetActivatorMutationRate().Value, mutation.ActivatorMutationRate)
            let child = mutation.MakeSorterModelFromIndexAndMod model (UMX.tag 0) (UMX.tag 0)
            Assert.Equal(SimpleSorterModel.getLength child.Model, child.Activators.Length)
            Assert.All(child.Activators, fun value -> Assert.InRange(value, -model.AMax, model.AMax))
        | _ -> failwith "Expected SimpleR seed and mutator."

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleR catalog entries reject malformed and conflicting activation parameters`` soss =
    let host = runHost.Create (configuration soss false) 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber> |> Array.head
    let setupContext = { Run = host.Run; QueryParamsFromSetupParams = host.QueryParamsFromSetupParams }
    let enhanceSetup = SetupEnhancerCatalog.get host.Run.SetupEnhancerFunctionName
    for key, value in [ParamKeys.aMaxKey, "-1"; ParamKeys.aMaxKey, "bad"; ParamKeys.activationProbabilityKey, "0.5"] do
        let bad = setupParameters.create (Map.add key value setup.ParamMap)
        Assert.Throws<ArgumentException>(fun () -> enhanceSetup setupContext bad |> ignore) |> ignore
    let parameters = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let context = runParamEnhancerContext.createForSetup host.QueryParamsFromRunParams host.Run setup
    let enhanceRun = RunParamEnhancerBuilders.get host.Run.RunEnhancerFunctionName
    for value in ["-0.1"; "1.1"; "NaN"; "bad"] do
        let bad = runParameters.create (Map.add ParamKeys.activatorMutationRateKey value parameters.ParamMap)
        Assert.Throws<ArgumentException>(fun () -> enhanceRun context bad |> ignore) |> ignore
