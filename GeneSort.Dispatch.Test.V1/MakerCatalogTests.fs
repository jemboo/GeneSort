module GeneSort.Dispatch.Test.V1.MakerCatalogTests

open System
open FSharp.UMX
open MessagePack
open Xunit
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Reports
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.Mp.V1
open GeneSort.Dispatch.V1

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
            ParametersEnhancerName = RunParametersEnhancerNames.msuf32MutationRate
        }
        Setup = {
            FunctionName = GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.sgdStandard
            SortableTestsModule = UMX.tag "SortableTests.Prefix.Channels32"
            SorterEvalModule = UMX.tag "SorterEval.Prefix"
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
            ParametersEnhancerName = SetupParametersEnhancerNames.noOp
        }
    } {
        MakerNames = names
        GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
        GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
    }
    |> Run.ofSgd

let private makeRun = makeRunWithExecuteName ExecuteFunctionNames.standard

let private pathWithoutMutationSettings () =
    let configured = makeRun EvolutionMakerNames.standard "Tests" "Mutator"
    let parameters = emptyParameters.WithId(Some (UMX.tag (Guid.NewGuid())))
                                    .WithRunName(Some configured.RunName).WithRepl(Some 0<replNumber>)
                                    .WithGenerationIntervalLast(Some (UMX.tag 1))
                                    .WithSetupSorterPoolSetId(Some (UMX.tag (Guid.NewGuid())))
                                    .WithSetupSortableTestsWithPrefixId(Some (UMX.tag (Guid.NewGuid())))
    EvoPath.fromParameters configured parameters |> fst

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
            ParametersEnhancerName = RunParametersEnhancerNames.sorterEvalStandard
        }
        Setup = {
            FunctionName = GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
            ParametersEnhancerName = SetupParametersEnhancerNames.noOp
        }
    }
    |> Run.ofSimple

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
        Assert.Equal(original.RunParametersEnhancerName, restored.RunParametersEnhancerName)
        Assert.Equal(original.SetupParametersEnhancerName, restored.SetupParametersEnhancerName)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SGD setup functions and source modules are explicit and serialized`` soss =
    MessagePackSetup.configure ()
    let configured = if soss then WorkflowFixtures.sossSource [0; 1] else WorkflowFixtures.standardSource ()
    let dto = { RunDto.fromDomain configured with SetupSortableTestsModule = "Sources.SelectedTests"; SetupSorterEvalModule = "Sources.SelectedEvals" }
    Assert.Equal(Some dto.SetupFunctionName, configured.SetupFunctionName)
    let prepare configured = RunPreparation.prepare { Run = configured; FirstReplica = 0; ReplicaCount = 1 }
    let restored = MessagePackSerializer.Serialize dto |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(Some dto.SetupFunctionName, restored.SetupFunctionName)
    Assert.Equal("Sources.SelectedTests", string restored.SgdSetup.Value.SortableTestsModule)
    Assert.Equal("Sources.SelectedEvals", string restored.SgdSetup.Value.SorterEvalModule)
    Assert.Equal(dto, RunDto.fromDomain restored)
    Assert.True((prepare (RunDto.toDomain dto)).Entries = (prepare restored).Entries)
    for incomplete in [{ dto with SetupFunctionName = null }; { dto with SetupSortableTestsModule = null }; { dto with SetupSorterEvalModule = null }] do
        Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain incomplete |> ignore) |> ignore

