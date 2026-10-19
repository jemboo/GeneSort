module GeneSort.Dispatch.Test.V2.MakerCatalogTests

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs
open System
open Xunit
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SorterSgd
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Dispatch.V1
open GeneSort.Project.Mp.V1
open GeneSort.Core.Mp
open MessagePack
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Project.V1.Parameters

let private emptyParameters = runParameters.create Map.empty

let private sources : evolutionSetupSources = {
    LoadMergeTests = fun _ _ _ -> async { return Error "merge tests loader" }
    LoadPrefixTests = fun _ _ _ -> async { return Error "prefix tests loader" }
    LoadStandardSorterEvals = fun _ _ _ -> async { return Error "standard eval loader" }
    LoadMergeSorterEvals = fun _ _ _ -> async { return Error "merge eval loader" }
    LoadPrefixSorterEvals = fun _ _ _ -> async { return Error "prefix eval loader" }
}

let private errorOf result =
    match result with
    | Error message -> message
    | Ok _ -> failwith "Expected missing run parameters to fail."

let private makeRunWithExecuteName executeName names (projectName: string) (runName: string) =
    SgdRun.create {
        DatabaseName = (UMX.tag "CatalogTests")
        ProjectName = (UMX.tag projectName)
        RunName = (UMX.tag runName)
        Description = "Catalog configuration test"
        AllowOverwrite = false
        Execution = {
            FunctionName = executeName
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf32MutationRate
        }
        Setup = {
            FunctionName = GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
            EnhancerFunctionName = SetupEnhancerFunctionNames.noOp
            OutputTypes = []
        }
    } {
        MakerNames = names
        GenSaveIntervalsName = "expInterval100_L50ss"
        GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
    }
    |> run.SgdRun

let private makeRun = makeRunWithExecuteName ExecuteFunctionNames.standard

let private makeSimpleRun executeName =
    SimpleRun.create {
        DatabaseName = (UMX.tag "CatalogTests")
        ProjectName = (UMX.tag "Tests")
        RunName = (UMX.tag "SimpleExecution")
        Description = "Execution test"
        AllowOverwrite = false
        Execution = {
            FunctionName = executeName
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
        }
        Setup = {
            FunctionName = GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
            EnhancerFunctionName = SetupEnhancerFunctionNames.noOp
            OutputTypes = []
        }
    }
    |> run.SimpleRun

[<Fact>]
let ``setup spans and query catalogs survive serialization for simple and SGD runs`` () =
    MessagePackSetup.configure ()
    for original in [makeSimpleRun ExecuteFunctionNames.sorterEval; makeRun EvolutionMakerNames.standard "Tests" "Sgd"] do
        let dto = RunDto.fromDomain original
        let configured = RunDto.toDomain { dto with SetupParameterSpans = [|{ Key = "SetupOnly"; Values = [|"one"; "two"|] }|]; SetupQueryCatalogName = "tests.setup-query" }
        let restored = configured |> RunDto.fromDomain |> MessagePackSerializer.Serialize |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
        Assert.Equal<(string * string list) list>(configured.SetupParameterSpans, restored.SetupParameterSpans)
        Assert.Equal("tests.setup-query", restored.SetupQueryCatalogName)
        Assert.Equal<(string * string list) list>(original.RunParameterSpans, restored.RunParameterSpans)
        Assert.Equal(original.RunQueryCatalogName, restored.RunQueryCatalogName)
        Assert.Equal(original.RunEnhancerFunctionName, restored.RunEnhancerFunctionName)
        Assert.Equal(original.SetupEnhancerFunctionName, restored.SetupEnhancerFunctionName)

