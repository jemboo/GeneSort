module GeneSort.Dispatch.Test.V1.SgdSetupConfigTests

open System
open System.Threading
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1

let private rows configured = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
let private configured soss = if soss then WorkflowFixtures.sossSource [0] else WorkflowFixtures.standardSource ()

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``each source database contributes only to its dependent setup identities`` soss =
    let original = configured soss
    let dto = RunDto.fromDomain original
    let testsChanged = RunDto.toDomain { dto with SetupSortableTestsDatabaseName = "OtherTests" }
    let evalsChanged = RunDto.toDomain { dto with SetupSorterSetEvalsDatabaseName = "OtherEvals" }
    let first = Assert.Single(rows original)
    let tests = Assert.Single(rows testsChanged)
    let evals = Assert.Single(rows evalsChanged)
    Assert.NotEqual(first.GetSetupSortableTestsWithPrefixId(), tests.GetSetupSortableTestsWithPrefixId())
    Assert.NotEqual(first.GetSetupSorterPoolSetId(), tests.GetSetupSorterPoolSetId())
    Assert.Equal(first.GetSetupSortableTestsWithPrefixId(), evals.GetSetupSortableTestsWithPrefixId())
    Assert.NotEqual(first.GetSetupSorterPoolSetId(), evals.GetSetupSorterPoolSetId())
    Assert.NotEqual(first.GetId(), tests.GetId())
    Assert.NotEqual(first.GetId(), evals.GetId())

let private emptyDb () =
    { new IGeneSortDb with
        member _.databaseName = UMX.tag "Output"
        member _.makeQueryParamsFromRunParams _ _ = failwith "Unexpected query"
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected parameter load"
        member _.saveAsync _ _ _ _ = failwith "Unexpected save"
        member _.loadAsync _ _ = failwith "Unexpected load"
        member _.loadIfFoundAsync _ _ = async.Return None
        member _.doesOutPutDataExist _ _ = async.Return false }

let private storage () = runStorage.create {
    RunDb = emptyDb()
    QueryParamsFromRunParams = fun _ _ -> failwith "Setup uses its recorded references"
}

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SGD setup binds the configured test and evaluation databases before invoking its makers`` soss =
    ProjectCatalogs.registerAll()
    let name = "test-databases-" + Guid.NewGuid().ToString("N")
    SortableTestsMakerCatalog.register name (fun _ _ -> async.Return (Error "Reached selected maker"))
    let dto = RunDto.fromDomain (configured soss)
    let run = RunDto.toDomain {
        dto with
            SortableTestsMakerName = name
            SetupSortableTestsDatabaseName = "MyTests"
            SetupSorterSetEvalsDatabaseName = "MyEvals"
    }
    let mutable selected = None
    let sources = workflowSources.create {
        OpenDatabase = fun _ _ _ -> failwith "Fresh setup does not import a checkpoint"
        Evolution = fun tests evals ->
            selected <- Some (string tests, string evals)
            Unchecked.defaultof<evolutionSetupSources>
    }
    use cts = new CancellationTokenSource()
    let setup = SetupExecutorCatalog.prepare sources run (storage())
    let result = setup (Assert.Single(rows run)) (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(Some ("MyTests", "MyEvals"), selected)
    match result with
    | Error message -> Assert.Contains("Reached selected maker", message)
    | Ok _ -> failwith "Expected marker from selected maker"

[<Fact>]
let ``setup dispatch uses its own function name independently of execution`` () =
    let name = "explicit-setup-" + Guid.NewGuid().ToString("N")
    let mutable called = false
    SetupExecutorCatalog.register name (fun _ _ _ -> fun rp _ _ _ -> async {
        called <- true
        return Ok (Parameters rp)
    })
    let dto = RunDto.fromDomain (configured false)
    let run = RunDto.toDomain { dto with SetupFunctionName = name; ExecuteFunctionName = "not-registered-execution" }
    let sources = workflowSources.create {
        OpenDatabase = fun _ _ _ -> failwith "Unexpected source"
        Evolution = fun _ _ -> Unchecked.defaultof<_>
    }
    let setup = SetupExecutorCatalog.prepare sources run (storage())
    use cts = new CancellationTokenSource()
    let result = setup (runParameters.create Map.empty) (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.True(Result.isOk result)
    Assert.True(called)

[<Theory>]
[<InlineData("")>]
[<InlineData("../Outside")>]
let ``editor validates source database names as separate folder names`` invalid =
    let run = configured false
    let args = (RunTypeEditor.fromRun run).ToArgs()
    for draft in [{ args with SortableTestsDatabaseName = invalid }; { args with SorterSetEvalsDatabaseName = invalid }] do
        Assert.True(RunTypeEditor.build run (runDraft.create draft) |> Result.isError)