[<Fact>]
let ``host generates independent setup and run combinations using their own query catalogs`` () =
    let runQuery = "tests.run-query." + Guid.NewGuid().ToString("N")
    let setupQuery = "tests.setup-query." + Guid.NewGuid().ToString("N")
    QueryParamsCatalogRun.register runQuery (fun project database rp dataType ->
        Some (queryParams.create database project (rp.GetRepl()) None dataType [|"Phase", "run"|]))
    QueryParamsCatalogSetup.register setupQuery (fun project database sp dataType ->
        Some (queryParams.create database project (sp.GetRepl()) None dataType [|"Phase", "setup"|]))
    let setupParametersEnhancerName = "tests.setup-parameters-enhancer." + Guid.NewGuid().ToString("N")
    SetupParametersEnhancerCatalog.register setupParametersEnhancerName (fun context sp ->
        let query = context.QueryParamsFromSetupParams sp (geneSortDataType.Run context.Run.RunName) |> Option.get
        sp.WithId(Some query.Id))
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    let configured =
        { dto with
            RunParameterSpans = [|{ Key = "RunOnly"; Values = [|"a"; "b"; "c"|] }|]
            RunQueryCatalogName = runQuery
            SetupParameterSpans = [|{ Key = "SetupOnly"; Values = [|"one"; "two"|] }|]
            SetupQueryCatalogName = setupQuery
            SetupParametersEnhancerName = setupParametersEnhancerName }
        |> RunDto.toDomain
    let host = runHost.Create configured 1
    let runParameters = host.MakeRunParameters 0<replNumber> 2<replNumber>
    let setupParameters = host.MakeSetupParameters 0<replNumber> 2<replNumber>
    Assert.Equal(12, runParameters.Length)
    Assert.Equal(4, setupParameters.Length)
    for rp in runParameters do
        Assert.Contains(rp.ParamMap.["SetupOnly"], [|"one"; "two"|])
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
    RunParametersEnhancerCatalog.register enhancerName (fun _ rp -> rp.WithRunFinished(Some false)) |> ignore
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    let configuredRun = { dto with RunParametersEnhancerName = enhancerName } |> RunDto.toDomain
    let host = runHost.Create configuredRun 1
    let inputs =
        [| for width in [ 3; 4; 5; 8 ] ->
            emptyParameters.WithChannelCount(Some (UMX.tag<channelCount> width))
                .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf4) |]
    let results = host.EnhanceRunParameters inputs |> Seq.toArray
    Assert.Equal(inputs.Length, results.Length)
    Array.iter2 (fun (input: runParameters) (result: runParameters) ->
        Assert.Equal(input.GetChannelCount(), result.GetChannelCount())
        Assert.Equal(Some false, result.GetRunFinished())) inputs results

[<Fact>]
let ``hosts reject unknown enhancers at creation`` () =
    for original in [makeSimpleRun ExecuteFunctionNames.sorterEval; makeRun EvolutionMakerNames.standard "Tests" "Sgd"] do
        let dto = RunDto.fromDomain original
        let unknown = "tests.unknown-enhancer." + Guid.NewGuid().ToString("N")
        for configured in [ { dto with RunParametersEnhancerName = unknown }; { dto with SetupParametersEnhancerName = unknown } ] do
            let error = Assert.Throws<InvalidOperationException>(fun () -> runHost.Create (RunDto.toDomain configured) 1 |> ignore)
            Assert.Contains(unknown, error.Message)

[<Fact>]
let ``hosts retain resolved enhancers for refinement and generation`` () =
    for original in [makeSimpleRun ExecuteFunctionNames.sorterEval; makeRun EvolutionMakerNames.standard "Tests" "Sgd"] do
        let name = "tests.stored-enhancer." + Guid.NewGuid().ToString("N")
        let mutable runCalls = 0
        let mutable setupCalls = 0
        let enhanceRun : runParametersEnhancer = fun _ rp ->
            runCalls <- runCalls + 1
            rp.WithRunFinished(Some false)
        let enhanceSetup : setupParametersEnhancer = fun _ sp ->
            setupCalls <- setupCalls + 1
            sp
        RunParametersEnhancerCatalog.register name enhanceRun |> ignore
        SetupParametersEnhancerCatalog.register name enhanceSetup
        QueryParamsCatalogRun.register name (fun project database rp dataType ->
            Some (queryParams.create database project (rp.GetRepl()) None dataType [||]))
        let dto = RunDto.fromDomain original
        let configured = RunDto.toDomain { dto with RunParametersEnhancerName = name; SetupParametersEnhancerName = name; RunQueryCatalogName = name }
        let host = runHost.Create configured 1
        Assert.Same(enhanceRun, host.RunParametersEnhancer)
        Assert.Same(enhanceSetup, host.SetupParametersEnhancer)
        RunParametersEnhancerCatalog.register name (fun _ _ -> failwith "Host looked up the run enhancer again.") |> ignore
        let refined = host.EnhanceRunParameters [emptyParameters] |> Seq.exactlyOne
        Assert.Equal(Some false, refined.GetRunFinished())
        host.MakeSetupParameters 0<replNumber> 1<replNumber> |> ignore
        let generated = host.MakeRunParameters 0<replNumber> 1<replNumber>
        Assert.Single(generated) |> ignore
        Assert.Equal(Some false, generated.[0].GetRunFinished())
        Assert.Equal(2, runCalls)
        Assert.Equal(2, setupCalls)