[<Fact>]
let ``setup output types preserve every DU case and its value through run serialization`` () =
    MessagePackSetup.configure ()
    let outputTypes = [
        geneSortDataType.Run (UMX.tag "run")
        geneSortDataType.RunParameters (UMX.tag "parameters")
        geneSortDataType.SorterPoolSet "seed"
        geneSortDataType.SorterPoolSetSummarySet "summary"
        geneSortDataType.SorterSet "sorters"
        geneSortDataType.SorterModelSet "models"
        geneSortDataType.SortableTests "tests"
        geneSortDataType.SortableTestsWithPrefix "prefix"
        geneSortDataType.SorterSetEval "evaluations"
        geneSortDataType.SorterPoolBinsSetSeries "bins"
        geneSortDataType.SorterPoolSetHistory "history"
        geneSortDataType.TextReport (UMX.tag "report")
    ]
    for original in [makeSimpleRun ExecuteFunctionNames.sorterEval; makeRun EvolutionMakerNames.standard "Tests" "Sgd"] do
        for expected in [[]; outputTypes] do
            let dto = RunDto.fromDomain original
            let configured = RunDto.toDomain { dto with SetupOutputTypes = expected |> List.map OutputDataTypeDto.fromDomain |> List.toArray }
            let restored = configured |> RunDto.fromDomain |> MessagePackSerializer.Serialize |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
            Assert.Equal<geneSortDataType list>(expected, restored.SetupOutputTypes)
            Assert.Equal<geneSortDataType list>(expected, SimpleRun.setupOutputTypes restored.BaseRun)

[<Fact>]
let ``host generates independent setup and run combinations using their own query catalogs`` () =
    let runQuery = "tests.run-query." + Guid.NewGuid().ToString("N")
    let setupQuery = "tests.setup-query." + Guid.NewGuid().ToString("N")
    QueryParamsCatalogRun.register runQuery (fun project database rp dataType ->
        Some (queryParams.create database project (rp.GetRepl()) None dataType [|"Phase", "run"|]))
    QueryParamsCatalogRun.registerForSetup setupQuery (fun project database sp dataType ->
        Some (queryParams.create database project (sp.GetRepl()) None dataType [|"Phase", "setup"|]))
    let setupEnhancerName = "tests.setup-enhancer." + Guid.NewGuid().ToString("N")
    SetupEnhancerCatalog.register setupEnhancerName (fun context sp ->
        let query = context.QueryParamsFromSetupParams sp (geneSortDataType.Run context.Run.RunName) |> Option.get
        sp.WithId(Some query.Id))
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    let configured =
        { dto with
            RunParameterSpans = [|{ Key = "RunOnly"; Values = [|"a"; "b"; "c"|] }|]
            RunQueryCatalogName = runQuery
            SetupParameterSpans = [|{ Key = "SetupOnly"; Values = [|"one"; "two"|] }|]
            SetupQueryCatalogName = setupQuery
            SetupEnhancerFunctionName = setupEnhancerName }
        |> RunDto.toDomain
    let host = runHost.Create configured 1
    let runParameters = host.MakeRunParameters 0<replNumber> 2<replNumber>
    let setupParameters = host.MakeSetupParameters 0<replNumber> 2<replNumber>
    Assert.Equal(12, runParameters.Length)
    Assert.Equal(4, setupParameters.Length)
    for rp in runParameters do
        Assert.False(rp.ParamMap.ContainsKey "SetupOnly")
        let query = host.QueryParamsFromRunParams rp (geneSortDataType.Run configured.RunName) |> Option.get
        Assert.Equal("run", query.Properties.["Phase"])
        Assert.Equal(Some query.Id, rp.GetId())
    for sp in setupParameters do
        Assert.False(sp.ParamMap.ContainsKey "RunOnly")
        let query = host.QueryParamsFromSetupParams sp (geneSortDataType.Run configured.RunName) |> Option.get
        Assert.Equal("setup", query.Properties.["Phase"])
        Assert.Equal(Some query.Id, sp.GetId())

