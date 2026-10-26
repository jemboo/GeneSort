module GeneSort.Dispatch.Test.V2.SetupCatalogTests

open System
open FSharp.UMX
open MessagePack
open Xunit
open GeneSort.Core.Mp
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.SortableTestsWithPrefix
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.Parameters
open GeneSort.Project.Mp.V1
open GeneSort.Dispatch.V1

let private parameters = runParameters.create Map.empty

let private makeRun queryCatalog setupCatalog =
    let executeName =
        if queryCatalog = QueryCatalogNamesRun.sorterMutateStandard || queryCatalog = QueryCatalogNamesRun.sorterMutateMerge || queryCatalog = QueryCatalogNamesRun.sorterMutatePrefix then
            GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterMutate
        else GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval
    SimpleRun.create {
        DatabaseName = (UMX.tag "SetupTests")
        ProjectName = (UMX.tag "Tests")
        RunName = (UMX.tag "SetupTests")
        Description = "Setup test"
        AllowOverwrite = false
        Execution = {
            FunctionName = executeName
            ParameterSpans = []
            QueryCatalogName = queryCatalog
            EnhancerFunctionName = "sortable-test"
        }
        Setup = {
            FunctionName = setupCatalog
            ParameterSpans = []
            QueryCatalogName = queryCatalog
            EnhancerFunctionName = SetupEnhancerFunctionNames.noOp
        }
    } |> run.SimpleRun

let private makeHost () = runHost.Create (makeRun QueryCatalogNamesRun.sorterMutateStandard SetupCatalogNames.mutantStandard) 1

let private context (step: string -> setupFunction) : setupContext =
    setupContext.create {
        MakeSorterModelSets = Map.ofList [
            SorterModelSetCatalogNames.uniform, step "models"
            SorterModelSetCatalogNames.mutantStandard, step "mutant-standard"
            SorterModelSetCatalogNames.mutantMerge, step "mutant-merge"
            SorterModelSetCatalogNames.mutantPrefix, step "mutant-prefix"
        ]
        MakeStandardTestsWithPrefix = step "standard"
        MakeMergeTestsWithPrefix = step "merge"
        MakePrefixTestsWithPrefix = step "prefix"
    }

[<Theory>]
[<InlineData("setup.standard", "standard")>]
[<InlineData("setup.merge", "merge")>]
[<InlineData("setup.prefix", "prefix")>]
let ``combined setup executes models then the selected tests`` name expectedTests =
    let calls = ResizeArray<string>()
    let bindings = context (fun label rp -> async {
        Assert.Same(parameters, rp)
        calls.Add label
        return Ok ()
    })
    let result = SetupCatalog.get name bindings parameters |> Async.RunSynchronously
    Assert.Equal(Ok (), result)
    Assert.Equal<string list>(["models"; expectedTests], List.ofSeq calls)

[<Theory>]
[<InlineData("setup.mutant-standard", "mutant-standard", "standard")>]
[<InlineData("setup.mutant-merge", "mutant-merge", "merge")>]
[<InlineData("setup.mutant-prefix", "mutant-prefix", "prefix")>]
let ``mutant setup selects matching model and test makers`` name expectedModels expectedTests =
    let calls = ResizeArray<string>()
    let bindings = context (fun label rp -> async {
        Assert.Same(parameters, rp)
        calls.Add label
        return Ok ()
    })
    Assert.Equal(Ok (), SetupCatalog.get name bindings parameters |> Async.RunSynchronously)
    Assert.Equal<string list>([expectedModels; expectedTests], List.ofSeq calls)

[<Theory>]
[<InlineData("mutant-standard")>]
[<InlineData("standard")>]
let ``mutant setup phase stops when either setup step fails`` failingStep =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async {
        calls.Add label
        return if label = failingStep then Error "setup failed" else Ok ()
    })
    use cts = new System.Threading.CancellationTokenSource()
    let host = (makeHost ()).WithSetupContext(bindings)
    let result =
        SetupExecution.run host.Setup parameters cts None
        |> Async.RunSynchronously
    Assert.Equal(Error "setup failed", result)
    let expected = if failingStep = "mutant-standard" then ["mutant-standard"] else ["mutant-standard"; "standard"]
    Assert.Equal<string list>(expected, List.ofSeq calls)

[<Fact>]
let ``unbound mutant setup reports an error without running tests`` () =
    let calls = ResizeArray<string>()
    let bindings = (context (fun label _ -> async { calls.Add label; return Ok () })).WithMakeSorterModelSets(Map.empty)
    let result = SetupCatalog.get SetupCatalogNames.mutantPrefix bindings parameters |> Async.RunSynchronously
    Assert.True(Result.isError result)
    Assert.Empty(calls)