[<Fact>]
let ``no-op setup enhancers preserve parameters without invoking the run enhancer`` () =
    let dto = makeSimpleRun ExecuteFunctionNames.sorterEval |> RunDto.fromDomain
    for name in [SetupParametersEnhancerNames.noOp; SetupParametersEnhancerNames.sortableTests] do
        let configured = RunDto.toDomain { dto with SetupParametersEnhancerName = name }
        let host = runHost.Create configured 1
        let input = setupParameters.create (Map.ofList ["custom", "preserved"])
        let result = host.EnhanceSetupParameters [input] |> Seq.exactlyOne
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
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain { dto with RunParametersEnhancerName = missing } |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain { dto with SetupParametersEnhancerName = missing } |> ignore) |> ignore

[<Fact>]
let ``setup enhancer catalog rejects unknown names and duplicate registrations`` () =
    Assert.Throws<InvalidOperationException>(fun () -> SetupParametersEnhancerCatalog.get (Guid.NewGuid().ToString()) |> ignore) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SetupParametersEnhancerCatalog.register SetupParametersEnhancerNames.noOp SetupParametersEnhancers.noOp) |> ignore
    Assert.Throws<ArgumentException>(fun () -> SetupParametersEnhancerCatalog.register " " SetupParametersEnhancers.noOp) |> ignore

[<Theory>]
[<InlineData("sorter-eval.evaluate")>]
[<InlineData("sorter-mutate.evaluate")>]
let ``simple run execution names persist and resolve on host creation`` name =
    MessagePackSetup.configure ()
    let restored = makeSimpleRun name |> RunDto.fromDomain |> MessagePackSerializer.Serialize |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(name, restored.ExecuteFunctionName)
    let host = runHost.Create restored 1
    match ExecuteFunctionCatalog.get name, host with
    | SimpleExecution _, SimpleRunHost _ -> Assert.NotNull(host.ExecuteFunction)
    | _ -> failwith "Expected a simple execution binding and host."

[<Fact>]
let ``simple host rejects unknown execution names`` () =
    let name = Guid.NewGuid().ToString()
    let error = Assert.Throws<InvalidOperationException>(fun () -> runHost.Create (makeSimpleRun name) 1 |> ignore)
    Assert.Contains(name, error.Message)

[<Fact>]
let ``simple hosts retain the configured execution function`` () =
    let name = Guid.NewGuid().ToString()
    let mutable calls = 0
    ExecuteFunctionCatalog.register name (SimpleExecution (fun context rp _ _ _ -> async {
        Assert.Equal("CatalogTests", %context.RunDb.databaseName)
        calls <- calls + 1
        return Ok rp
    }))
    let host = runHost.Create (makeSimpleRun name) 1
    use cts = new System.Threading.CancellationTokenSource()
    for _ in 1..2 do
        let result = host.ExecuteFunction emptyParameters (UMX.tag false) cts None |> Async.RunSynchronously
        Assert.Equal(Ok emptyParameters, result)
    Assert.Equal(2, calls)

[<Theory>]
[<InlineData("evolution.standard")>]
[<InlineData("evolution.soss")>]
let ``evolution entry rejects simple runs when binding the host`` name =
    let error = Assert.Throws<ArgumentException>(fun () -> runHost.Create (makeSimpleRun name) 1 |> ignore)
    Assert.Equal("run", error.ParamName)
    Assert.Contains("requires an SgdRun", error.Message)

[<Theory>]
[<InlineData("evolution.standard")>]
[<InlineData("evolution.soss")>]
let ``host resolves built-in evolution functions at creation`` name =
    let configured = makeRunWithExecuteName name EvolutionMakerNames.standard "Tests" "Execute"
    Assert.Equal(name, configured.ExecuteFunctionName)
    Assert.Equal(name, SimpleRun.executeFunctionName configured.BaseRun)
    match runHost.Create configured 1 with
    | SgdRunHost host ->
        match ExecuteFunctionCatalog.get name with
        | SgdExecution _ -> Assert.NotNull(host.ExecuteFunction)
        | _ -> failwith "Expected an SGD execution binding"
    | _ -> failwith "Expected SGD host"