[<Fact>]
let ``parameter refinement enhances every parameter combination`` () =
    let enhancerName = "tests.enhance-every-combination"
    RunParamEnhancerBuilders.register enhancerName (fun _ rp -> rp.WithRunFinished(Some false)) |> ignore
    let dto = makeSimpleRun ExecuteFunctionNames.standard |> RunDto.fromDomain
    let configuredRun = { dto with RunEnhancerFunctionName = enhancerName } |> RunDto.toDomain
    let host = runHost.Create configuredRun 1
    let inputs =
        [| for width in [ 3; 4; 5; 8 ] ->
            emptyParameters.WithSortingWidth(Some (UMX.tag<sortingWidth> width))
                .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf4) |]
    let results = host.ParamMapRefiner inputs |> Seq.toArray
    Assert.Equal(inputs.Length, results.Length)
    Array.iter2 (fun (input: runParameters) (result: runParameters) ->
        Assert.Equal(input.GetSortingWidth(), result.GetSortingWidth())
        Assert.Equal(Some false, result.GetRunFinished())) inputs results

[<Fact>]
let ``built-in setup enhancers preserve parameters without invoking the run enhancer`` () =
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    for name in [SetupEnhancerFunctionNames.noOp; SetupEnhancerFunctionNames.sortableTests;
                 SetupEnhancerFunctionNames.sorterEval; SetupEnhancerFunctionNames.sorterMutate;
                 SetupEnhancerFunctionNames.sorterSgd] do
        let configured = RunDto.toDomain { dto with SetupEnhancerFunctionName = name }
        let host = runHost.Create configured 1
        let input = setupParameters.create (Map.ofList ["custom", "preserved"])
        let result = host.SetupParamMapRefiner [input] |> Seq.exactlyOne
        Assert.Same(input, result)
        let generated = host.MakeSetupParameters 0<replNumber> 2<replNumber>
        Assert.Equal(2, generated.Length)
        Assert.All(generated, fun sp -> Assert.Equal(1, sp.ParamMap.Count))

[<Theory>]
[<InlineData(null)>]
[<InlineData("")>]
[<InlineData(" ")>]
let ``both enhancer function names are required when constructing runs`` missing =
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain { dto with RunEnhancerFunctionName = missing } |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain { dto with SetupEnhancerFunctionName = missing } |> ignore) |> ignore

[<Fact>]
let ``setup enhancer catalog rejects unknown names and duplicate registrations`` () =
    Assert.Throws<InvalidOperationException>(fun () -> SetupEnhancerCatalog.get (Guid.NewGuid().ToString()) |> ignore) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SetupEnhancerCatalog.register SetupEnhancerFunctionNames.noOp SetupEnhancerCatalog.noOp) |> ignore
    Assert.Throws<ArgumentException>(fun () -> SetupEnhancerCatalog.register " " SetupEnhancerCatalog.noOp) |> ignore

[<Theory>]
[<InlineData("sorter-eval.evaluate")>]
[<InlineData("sorter-mutate.evaluate")>]
let ``simple run execution names persist and resolve on host creation`` name =
    MessagePackSetup.configure ()
    let restored = makeSimpleRun name |> RunDto.fromDomain |> MessagePackSerializer.Serialize |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(name, restored.ExecuteFunctionName)
    let host = runHost.Create restored 1
    Assert.Same(ExecuteFunctionCatalog.get name, host.ExecuteFunction)
    Assert.True(host.ExecutionContext.MakeMutator.IsNone)
    Assert.True(host.ExecutionContext.GenSaveIntervals.IsNone)

[<Fact>]
let ``simple host rejects unknown execution names`` () =
    let name = Guid.NewGuid().ToString()
    let error = Assert.Throws<InvalidOperationException>(fun () -> runHost.Create (makeSimpleRun name) 1 |> ignore)
    Assert.Contains(name, error.Message)