[<Theory>]
[<InlineData("sorter-eval.standard", "setup.standard")>]
[<InlineData("sorter-eval.standard", "setup.prefix")>]
[<InlineData("sorter-eval.merge", "setup.merge")>]
[<InlineData("sorter-eval.prefix", "setup.prefix")>]
[<InlineData("sorter-mutate.standard", "setup.mutant-standard")>]
[<InlineData("sorter-mutate.merge", "setup.mutant-merge")>]
[<InlineData("sorter-mutate.prefix", "setup.mutant-prefix")>]
let ``explicit setup function name survives serialization`` queryCatalog expected =
    MessagePackSetup.configure ()
    let configured = makeRun queryCatalog expected
    Assert.Equal(expected, configured.SetupFunctionName)
    let dto = RunDto.fromDomain configured
    let json = MessagePackSerializer.Serialize dto |> MessagePackSerializer.ConvertToJson |> System.Text.Json.Nodes.JsonNode.Parse

    let restored = MessagePackSerializer.ConvertFromJson(json.ToJsonString()) |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(expected, restored.SetupFunctionName)

[<Fact>]
let ``saved runs must specify a setup function name`` () =
    MessagePackSetup.configure ()
    let dto = makeRun QueryCatalogNamesRun.sorterEvalStandard SetupCatalogNames.prefix |> RunDto.fromDomain
    let json = MessagePackSerializer.Serialize dto |> MessagePackSerializer.ConvertToJson |> System.Text.Json.Nodes.JsonNode.Parse
    json.AsObject().Remove("SetupFunctionName") |> ignore
    let legacyDto = MessagePackSerializer.ConvertFromJson(json.ToJsonString()) |> MessagePackSerializer.Deserialize<runDto>
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain legacyDto |> ignore) |> ignore

[<Fact>]
let ``host setup honors the run setup function and custom names round trip`` () =
    MessagePackSetup.configure ()
    let customName = string (Guid.NewGuid())
    SetupCatalog.register customName (SetupFunctions.fromCatalogs SorterModelSetCatalogNames.mutantMerge SortableTestsWithPrefixCatalogNames.prefix)
    let configured = makeRun QueryCatalogNamesRun.sorterMutateStandard customName
    let restored = configured |> RunDto.fromDomain |> MessagePackSerializer.Serialize |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(customName, restored.SetupFunctionName)
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Ok () })
    let host = (runHost.Create restored 1).WithSetupContext(bindings)
    Assert.Equal(Ok (), host.Setup parameters |> Async.RunSynchronously)
    Assert.Equal<string list>(["mutant-merge"; "prefix"], List.ofSeq calls)

[<Theory>]
[<InlineData("models")>]
[<InlineData("standard")>]
let ``combined setup propagates errors and stops at the failing step`` failingStep =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async {
        calls.Add label
        return if label = failingStep then Error "setup failed" else Ok ()
    })
    let result = SetupCatalog.get SetupCatalogNames.standard bindings parameters |> Async.RunSynchronously
    Assert.Equal(Error "setup failed", result)
    let expected = if failingStep = "models" then ["models"] else ["models"; "standard"]
    Assert.Equal<string list>(expected, List.ofSeq calls)

[<Fact>]
let ``setup catalog bindings do not leak across runs`` () =
    let calls = ResizeArray<string>()
    let bind label = context (fun step _ -> async { calls.Add(label + step); return Ok () })
    let first = SetupCatalog.get SetupCatalogNames.prefix (bind "first:")
    let second = SetupCatalog.get SetupCatalogNames.prefix (bind "second:")
    second parameters |> Async.RunSynchronously |> ignore
    first parameters |> Async.RunSynchronously |> ignore
    Assert.Equal<string list>(["second:models"; "second:prefix"; "first:models"; "first:prefix"], List.ofSeq calls)

[<Fact>]
let ``custom catalog entries can be composed and reject invalid registrations`` () =
    let modelName, testName, setupName = string (Guid.NewGuid()), string (Guid.NewGuid()), string (Guid.NewGuid())
    let modelBuilder : setupBuilder = fun ctx -> ctx.MakeSorterModelSets.[SorterModelSetCatalogNames.uniform]
    let testsBuilder : setupBuilder = fun ctx -> ctx.MakeMergeTestsWithPrefix
    SorterModelSetCatalog.register modelName modelBuilder
    SortableTestsWithPrefixCatalog.register testName testsBuilder
    SetupCatalog.register setupName (SetupFunctions.fromCatalogs modelName testName)
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Ok () })
    Assert.Equal(Ok (), SetupCatalog.get setupName bindings parameters |> Async.RunSynchronously)
    Assert.Equal<string list>(["models"; "merge"], List.ofSeq calls)
    Assert.Throws<InvalidOperationException>(fun () -> SorterModelSetCatalog.register modelName modelBuilder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SortableTestsWithPrefixCatalog.register testName testsBuilder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SetupCatalog.register setupName modelBuilder) |> ignore
    Assert.Throws<ArgumentException>(fun () -> SetupCatalog.register " " modelBuilder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SetupCatalog.get "missing-setup" |> ignore) |> ignore