[<Fact>]
let ``SGD execution binds required dependencies once before running`` () =
    let name = Guid.NewGuid().ToString()
    let mutable bindings = 0
    let mutable executions = 0
    ExecuteFunctionCatalog.register name (SgdExecution (fun storage dependencies ->
        bindings <- bindings + 1
        Assert.Equal(GenIntervalNames.expInterval100_L50ss, dependencies.GenSaveIntervals.Name)
        Assert.Equal(GenIntervalNames.summaryInterval_C_1p5C, dependencies.GenSaveSubIntervals.Name)
        Assert.NotNull(dependencies.MakeMutator)
        fun path state _ _ _ -> async {
            Assert.Equal("CatalogTests", %storage.RunDb.databaseName)
            Assert.Equal(path.Settings[ParamKeys.setupSorterPoolSetIdKey], string path.Start.Pool.Id)
            Assert.Equal(path.Settings[ParamKeys.setupSortableTestsWithPrefixIdKey], string path.Start.Tests.Id)
            executions <- executions + 1
            return Ok state
        }))
    let host = runHost.Create (makeRunWithExecuteName name EvolutionMakerNames.standard "Tests" "Bound") 1
    Assert.Equal(1, bindings)
    let parameters = emptyParameters.WithId(Some (UMX.tag (Guid.NewGuid())))
                                    .WithRunName(Some host.Run.RunName).WithRepl(Some 0<replNumber>)
                                    .WithGenerationIntervalLast(Some (UMX.tag 1))
                                    .WithSetupSorterPoolSetId(Some (UMX.tag (Guid.NewGuid())))
                                    .WithSetupSortableTestsWithPrefixId(Some (UMX.tag (Guid.NewGuid())))
    use cts = new System.Threading.CancellationTokenSource()
    for _ in 1..2 do
        host.ExecuteFunction parameters (UMX.tag false) cts None |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Equal(1, bindings)
    Assert.Equal(2, executions)