[<Fact>]
let ``evaluation and mutation executors use the simple host configured function`` () =
    let name = Guid.NewGuid().ToString()
    let mutable calls = 0
    ExecuteFunctionCatalog.register name (fun context rp _ _ _ -> async {
        Assert.Equal("CatalogTests", %context.RunDb.databaseName)
        calls <- calls + 1
        return Ok rp
    })
    let host = runHost.Create (makeSimpleRun name) 1
    use cts = new System.Threading.CancellationTokenSource()
    let executors = [
        GeneSort.Dispatch.V1.SorterEval.SorterEvalExecutor.standardExecutor
        GeneSort.Dispatch.V1.SorterEval.SorterEvalExecutor.mergeExecutor
        GeneSort.Dispatch.V1.SorterEval.SorterEvalExecutor.prefixExecutor
        GeneSort.Dispatch.V1.SorterMutate.SorterMutateExecutor.standardExecutor
        GeneSort.Dispatch.V1.SorterMutate.SorterMutateExecutor.mergeExecutor
        GeneSort.Dispatch.V1.SorterMutate.SorterMutateExecutor.prefixExecutor
    ]
    for executor in executors do
        let result = executor.Execute host emptyParameters (UMX.tag false) cts None |> Async.RunSynchronously
        Assert.Equal(Ok emptyParameters, result)
    Assert.Equal(6, calls)

[<Fact>]
let ``evolution entry reports missing SGD context on simple runs`` () =
    let host = runHost.Create (makeSimpleRun ExecuteFunctionNames.standard) 1
    use cts = new System.Threading.CancellationTokenSource()
    let result = host.ExecuteFunction host.ExecutionContext emptyParameters (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Contains("SGD context", errorOf result)

[<Theory>]
[<InlineData("evolution.standard")>]
[<InlineData("evolution.soss")>]
let ``host resolves built-in evolution functions at creation`` name =
    let configured = makeRunWithExecuteName name EvolutionMakerNames.standard "Tests" "Execute"
    Assert.Equal(name, configured.ExecuteFunctionName)
    Assert.Equal(name, SimpleRun.executeFunctionName configured.BaseRun)
    match runHost.Create configured 1 with
    | SgdRunHost host -> Assert.Same(ExecuteFunctionCatalog.get name, host.ExecuteFunction)
    | _ -> failwith "Expected SGD host"

[<Fact>]
let ``execution uses the configured function regardless of executor label`` () =
    let name = Guid.NewGuid().ToString()
    let mutable calls = 0
    ExecuteFunctionCatalog.register name (fun _ rp _ _ _ -> async {
        calls <- calls + 1
        return Ok rp
    })
    let configured = makeRunWithExecuteName name EvolutionMakerNames.standard "Tests" "CustomExecution"
    let host = runHost.Create configured 1
    use cts = new System.Threading.CancellationTokenSource()
    for _ in 1..2 do
        let result = RunParamsExecutor.hostExecutor.Execute host emptyParameters (UMX.tag false) cts None |> Async.RunSynchronously
        match result with
        | Ok rp -> Assert.Same(emptyParameters, rp)
        | Error error -> failwith error
    Assert.Equal(2, calls)

[<Fact>]
let ``unknown evolution function is rejected when making the host`` () =
    let unknown = Guid.NewGuid().ToString()
    let configured = makeRunWithExecuteName unknown EvolutionMakerNames.standard "Tests" "Unknown"
    let error = Assert.Throws<InvalidOperationException>(fun () -> runHost.Create configured 1 |> ignore)
    Assert.Contains(unknown, error.Message)

[<Fact>]
let ``execute catalog rejects blank names and duplicate registrations`` () =
    let execute = ExecuteFunctionCatalog.get ExecuteFunctionNames.standard
    Assert.Throws<ArgumentException>(fun () -> ExecuteFunctionCatalog.register " " execute) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> ExecuteFunctionCatalog.register ExecuteFunctionNames.standard execute) |> ignore
    Assert.Throws<ArgumentException>(fun () -> makeRunWithExecuteName "" EvolutionMakerNames.standard "Tests" "Blank" |> ignore) |> ignore

