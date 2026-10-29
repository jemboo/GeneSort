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
let ``each source module contributes only to its dependent setup identities`` soss =
    let original = configured soss
    let dto = RunDto.fromDomain original
    let testsChanged = RunDto.toDomain { dto with SetupSortableTestsModule = "Sources.OtherTests" }
    let evalsChanged = RunDto.toDomain { dto with SetupSorterEvalModule = "Sources.OtherEvals" }
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
let ``SGD setup binds the configured test and evaluation modules before invoking its makers`` soss =
    ProjectCatalogs.registerAll()
    let name = "test-databases-" + Guid.NewGuid().ToString("N")
    SortableTestsMakerCatalog.register name (fun _ _ -> async.Return (Error "Reached selected maker"))
    let dto = RunDto.fromDomain (configured soss)
    let run = RunDto.toDomain {
        dto with
            SortableTestsMakerName = name
            SetupSortableTestsModule = "Sources.MyTests"
            SetupSorterEvalModule = "Sources.MyEvals"
    }
    let mutable selected = None
    let sources = workflowSources.create {
        OpenDatabase = fun _ _ _ -> failwith "Fresh setup does not import a checkpoint"
        Evolution = fun _ _ -> failwith "SGD must use module sources"
        EvolutionModules = fun parameters ->
            selected <- Some (parameters.ParamMap[ParamKeys.sortableTestsModuleKey], parameters.ParamMap[ParamKeys.sorterEvalModuleKey])
            Unchecked.defaultof<evolutionSetupSources>
    }
    use cts = new CancellationTokenSource()
    let setup = SetupExecutorCatalog.prepare sources run (storage())
    let result = setup (Assert.Single(rows run)) (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(Some ("Sources.MyTests", "Sources.MyEvals"), selected)
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
        EvolutionModules = fun _ -> Unchecked.defaultof<_>
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
[<InlineData("Prefix")>]
let ``editor validates source qualified source module names`` invalid =
    let run = configured false
    let args = (RunTypeEditor.fromRun run).ToArgs()
    for draft in [{ args with SortableTestsModule = invalid }; { args with SorterEvalModule = invalid }] do
        Assert.True(RunTypeEditor.build run (runDraft.create draft) |> Result.isError)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SGD source modules are setup inputs and never processing query properties`` soss =
    let run = configured soss
    let parameters = Assert.Single(rows run)
    Assert.Equal(string run.SgdSetup.Value.SortableTestsModule, parameters.ParamMap[ParamKeys.sortableTestsModuleKey])
    Assert.Equal(string run.SgdSetup.Value.SorterEvalModule, parameters.ParamMap[ParamKeys.sorterEvalModuleKey])
    let changed = runParameters.create (parameters.ParamMap
        |> Map.add ParamKeys.sortableTestsModuleKey "Other.Tests"
        |> Map.add ParamKeys.sorterEvalModuleKey "Other.Evaluations")
    let query = QueryParamsCatalogRun.get run.RunQueryCatalogName run.ProjectName run.DatabaseName
    for output in [geneSortDataType.Run run.RunName; geneSortDataType.RunParameters run.RunName; geneSortDataType.SorterPoolSet ""] do
        let original = query parameters output |> Option.get
        Assert.False(original.Properties.ContainsKey ParamKeys.sortableTestsModuleKey)
        Assert.False(original.Properties.ContainsKey ParamKeys.sorterEvalModuleKey)
        Assert.Equal(original.Id, (query changed output |> Option.get).Id)

[<Fact>]
let ``mutation source modules identify setup artifacts and stay out of processing queries`` () =
    ProjectCatalogs.registerAll()
    GeneSort.Dispatch.V1.RunTypeCatalog.all |> ignore
    for run in [GeneSort.Dispatch.V1.SorterMutate.Standard.Msrs(); GeneSort.Dispatch.V1.SorterMutate.Merge.Msce(); GeneSort.Dispatch.V1.SorterMutate.Prefix.Msce()] do
        let row = rows run |> Array.head
        let setup = QueryParamsCatalogSetup.get run.SetupQueryCatalogName run.ProjectName run.DatabaseName
        let parameters = setupParameters.create row.ParamMap
        let models = setup parameters (geneSortDataType.SorterModelSet "") |> Option.get
        let selected = row.ParamMap[ParamKeys.sorterEvalModuleKey]
        Assert.True(RunModuleCatalog.resolve selected |> Result.isOk)
        Assert.Equal(selected, models.Properties[ParamKeys.sorterEvalModuleKey])
        let changed = setupParameters.create (row.ParamMap |> Map.add ParamKeys.sorterEvalModuleKey "Sources.OtherEvaluations")
        Assert.NotEqual(models.Id, (setup changed (geneSortDataType.SorterModelSet "") |> Option.get).Id)
        match row.ParamMap.TryFind ParamKeys.sortableTestsModuleKey with
        | Some selectedTests ->
            Assert.True(RunModuleCatalog.resolve selectedTests |> Result.isOk)
            let tests = setup parameters (geneSortDataType.SortableTestsWithPrefix "") |> Option.get
            Assert.Equal(selectedTests, tests.Properties[ParamKeys.sortableTestsModuleKey])
            let changedTests = setupParameters.create (row.ParamMap |> Map.add ParamKeys.sortableTestsModuleKey "Sources.OtherTests")
            Assert.NotEqual(tests.Id, (setup changedTests (geneSortDataType.SortableTestsWithPrefix "") |> Option.get).Id)
        | None -> () // Standard tests are generated locally.
        let query = QueryParamsCatalogRun.get run.RunQueryCatalogName run.ProjectName run.DatabaseName
        let processing = query row (geneSortDataType.RunParameters run.RunName) |> Option.get
        Assert.False(processing.Properties.ContainsKey ParamKeys.sortableTestsModuleKey)
        Assert.False(processing.Properties.ContainsKey ParamKeys.sorterEvalModuleKey)

[<Fact>]
let ``mutation setup obtains source modules from the generated setup row`` () =
    ProjectCatalogs.registerAll()
    for run in [GeneSort.Dispatch.V1.SorterMutate.Standard.Msrs(); GeneSort.Dispatch.V1.SorterMutate.Merge.Msce(); GeneSort.Dispatch.V1.SorterMutate.Prefix.Msce()] do
        let row = rows run |> Array.head
        let selectedRow = runParameters.create (row.ParamMap
            |> Map.add ParamKeys.sortableTestsModuleKey "Sources.SelectedTests"
            |> Map.add ParamKeys.sorterEvalModuleKey "Sources.SelectedEvaluations")
        let mutable selected = None
        let sources = workflowSources.create {
            OpenDatabase = fun _ _ _ -> failwith "Unexpected direct database access"
            Evolution = fun _ _ -> failwith "Mutation must use module sources"
            EvolutionModules = fun parameters ->
                selected <- Some (parameters.ParamMap[ParamKeys.sortableTestsModuleKey], parameters.ParamMap[ParamKeys.sorterEvalModuleKey])
                failwith "Reached module source resolver"
        }
        let query = QueryParamsCatalogSetup.get run.SetupQueryCatalogName run.ProjectName run.DatabaseName
        let storage = runStorage.create {
            RunDb = emptyDb()
            QueryParamsFromRunParams = fun rp output -> query (setupParameters.create rp.ParamMap) output
        }
        use cts = new CancellationTokenSource()
        let result = SetupExecutorCatalog.prepare sources run storage selectedRow (UMX.tag false) cts None |> Async.RunSynchronously
        Assert.Equal(Some ("Sources.SelectedTests", "Sources.SelectedEvaluations"), selected)
        Assert.True(Result.isError result)