[<Fact>]
let ``execution uses the configured function regardless of executor label`` () =
    let name = Guid.NewGuid().ToString()
    let mutable calls = 0
    ExecuteFunctionCatalog.register name (SimpleExecution (fun _ rp _ _ _ -> async {
        calls <- calls + 1
        return Ok rp
    }))
    let configured = makeRunWithExecuteName name EvolutionMakerNames.standard "Tests" "CustomExecution"
    let host = runHost.Create configured 1
    use cts = new System.Threading.CancellationTokenSource()
    for _ in 1..2 do
        let result = host.ExecuteFunction emptyParameters (UMX.tag false) cts None |> Async.RunSynchronously
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
    let names = evolutionMakerNames.create { SortableTests = (key); SeedPoolSet = (key); Mutator = (key) }
    SortableTestsMakerCatalog.register key (fun _ _ -> async { return Error "host tests" })
    SeedSorterPoolSetMakerCatalog.register key (fun _ _ -> async { return Error "host seed" })
    MutatorMakerCatalog.register key (fun _ -> async { return Error "host mutator" })
    match runHost.Create (makeRun names "CatalogTests" "Custom") 1 with
    | SimpleRunHost _ -> failwith "Expected SGD host"
    | SgdRunHost host ->
        Assert.Equal(names, host.MakerNames)
        Assert.Equal("host tests", host.MakeSortableTests sources emptyParameters |> Async.RunSynchronously |> errorOf)
        Assert.Equal("host seed", host.MakeSeedPoolSet sources emptyParameters |> Async.RunSynchronously |> errorOf)
        Assert.Equal("host mutator", host.MakeMutator (pathWithoutMutationSettings ()) |> Async.RunSynchronously |> errorOf)

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
    let names = evolutionMakerNames.create { SortableTests = ("custom.tests"); SeedPoolSet = ("custom.seed"); Mutator = ("custom.mutator") }
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
        sorterEvalV2.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 32<channelCount> 3<sortableCount>
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
        emptyParameters.WithChannelCount(Some 32<channelCount>)
            .WithRepl(Some 0<replNumber>)
            .WithMergeLibId(Some (mergeLibId.create 32<channelCount> 2<mergeDimension> sorterLibVariant.VariantA))
            .WithPrefixLibId(Some (prefixLibId.create 32<channelCount> 4<stageLength> prefixLibVariant.PrefixA))
            .WithSorterPoolCount(Some 1<sorterPoolCount>)
            .WithSorterCountPerPool(Some 1<sorterCountPerPool>)
            .WithSorterEvalMeasureInitial(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithRngType(Some rngType.Lcg)
            .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf4)
            .WithSeedSorterPoolSelectionType(Some (sorterSelectionType.GuidOrder 1<sorterCount>))
            .WithExcludeSelfCe(Some (UMX.tag<excludeSelfCe> true))
            .WithMutationMod(Some 0<mutationMod>)
    for maximum, probability in [None, None; Some 3, None; None, Some 0.5] do
        let poolSet =
            match SeedSorterPoolSetMakerCatalog.get name supplied ((parameters.WithAMax maximum).WithActivationProbability probability) |> Async.RunSynchronously with
            | Ok value -> value
            | Error error -> failwith error
        let models =
            match poolSet with
            | sorterPoolSet.Standard value ->
                Assert.False(expectSoss)
                Assert.Single(value.SorterPools) |> ignore
                value.SorterPools |> Map.values |> Seq.collect (fun p -> p.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel))
            | sorterPoolSet.Soss value ->
                Assert.True(expectSoss)
                Assert.Single(value.SorterPools) |> ignore
                value.SorterPools |> Map.values |> Seq.collect (fun p -> p.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel))
        Assert.All(models, fun model ->
            match maximum, model with
            | None, sorterModel.SimpleS paired when probability.IsSome ->
                Assert.Equal(SimpleSorterModel.getLength paired.Model, paired.Activators.Length)
            | None, sorterModel.Simple _ -> ()
            | Some expected, sorterModel.SimpleR paired ->
                Assert.Equal(expected, paired.AMax)
                Assert.Equal(SimpleSorterModel.getLength paired.Model, paired.Activators.Length)
                Assert.All(paired.Activators, fun value -> Assert.InRange(value, -expected, expected))
            | _ -> failwith "Seed model family does not match AMax")

[<Theory>]
[<InlineData(true)>]
[<InlineData(false)>]
let ``workflows reject incompatible seeds before evaluating or saving`` (sossWorkflow: bool) =
    let standard = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) None
    let seed = if sossWorkflow then sorterPoolSet.Standard standard else SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
    let result =
        if sossWorkflow then
            GeneSort.Project.V1.Catalogs.Setup.SorterSgd.EvoStartCreation.evaluateSossSeed
                emptyParameters Unchecked.defaultof<_> Unchecked.defaultof<_> seed
            |> Result.map ignore
        else
            GeneSort.Project.V1.Catalogs.Setup.SorterSgd.EvoStartCreation.evaluateStandardSeed
                emptyParameters Unchecked.defaultof<_> Unchecked.defaultof<_> seed
            |> Result.map ignore
    Assert.Contains((if sossWorkflow then "Expected a Soss" else "Expected a Standard"), errorOf result)

[<Fact>]
let ``project catalogs initialize together without dispatch dependencies`` () =
    ProjectCatalogs.registerAll ()
    ProjectCatalogs.registerAll ()
    QueryParamsCatalogRun.get QueryCatalogNamesRun.sortableTestMerge |> ignore
    MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel |> ignore
    let assembly = typeof<evolutionSetupSources>.Assembly
    Assert.Equal("GeneSort.Project.V1", assembly.GetName().Name)
    Assert.Same(assembly, typeof<IGeneSortDb>.Assembly)
    Assert.Same(assembly, typeof<executionBinding>.Assembly)
    for catalog in [
        "GeneSort.Project.V1.Catalogs.Execution.ExecuteFunctionCatalog"
        "GeneSort.Project.V1.Catalogs.Setup.SetupExecutorCatalog"
        "GeneSort.Project.V1.Catalogs.Reports.ReportCatalog"
    ] do
        Assert.NotNull(assembly.GetType(catalog))
    Assert.DoesNotContain(assembly.GetReferencedAssemblies(), fun reference ->
        reference.Name = "GeneSort.Dispatch.V1" || reference.Name = "GeneSort.FileDb.V1")