[<Theory>]
[<InlineData("evolution.standard")>]
[<InlineData("evolution.soss")>]
[<InlineData("custom.evolution")>]
let ``execute function name survives MessagePack round trip`` name =
    MessagePackSetup.configure ()
    let configured = makeRunWithExecuteName name EvolutionMakerNames.standard "Tests" "Serialized"
    let restored = configured |> RunDto.fromDomain |> MessagePackSerializer.Serialize |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(name, restored.ExecuteFunctionName)
    Assert.Equal(name, SimpleRun.executeFunctionName restored.BaseRun)

[<Theory>]
[<InlineData(null)>]
[<InlineData("")>]
[<InlineData("  ")>]
let ``simple runs require execution names at construction and deserialization`` missing =
    Assert.Throws<ArgumentException>(fun () -> makeSimpleRun missing |> ignore) |> ignore
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain { dto with ExecuteFunctionName = missing } |> ignore) |> ignore

[<Fact>]
let ``SGD construction initializes execution in the base run`` () =
    let configured = makeRun EvolutionMakerNames.standard "Tests" "BaseExecution"
    Assert.Equal(ExecuteFunctionNames.standard, SimpleRun.executeFunctionName configured.BaseRun)

[<Fact>]
let ``saved SGD runs must name their execute function explicitly`` () =
    MessagePackSetup.configure ()
    let dto = makeRun EvolutionMakerNames.standard "Tests" "MissingExecution" |> RunDto.fromDomain
    let json = MessagePackSerializer.Serialize dto |> MessagePackSerializer.ConvertToJson |> System.Text.Json.Nodes.JsonNode.Parse
    Assert.True(json.AsObject().Remove "ExecuteFunctionName")
    let missing = MessagePackSerializer.ConvertFromJson(json.ToJsonString()) |> MessagePackSerializer.Deserialize<runDto>
    let error = Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain missing |> ignore)
    Assert.Contains("ExecuteFunctionName", error.Message)

[<Fact>]
let ``SGD host resolves custom configured makers`` () =
    let key = Guid.NewGuid().ToString()
    let names = evolutionMakerNames.create (key) (key) (key)
    SortableTestsMakerCatalog.register key (fun _ _ -> async { return Error "host tests" })
    SeedSorterPoolSetMakerCatalog.register key (fun _ _ -> async { return Error "host seed" })
    MutatorMakerCatalog.register key (fun _ -> async { return Error "host mutator" })
    match runHost.Create (makeRun names "CatalogTests" "Custom") 1 with
    | SimpleRunHost _ -> failwith "Expected SGD host"
    | SgdRunHost host ->
        Assert.Equal(names, host.MakerNames)
        Assert.Equal("host tests", host.MakeSortableTests sources emptyParameters |> Async.RunSynchronously |> errorOf)
        Assert.Equal("host seed", host.MakeSeedPoolSet sources emptyParameters |> Async.RunSynchronously |> errorOf)
        Assert.Equal("host mutator", host.MakeMutator emptyParameters |> Async.RunSynchronously |> errorOf)

[<Theory>]
[<InlineData("tests")>]
[<InlineData("seed")>]
[<InlineData("mutator")>]
let ``SGD host rejects unknown makers before execution`` field =
    let unknown = Guid.NewGuid().ToString()
    let names =
        match field with
        | "tests" -> EvolutionMakerNames.standard.WithSortableTests(unknown)
        | "seed" -> EvolutionMakerNames.standard.WithSeedPoolSet(unknown)
        | _ -> EvolutionMakerNames.standard.WithMutator(unknown)
    let error = Assert.Throws<InvalidOperationException>(fun () -> runHost.Create (makeRun names "Tests" "Unknown") 1 |> ignore)
    Assert.Contains(unknown, error.Message)