[<Theory>]
[<InlineData("setup.standard", "standard")>]
[<InlineData("setup.merge", "merge")>]
[<InlineData("setup.prefix", "prefix")>]
let ``evaluation setup phase creates setup without completing the run`` catalog testStep =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Ok () })
    let host = (runHost.Create (makeRun QueryCatalogNamesRun.sorterEvalStandard catalog) 1).WithSetupContext(bindings)
    use cts = new System.Threading.CancellationTokenSource()
    let result = SetupExecution.run host.Setup parameters cts None |> Async.RunSynchronously
    match result with
    | Ok updated ->
        Assert.Same(parameters, updated)
        Assert.NotEqual(Some true, updated.GetRunFinished())
    | Error error -> failwith error
    Assert.Equal<string list>(["models"; testStep], List.ofSeq calls)

[<Theory>]
[<InlineData("models")>]
[<InlineData("standard")>]
let ``evaluation setup phase stops on failure`` failingStep =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async {
        calls.Add label
        return if label = failingStep then Error "setup failed" else Ok ()
    })
    let host = (runHost.Create (makeRun QueryCatalogNamesRun.sorterEvalStandard SetupCatalogNames.standard) 1).WithSetupContext(bindings)
    use cts = new System.Threading.CancellationTokenSource()
    Assert.Equal(Error "setup failed", SetupExecution.run host.Setup parameters cts None |> Async.RunSynchronously)
    Assert.Equal<string list>((if failingStep = "models" then ["models"] else ["models"; "standard"]), List.ofSeq calls)

[<Theory>]
[<InlineData("standard")>]
[<InlineData("merge")>]
[<InlineData("prefix")>]
let ``evaluation run phase never invokes setup`` variant =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Error "Setup must not run" })
    let queryCatalog, setupCatalog =
        match variant with
        | "standard" -> QueryCatalogNamesRun.sorterEvalStandard, SetupCatalogNames.standard
        | "merge" -> QueryCatalogNamesRun.sorterEvalMerge, SetupCatalogNames.merge
        | _ -> QueryCatalogNamesRun.sorterEvalPrefix, SetupCatalogNames.prefix
    let host = (runHost.Create (makeRun queryCatalog setupCatalog) 1).WithSetupContext(bindings)
    use cts = new System.Threading.CancellationTokenSource()
    // Missing query parameters stop evaluation before disk access, after the old setup call site.
    let result = host.ExecuteFunction parameters (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(Error "Failed to create QueryParams for SorterModelSet.", result)
    Assert.Empty(calls)

[<Fact>]
let ``cancelled evaluation setup does not invoke makers`` () =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Ok () })
    let host = (runHost.Create (makeRun QueryCatalogNamesRun.sorterEvalStandard SetupCatalogNames.standard) 1).WithSetupContext(bindings)
    use cts = new System.Threading.CancellationTokenSource()
    cts.Cancel()
    let result = SetupExecution.run host.Setup parameters cts None |> Async.RunSynchronously
    Assert.True(Result.isError result)
    Assert.Empty(calls)

[<Theory>]
[<InlineData("standard")>]
[<InlineData("merge")>]
[<InlineData("prefix")>]
let ``mutant run phase does not execute setup`` variant =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Error "Unexpected setup" })
    let queryCatalog, setupCatalog =
        match variant with
        | "standard" -> QueryCatalogNamesRun.sorterMutateStandard, SetupCatalogNames.mutantStandard
        | "merge" -> QueryCatalogNamesRun.sorterMutateMerge, SetupCatalogNames.mutantMerge
        | _ -> QueryCatalogNamesRun.sorterMutatePrefix, SetupCatalogNames.mutantPrefix
    let host = (runHost.Create (makeRun queryCatalog setupCatalog) 1).WithSetupContext(bindings)
    use cts = new System.Threading.CancellationTokenSource()
    let result = host.ExecuteFunction parameters (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(Error "Failed to create QueryParams for mutant SorterModelSet.", result)
    Assert.Empty(calls)
