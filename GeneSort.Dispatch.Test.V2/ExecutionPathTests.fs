module GeneSort.Dispatch.Test.V2.ExecutionPathTests

open System
open System.Threading
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.Reports
open GeneSort.Dispatch.V1

let private createHostWithSaveResult saveResult finished execute =
    let name = Guid.NewGuid().ToString()
    ExecuteFunctionCatalog.register name (SimpleExecution execute)
    QueryParamsCatalogRun.register name (fun project database rp dataType ->
        Some (queryParams.create database project (rp.GetRepl()) None dataType [||]))
    QueryParamsCatalogSetup.register name (fun project database sp dataType ->
        Some (queryParams.create database project (sp.GetRepl()) None dataType [||]))
    let configured =
        SimpleRun.create {
            DatabaseName = (UMX.tag "Tests")
            ProjectName = (UMX.tag "Tests")
            RunName = (UMX.tag "Paths")
            Description = "Path test"
            AllowOverwrite = false
            Execution = {
                FunctionName = name
                ParameterSpans = []
                QueryCatalogName = name
                EnhancerFunctionName = "sortable-test"
            }
            Setup = {
                FunctionName = SetupCatalogNames.standard
                ParameterSpans = []
                QueryCatalogName = name
                EnhancerFunctionName = SetupEnhancerFunctionNames.noOp
            }
        }
        |> run.SimpleRun
    let rp = (runParameters.create Map.empty).WithRunFinished(Some finished).WithRepl(Some (UMX.tag 0))
    let writes = ResizeArray<geneSortDataMode>()
    let db =
        { new IGeneSortDb with
            member _.databaseName = UMX.tag "Tests"
            member _.makeQueryParamsFromRunParams _ _ = failwith "Use host query builder"
            member _.getRunParameters _ _ _ _ _ = async { return Ok [|rp|] }
            member _.saveAsync mode _ _ _ = async { writes.Add mode; return saveResult }
            member _.loadAsync _ _ = failwith "Unexpected load"
            member _.loadIfFoundAsync _ _ = failwith "Unexpected load"
            member _.doesOutPutDataExist _ _ = failwith "Unexpected probe" }
    (runHost.Create configured 1).WithRunDb(db), writes

let private createHost finished execute = createHostWithSaveResult (Ok ()) finished execute

[<Fact>]
let ``execution binds storage once and rebinding preserves previously prepared functions`` () =
    let bindings = ResizeArray<IGeneSortDb>()
    let used = ResizeArray<IGeneSortDb>()
    let bind (storage: runStorage) : runFunction =
        bindings.Add storage.RunDb
        fun rp _ _ _ -> async { used.Add storage.RunDb; return Ok rp }
    let original, _ = createHost false bind
    let replacement, _ = createHost false (fun _ rp _ _ _ -> async { return Ok rp })
    let previousExecute = original.ExecuteFunction
    let updated = original.WithRunDb replacement.RunDb
    Assert.Equal(3, bindings.Count)
    let parameters = runParameters.create Map.empty
    use cts = new CancellationTokenSource()
    for execute in [previousExecute; updated.ExecuteFunction; updated.ExecuteFunction] do
        execute parameters (UMX.tag false) cts None |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Equal(3, bindings.Count)
    Assert.Same(original.RunDb, used.[0])
    Assert.Same(replacement.RunDb, used.[1])
    Assert.Same(replacement.RunDb, used.[2])

[<Fact>]
let ``failed parameter persistence is reported as a batch failure`` () =
    let host, _ = createHostWithSaveResult (Error "Storage unavailable") false (fun _ rp _ _ _ -> async { return Ok rp })
    use cts = new CancellationTokenSource()
    let result = ProjectOps.executeRuns (UMX.tag 0) (UMX.tag 1) (UMX.tag false) cts None host 1 |> Async.RunSynchronously
    match result with
    | Ok [|Failure (_, _, error)|] -> Assert.Contains("Storage unavailable", error)
    | other -> failwithf "Expected persistence failure, got %A" other