[<Fact>]
let ``SGD maker names survive MessagePack round trip`` () =
    MessagePackSetup.configure ()
    let names = evolutionMakerNames.create ("custom.tests") ("custom.seed") ("custom.mutator")
    let dto = makeRun names "Tests" "Custom" |> RunDto.fromDomain
    let restored = MessagePackSerializer.Serialize dto |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(Some names, restored.MakerNames)

[<Theory>]
[<InlineData(null)>]
[<InlineData("")>]
[<InlineData("  ")>]
let ``saved SGD runs require every explicit maker name`` missing =
    let dto = makeRun EvolutionMakerNames.standard "SorterSgd.Prefix" "OldGenSoss" |> RunDto.fromDomain
    for invalid in [
        { dto with SortableTestsMakerName = missing }
        { dto with SeedPoolSetMakerName = missing }
        { dto with MutatorMakerName = missing }
        { dto with SortableTestsMakerName = missing; SeedPoolSetMakerName = missing; MutatorMakerName = missing }
    ] do
        Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain invalid |> ignore) |> ignore

[<Fact>]
let ``run labels do not change explicit maker names`` () =
    let names = EvolutionMakerNames.standard
    let restored = makeRun names "SorterSgd.Prefix" "OldGenSoss" |> RunDto.fromDomain |> RunDto.toDomain
    Assert.Equal(Some names, restored.MakerNames)

[<Theory>]
[<InlineData("seed-pool.standard.standard", false)>]
[<InlineData("seed-pool.standard.merge", false)>]
[<InlineData("seed-pool.standard.prefix", false)>]
[<InlineData("seed-pool.soss.standard", true)>]
[<InlineData("seed-pool.soss.merge", true)>]
[<InlineData("seed-pool.soss.prefix", true)>]
let ``seed catalogs produce the requested pool variant`` (name: string) (expectSoss: bool) =
    ProjectCatalogs.registerAll ()
    let evaluation =
        sorterEvalV2.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 32<sortingWidth> 3<sortableCount>
            (UMX.tag 7) 2<stageLength> [||] (UMX.tag<isReflectionSymmetric> false)
            (UMX.tag 1) (UMX.tag 0) |> sorterEval.V2
    let evaluations = sorterSetEval.create (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) [|evaluation|]
    let supplied = {
        sources with
            LoadStandardSorterEvals = fun _ _ _ -> async { return Ok evaluations }
            LoadMergeSorterEvals = fun _ _ _ -> async { return Ok evaluations }
            LoadPrefixSorterEvals = fun _ _ _ -> async { return Ok evaluations }
    }
    let parameters =
        emptyParameters.WithSortingWidth(Some 32<sortingWidth>)
            .WithRepl(Some 0<replNumber>)
            .WithMergeLibId(Some (mergeLibId.create 32<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA))
            .WithPrefixLibId(Some (prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA))
            .WithSorterPoolCount(Some 1<sorterPoolCount>)
            .WithSorterCountPerPool(Some 1<sorterCountPerPool>)
            .WithSorterEvalMeasureInitial(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithRngType(Some rngType.Lcg)
            .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf4)
            .WithSeedSorterPoolSelectionType(Some (sorterSelectionType.GuidOrder 1<sorterCount>))
            .WithExcludeSelfCe(Some (UMX.tag<excludeSelfCe> true))
            .WithMutationMod(Some 0<mutationMod>)
    let poolSet =
        match SeedSorterPoolSetMakerCatalog.get name supplied parameters |> Async.RunSynchronously with
        | Ok value -> value
        | Error error -> failwith error
    match poolSet with
    | sorterPoolSet.Standard value ->
        Assert.False(expectSoss)
        Assert.Single(value.SorterPools) |> ignore
    | sorterPoolSet.Soss value ->
        Assert.True(expectSoss)
        Assert.Single(value.SorterPools) |> ignore