[<Fact>]
let ``Project report catalog uses supplied storage and run identity without a Dispatch host`` () =
    let configured = makeSimpleRun ExecuteFunctionNames.sorterEval
    let evaluation =
        sorterEvalV2.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 32<channelCount> 3<sortableCount>
            (UMX.tag 7) 2<stageLength> [||] (UMX.tag<isReflectionSymmetric> false)
            (UMX.tag 1) (UMX.tag 0) |> sorterEval.V2
    let evaluations = sorterSetEval.create (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) [|evaluation|]
    let reports = ResizeArray<dataTableReport>()
    let mutable reads = 0
    let database =
        { new IGeneSortDb with
            member _.databaseName = configured.DatabaseName
            member _.makeQueryParamsFromRunParams _ _ = failwith "Use the supplied query builder."
            member _.getRunParameters _ _ _ _ _ = failwith "No host or batch lookup is needed."
            member _.doesOutPutDataExist _ _ = failwith "No existence probe is needed."
            member _.loadIfFoundAsync _ _ = failwith "No optional load is needed."
            member _.loadAsync mode _ = async {
                Assert.Equal(geneSortDataMode.Results, mode)
                reads <- reads + 1
                return Ok (outputData.SorterSetEval evaluations)
            }
            member _.saveAsync mode _ output overwrite = async {
                Assert.Equal(geneSortDataMode.Results, mode)
                Assert.False(%overwrite)
                match output with
                | outputData.TextReport report -> reports.Add report
                | _ -> failwith "Only a report should be saved."
                return Ok ()
            } }
    let storage = runStorage.create {
        RunDb = database
        QueryParamsFromRunParams = fun _ dataType ->
            Some (queryParams.create configured.DatabaseName configured.ProjectName None None dataType [||])
    }
    let suppliedSources = workflowSources.create {
        OpenDatabase = fun _ _ _ -> failwith "Evaluation reporting must not open source databases."
        EvolutionModules = fun _ -> sources
        Evolution = fun _ _ -> sources
    }
    let report = ReportCatalog.get ReportCatalog.sorterEvalFull suppliedSources configured storage
    Assert.Equal(0, reads)
    Assert.Empty(reports)
    use cts = new System.Threading.CancellationTokenSource()
    report emptyParameters (UMX.tag false) cts None |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Equal(1, reads)
    let saved = Assert.Single(reports)
    let runNameIndex = saved.DataHeaders |> Array.findIndex ((=) "RunName")
    Assert.Equal<string>(%configured.RunName, (Assert.Single(saved.DataRows)).Split('\t').[runNameIndex])

[<Fact>]
let ``merge test maker uses the supplied storage sources`` () =
    let library = mergeLibId.create 16<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
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
    let path = pathWithoutMutationSettings ()
    let expected = MutatorMakers.makeSimpleSorterModelMutator path |> Async.RunSynchronously |> errorOf
    let actual = MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel path |> Async.RunSynchronously |> errorOf
    Assert.Equal(expected, actual)

[<Fact>]
let ``mutator catalog supports custom makers and validates names`` () =
    let name = Guid.NewGuid().ToString()
    Assert.Throws<InvalidOperationException>(fun () -> MutatorMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        MutatorMakerCatalog.register " " (fun _ -> async { return Error "unused" })) |> ignore
    MutatorMakerCatalog.register name (fun _ -> async { return Error "custom mutator" })
    Assert.Equal("custom mutator", MutatorMakerCatalog.get name (pathWithoutMutationSettings ()) |> Async.RunSynchronously |> errorOf)
    Assert.Throws<InvalidOperationException>(fun () ->
        MutatorMakerCatalog.register name (fun _ -> async { return Error "replacement" })) |> ignore

[<Fact>]
let ``host creation initializes the maker catalogs`` () =
    runHost.Create (makeRun EvolutionMakerNames.standard "Tests" "CatalogInitialization") 1 |> ignore
    SortableTestsMakerCatalog.get SortableTestsMakerNames.prefix |> ignore
    SeedSorterPoolSetMakerCatalog.get SeedSorterPoolSetMakerNames.Standard.prefix |> ignore
    MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel |> ignore