[<Fact>]
let ``reports include completed runs without invoking execution or saving run state`` () =
    let host, writes = createHost true (fun _ _ _ _ _ -> failwith "Run execution must not happen")
    let mutable reportCalls = 0
    let report : runFunction =
        fun rp _ _ _ -> async {
                reportCalls <- reportCalls + 1
                Assert.Equal(Some true, rp.GetRunFinished())
                return Ok (rp.WithRunFinished(Some false))
            }
    use cts = new CancellationTokenSource()
    let result = ProjectOps.executeReportRuns (UMX.tag 0) (UMX.tag 1) (UMX.tag false) cts None host report 1 |> Async.RunSynchronously
    match result with
    | Ok [|Success _|] -> ()
    | other -> failwithf "Expected report success, got %A" other
    Assert.Equal(1, reportCalls)
    Assert.Empty(writes)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``setup phase never invokes execution`` setupFails =
    let calls = ResizeArray<string>()
    let host, writes = createHost false (fun _ rp _ _ _ -> async { calls.Add "run"; return Ok (rp.WithRunFinished(Some true)) })
    let setup : runFunction =
        fun rp _ _ _ -> async {
                calls.Add "setup"
                return if setupFails then Error "Setup failed" else Ok rp
            }
    use cts = new CancellationTokenSource()
    SetupExecutorCatalog.register host.Run.ExecuteFunctionName (fun _ _ _ -> setup)
    let result = ProjectOps.executeSetupRuns (UMX.tag 0) (UMX.tag 1) (UMX.tag false) cts None host 1 |> Async.RunSynchronously
    if setupFails then
        match result with | Ok [|Failure _|] -> () | other -> failwithf "Expected setup failure, got %A" other
        Assert.Equal<string list>(["setup"], List.ofSeq calls)
        Assert.Empty(writes)
    else
        Assert.True(Result.isOk result)
        Assert.Equal<string list>(["setup"], List.ofSeq calls)
        Assert.Equal(1, writes.Count)

[<Fact>]
let ``execute phase runs without resolving setup`` () =
    let mutable calls = 0
    // Deliberately no setup registration: a cloud worker only needs execution.
    let host, writes = createHost false (fun _ rp _ _ _ -> async {
        calls <- calls + 1
        return Ok (rp.WithRunFinished(Some true))
    })
    use cts = new CancellationTokenSource()
    let result = ProjectOps.executeRuns (UMX.tag 0) (UMX.tag 1) (UMX.tag false) cts None host 1 |> Async.RunSynchronously
    match result with
    | Ok [|Success _|] -> ()
    | other -> failwithf "Expected execution success, got %A" other
    Assert.Equal(1, calls)
    Assert.Single(writes) |> ignore

[<Fact>]
let ``normal run path skips completed runs`` () =
    let host, writes = createHost true (fun _ _ _ _ _ -> failwith "Completed run should be skipped")
    use cts = new CancellationTokenSource()
    let result = ProjectOps.executeRuns (UMX.tag 0) (UMX.tag 1) (UMX.tag false) cts None host 1 |> Async.RunSynchronously
    match result with
    | Ok [|Skipped _|] -> ()
    | other -> failwithf "Expected skip, got %A" other
    Assert.Empty(writes)

[<Fact>]
let ``missing setup registration fails preparation`` () =
    let host, writes = createHost false (fun _ _ _ _ _ -> failwith "Run must not start without setup")
    use cts = new CancellationTokenSource()
    let result = ProjectOps.executeSetupRuns (UMX.tag 0) (UMX.tag 1) (UMX.tag false) cts None host 1 |> Async.RunSynchronously
    match result with
    | Error error -> Assert.Contains("No setup executor is registered", error)
    | Ok _ -> failwith "Expected setup lookup failure"
    Assert.Empty(writes)

[<Fact>]
let ``built-in hosts resolve setup for every execution family`` () =
    let runs = [
        WorkflowFixtures.sortableTestsRun false
        WorkflowFixtures.sortableTestsRun true
        WorkflowFixtures.standardEvaluationHost ()
        WorkflowFixtures.mutationHost ()
        WorkflowFixtures.standardSource ()
        WorkflowFixtures.sossSource [0]
    ]
    for run in runs do
        let host = runHost.Create run 1
        Assert.NotNull(host.SetupExecutor)

[<Fact>]
let ``setup executor catalog rejects blank and duplicate names`` () =
    let builder _ _ _ : runFunction = fun rp _ _ _ -> async { return Ok rp }
    Assert.Throws<ArgumentException>(fun () -> SetupExecutorCatalog.register " " builder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () ->
        SetupExecutorCatalog.register GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.sorterEval builder) |> ignore

[<Fact>]
let ``report catalog exposes all report families independently of run execution`` () =
    Assert.Equal(10, ReportCatalog.names.Length)
    for name in ReportCatalog.names do Assert.NotNull(ReportCatalog.get name)
    Assert.Throws<ArgumentException>(fun () -> ReportCatalog.get "unknown-report" |> ignore) |> ignore