[<Theory>]
[<InlineData(true)>]
[<InlineData(false)>]
let ``workflows reject incompatible seeds before evaluating or saving`` (sossWorkflow: bool) =
    let standard = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) None
    let seed = if sossWorkflow then sorterPoolSet.Standard standard else SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
    let creator _ = async { return Ok seed }
    let result =
        if sossWorkflow then
            GeneSort.Dispatch.V1.SorterSgd.Soss.SgdEx_Soss.initializeAndSaveSeedPoolSet
                creator Unchecked.defaultof<_> Unchecked.defaultof<_> Unchecked.defaultof<_> emptyParameters
                Unchecked.defaultof<_> Unchecked.defaultof<_> ignore
            |> Async.RunSynchronously |> Result.map ignore
        else
            GeneSort.Dispatch.V1.SorterSgd.Standard.SgdEx_Standard.initializeAndSaveSeedPoolSet
                creator Unchecked.defaultof<_> Unchecked.defaultof<_> Unchecked.defaultof<_> emptyParameters
                Unchecked.defaultof<_> Unchecked.defaultof<_> ignore
            |> Async.RunSynchronously |> Result.map ignore
    Assert.Contains((if sossWorkflow then "Expected a Soss" else "Expected a Standard"), errorOf result)

[<Fact>]
let ``project catalogs initialize together without dispatch dependencies`` () =
    ProjectCatalogs.registerAll ()
    ProjectCatalogs.registerAll ()
    QueryParamsCatalogRun.get QueryCatalogNamesRun.sortableTestMerge |> ignore
    MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel |> ignore
    let assembly = typeof<evolutionSetupSources>.Assembly
    Assert.Equal("GeneSort.Project.V1", assembly.GetName().Name)
    Assert.DoesNotContain(assembly.GetReferencedAssemblies(), fun reference ->
        reference.Name = "GeneSort.Dispatch.V1" || reference.Name = "GeneSort.FileDb.V1" || reference.Name = "GeneSort.Db.V1")

[<Fact>]
let ``merge test maker uses the supplied storage sources`` () =
    let library = mergeLibId.create 16<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA
    let parameters = emptyParameters.WithMergeLibId(Some library).WithSortableDataFormat(Some sortableDataFormat.BitVector512)
    let mutable called = false
    let loadMergeTests (repl: int<replNumber>) actualLibrary format =
        async {
            called <- true
            Assert.Equal(0, UMX.untag repl)
            Assert.Equal(library, actualLibrary)
            Assert.Equal(sortableDataFormat.BitVector512, format)
            return Error "supplied loader result"
        }
    let suppliedSources = { sources with LoadMergeTests = loadMergeTests }
    ProjectCatalogs.registerAll ()
    let actual = SortableTestsMakerCatalog.get SortableTestsMakerNames.merge suppliedSources parameters |> Async.RunSynchronously |> errorOf
    Assert.True(called)
    Assert.Equal("supplied loader result", actual)

[<Theory>]
[<InlineData("standard")>]
[<InlineData("merge")>]
[<InlineData("prefix")>]
let ``built-in names resolve to the corresponding makers`` variant =
    SorterSgdMakerCatalogs.registerAll ()
    SorterSgdMakerCatalogs.registerAll ()
    let testsName, testsMaker, seedName, seedMaker =
        match variant with
        | "standard" -> SortableTestsMakerNames.standard, EvolutionTestMakers.makeStandardTests,
                        SeedSorterPoolSetMakerNames.Standard.standard, SeedPoolSetMakers.createStandardSeedSorterPoolSetStandard
        | "merge" -> SortableTestsMakerNames.merge, EvolutionTestMakers.makeMergeTests,
                     SeedSorterPoolSetMakerNames.Standard.merge, SeedPoolSetMakers.createStandardSeedSorterPoolSetMerge
        | _ -> SortableTestsMakerNames.prefix, EvolutionTestMakers.makePrefixTests,
               SeedSorterPoolSetMakerNames.Standard.prefix, SeedPoolSetMakers.createStandardSeedSorterPoolSetPrefix
    let expectedTests = testsMaker sources emptyParameters |> Async.RunSynchronously |> errorOf
    let actualTests = SortableTestsMakerCatalog.get testsName sources emptyParameters |> Async.RunSynchronously |> errorOf
    Assert.Equal(expectedTests, actualTests)
    let expectedSeed = seedMaker sources emptyParameters |> Async.RunSynchronously |> errorOf
    let actualSeed = SeedSorterPoolSetMakerCatalog.get seedName sources emptyParameters |> Async.RunSynchronously |> errorOf
    Assert.Equal(expectedSeed, actualSeed)

[<Fact>]
let ``custom makers can be registered and invoked by name`` () =
    let name = Guid.NewGuid().ToString()
    SortableTestsMakerCatalog.register name (fun _ _ -> async { return Error "custom tests" })
    SeedSorterPoolSetMakerCatalog.register name (fun _ _ -> async { return Error "custom seed" })
    Assert.Equal("custom tests", SortableTestsMakerCatalog.get name sources emptyParameters |> Async.RunSynchronously |> errorOf)
    Assert.Equal("custom seed", SeedSorterPoolSetMakerCatalog.get name sources emptyParameters |> Async.RunSynchronously |> errorOf)
    Assert.Throws<InvalidOperationException>(fun () ->
        SortableTestsMakerCatalog.register name (fun _ _ -> async { return Error "replacement" })) |> ignore
    Assert.Throws<InvalidOperationException>(fun () ->
        SeedSorterPoolSetMakerCatalog.register name (fun _ _ -> async { return Error "replacement" })) |> ignore

[<Fact>]
let ``unknown names and blank registrations report catalog errors`` () =
    let name = Guid.NewGuid().ToString()
    Assert.Throws<InvalidOperationException>(fun () -> SortableTestsMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SeedSorterPoolSetMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        SortableTestsMakerCatalog.register " " (fun _ _ -> async { return Error "unused" })) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        SeedSorterPoolSetMakerCatalog.register " " (fun _ _ -> async { return Error "unused" })) |> ignore

[<Fact>]
let ``simple mutator maker resolves by its built-in name`` () =
    SorterSgdMakerCatalogs.registerAll ()
    SorterSgdMakerCatalogs.registerAll ()
    let expected = MutatorMakers.makeSimpleSorterModelMutator emptyParameters |> Async.RunSynchronously |> errorOf
    let actual = MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel emptyParameters |> Async.RunSynchronously |> errorOf
    Assert.Equal(expected, actual)

[<Fact>]
let ``mutator catalog supports custom makers and validates names`` () =
    let name = Guid.NewGuid().ToString()
    Assert.Throws<InvalidOperationException>(fun () -> MutatorMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        MutatorMakerCatalog.register " " (fun _ -> async { return Error "unused" })) |> ignore
    MutatorMakerCatalog.register name (fun _ -> async { return Error "custom mutator" })
    Assert.Equal("custom mutator", MutatorMakerCatalog.get name emptyParameters |> Async.RunSynchronously |> errorOf)
    Assert.Throws<InvalidOperationException>(fun () ->
        MutatorMakerCatalog.register name (fun _ -> async { return Error "replacement" })) |> ignore

[<Fact>]
let ``host creation initializes the maker catalogs`` () =
    runHost.Create (makeRun EvolutionMakerNames.standard "Tests" "CatalogInitialization") 1 |> ignore
    SortableTestsMakerCatalog.get SortableTestsMakerNames.prefix |> ignore
    SeedSorterPoolSetMakerCatalog.get SeedSorterPoolSetMakerNames.Standard.prefix |> ignore
    MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel |> ignore
