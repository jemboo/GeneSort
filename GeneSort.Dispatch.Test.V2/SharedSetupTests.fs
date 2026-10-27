module GeneSort.Dispatch.Test.V2.SharedSetupTests

open System
open System.IO
open System.Threading
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup.SorterEval
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterEval

let private get result = Result.defaultWith failwith result

let private configuration () =
    SimpleRun.create {
        ProjectName = UMX.tag "SharedSetupTests"
        DatabaseName = UMX.tag "Shared"
        RunName = UMX.tag "Evaluation"
        Description = "Small, self-contained setup reuse test"
        AllowOverwrite = false
        Setup = {
            FunctionName = SetupCatalogNames.standard
            QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
            EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
            ParameterSpans = [
                ParamKeys.sortingWidthKey, ["4"]
                ParamKeys.rngTypeKey, ["LCG"]
                ParamKeys.simpleSorterModelTypeKey, ["Msce"; "Msrs"]
                ParamKeys.sorterCountKey, ["2"]
            ]
        }
        Execution = {
            FunctionName = ExecuteFunctionNames.sorterEval
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
            ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V1"; "V2"]]
        }
    } |> run.SimpleRun

type private TestDatabase() =
    let root = Path.Combine(Path.GetTempPath(), "GeneSort-SharedSetup-" + Guid.NewGuid().ToString("N")) |> Path.GetFullPath
    let data = Path.Combine(root, "SharedSetupTests", "Shared", "Data")
    member _.Data = data
    member _.Open() : IGeneSortDb = new GeneSortDbMp(UMX.tag data, QueryCatalogNamesRun.sorterEvalStandard)
    member _.Files() = Directory.GetFiles(Path.Combine(data, "Setup"), "*.msgpack", SearchOption.AllDirectories) |> Array.sort
    interface IDisposable with
        member _.Dispose() =
            if Directory.Exists root then Directory.Delete(root, true)

let private withValue key value (rp: runParameters) = runParameters.create (rp.ParamMap.Add(key, value))

[<Fact>]
let ``parallel setup shares files across executions replicas and hosts and execution follows saved references`` () =
    MessagePackSetup.configure ()
    use storage = new TestDatabase()
    let configured = configuration ()
    let host = (runHost.Create configured 4).WithRunDb(storage.Open())
    let otherHost = (runHost.Create configured 4).WithRunDb(storage.Open())
    let parameters = host.MakeRunParameters 0<replNumber> 2<replNumber>
    Assert.Equal(8, parameters.Length)
    Assert.Equal(4, parameters |> Array.map (fun rp -> rp.GetSetupSorterModelSetId()) |> Array.distinct |> Array.length)
    Assert.Single(parameters |> Array.map (fun rp -> rp.GetSetupSortableTestsWithPrefixId()) |> Array.distinct) |> ignore
    use cts = new CancellationTokenSource()
    Array.append parameters parameters
    |> Array.mapi (fun index rp ->
        let owner = if index % 2 = 0 then host else otherHost
        owner.SetupExecutor rp (UMX.tag false) cts None)
    |> Async.Parallel |> Async.RunSynchronously |> Array.iter (get >> ignore)
    let files = storage.Files()
    Assert.Equal(5, files.Length)
    Assert.All(files, fun file -> Assert.DoesNotContain(string Path.DirectorySeparatorChar + "0" + string Path.DirectorySeparatorChar, file))
    let stamps = files |> Array.map File.GetLastWriteTimeUtc
    for rp in parameters do
        host.SetupExecutor rp (UMX.tag true) cts None |> Async.RunSynchronously |> get |> ignore
        let finished = host.ExecuteFunction rp (UMX.tag false) cts None |> Async.RunSynchronously |> get
        Assert.Equal(Some true, finished.GetRunFinished())
    Assert.Equal<string>(files :> seq<string>, storage.Files() :> seq<string>)
    Assert.Equal<DateTime>(stamps :> seq<DateTime>, files |> Array.map File.GetLastWriteTimeUtc :> seq<DateTime>)
    // Execution needs no generator settings to resolve a recorded setup reference.
    let recorded = SetupFileReferences.fromRunParameters parameters.[0] |> Map.ofList |> runParameters.create
    for output in [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""] do
        let query = SetupInputReferences.tryRecordedInput host.Run.ProjectName host.Run.DatabaseName recorded output |> Option.get
        host.RunDb.loadAsync geneSortDataMode.Setup query |> Async.RunSynchronously |> get |> ignore
        let changed = parameters.[0].WithSorterCount(Some 99<sorterCount>).WithRepl(Some 100<replNumber>)
        let requested = host.QueryParamsFromRunParams changed output |> Option.get
        let resolved = SetupInputReferences.resolveRecorded changed requested
        Assert.Equal(query.Id, resolved.Id)

[<Theory>]
[<InlineData("SorterCount", "3")>]
[<InlineData("SortingWidth", "8")>]
[<InlineData("RngType", "SplitMix64")>]
[<InlineData("SimpleSorterModelType", "Mssi")>]
[<InlineData("ExcludeSelfCe", "false")>]
[<InlineData("AMax", "3")>]
[<InlineData("ActivationProbability", "0.5")>]
let ``uniform models distinguish every generation input`` key value =
    let host = runHost.Create (configuration ()) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let query (parameters: runParameters) = QueryParamsCatalogSetup.get host.Run.SetupQueryCatalogName host.Run.ProjectName host.Run.DatabaseName (setupParameters.create parameters.ParamMap) (geneSortDataType.SorterModelSet "") |> Option.get
    Assert.NotEqual((query rp).Id, (query (withValue key value rp)).Id)
    Assert.NotEqual((query rp).Id, (query (rp.WithRepl(Some 1<replNumber>))).Id)

[<Fact>]
let ``different model counts produce separate files with the requested contents`` () =
    MessagePackSetup.configure ()
    use storage = new TestDatabase()
    let host = (runHost.Create (configuration ()) 1).WithRunDb(storage.Open())
    let original = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head |> SetupFileReferences.attach []
    for count in [2; 3] do
        let settings = original.WithSorterCount(Some (UMX.tag count))
        let rp = SetupInputReferences.recordFiles (SetupOutputs.generatedFiles host.Run settings) settings
        SorterModelSetMakers.makeUniformSorterModelSet host.Storage (UMX.tag false) rp |> Async.RunSynchronously |> get
        let request = host.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "") |> Option.get
        let query = SetupInputReferences.resolveRecorded rp request
        let models = host.RunDb.loadAsync geneSortDataMode.Setup query |> Async.RunSynchronously |> get |> OutputData.asSorterModelSet |> get
        Assert.Equal(count, models.SorterModels.Length)
    Assert.Equal(2, storage.Files().Length)

[<Fact>]
let ``Merge tests share across models and replicas while library and format differences stay separate`` () =
    let host = runHost.Create (SorterEval.RunCfg.Merge.Test()) 1
    let parameters = host.MakeRunParameters 0<replNumber> 2<replNumber>
    let query (rp: runParameters) = QueryParamsCatalogSetup.get host.Run.SetupQueryCatalogName host.Run.ProjectName host.Run.DatabaseName (setupParameters.create rp.ParamMap) (geneSortDataType.SortableTestsWithPrefix "") |> Option.get
    let groups = parameters |> Array.groupBy (fun rp -> rp.GetMergeLibId(), rp.GetSortableDataFormat())
    Assert.Equal(groups.Length, parameters |> Array.map (query >> fun q -> q.Id) |> Array.distinct |> Array.length)
    for _, group in groups do Assert.Single(group |> Array.map (query >> fun q -> q.Id) |> Array.distinct) |> ignore
    let rp = parameters.[0]
    Assert.NotEqual((query rp).Id, (query (rp.WithSortableDataFormat(Some sortableDataFormat.BoolArray))).Id)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``seed pools share execution-only settings but retain family capacity mutation and activation inputs`` soss =
    let configured = if soss then WorkflowFixtures.sossSource [0] else WorkflowFixtures.standardSource ()
    let host = runHost.Create configured 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let query catalog (parameters: runParameters) = QueryParamsCatalogSetup.get catalog configured.ProjectName configured.DatabaseName (setupParameters.create parameters.ParamMap) (geneSortDataType.SorterPoolSet "") |> Option.get
    let baseline = query configured.SetupQueryCatalogName rp
    Assert.Equal(Some baseline.Id, rp.GetSetupSorterPoolSetId())
    for key, value in [ParamKeys.modificationRateKey, "0.5"; ParamKeys.activatorMutationRateKey, "0.9"; ParamKeys.partitionCountKey, "99"; ParamKeys.seedSossKey, "77"; ParamKeys.runNameKey, "AnotherRun"] do
        Assert.Equal(baseline.Id, (query configured.SetupQueryCatalogName (withValue key value rp)).Id)
    for key, value in [ParamKeys.sorterCountPerPoolKey, "99"; ParamKeys.mutationModKey, "99"; ParamKeys.aMaxKey, "9"; ParamKeys.activationProbabilityKey, "0.25"] do
        Assert.NotEqual(baseline.Id, (query configured.SetupQueryCatalogName (withValue key value rp)).Id)
    let otherFamily = if soss then QueryCatalogNamesSetup.sorterSgdMsufStandard else QueryCatalogNamesSetup.sossMsuf32Seed
    Assert.NotEqual(baseline.Id, (query otherFamily rp).Id)

[<Fact>]
let ``imported files share a physical path across replicas but source IDs and transformations stay separate`` () =
    use storage = new TestDatabase()
    let sourceId = UMX.tag<queryParamsId> (Guid.NewGuid())
    let imported (replica: int) source = queryParams.createForImportedFile (UMX.tag "Shared") (UMX.tag "SharedSetupTests") (Some (UMX.tag replica)) (geneSortDataType.SorterPoolSet "") source
    let path query = OutputDataFile.getFullOutputDataFilePath (UMX.tag storage.Data) geneSortDataMode.Setup query |> UMX.untag
    Assert.Equal(path (imported 0 sourceId), path (imported 7 sourceId))
    Assert.Equal(sourceId, (imported 0 sourceId).Id)
    Assert.Equal(sourceId, (imported 7 (imported 0 sourceId).Id).Id)
    Assert.NotEqual<string>(path (imported 0 sourceId), path (imported 0 (UMX.tag (Guid.NewGuid()))))
    let converted = queryParams.create (UMX.tag "Shared") (UMX.tag "SharedSetupTests") None None (geneSortDataType.SorterPoolSet "") [|"SourceFileId", string sourceId; "ExecutionFunction", ExecuteFunctionNames.soss|]
    Assert.NotEqual<string>(path (imported 0 sourceId), path converted)

[<Fact>]
let ``concurrent file publication retains one complete source and another database stays independent`` () =
    MessagePackSetup.configure ()
    use storage = new TestDatabase()
    use otherStorage = new TestDatabase()
    let host = runHost.Create (configuration ()) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let query = host.QueryParamsFromRunParams rp (geneSortDataType.SortableTestsWithPrefix "") |> Option.get
    let tests = SortableTestsMakers.makeStandardTests rp |> Async.RunSynchronously |> get |> outputData.SortableTestsWithPrefix
    [|for _ in 1 .. 12 -> OutputDataFile.saveToFileAsync (UMX.tag storage.Data) geneSortDataMode.Setup query tests (UMX.tag false)|]
    |> Async.Parallel |> Async.RunSynchronously |> Array.iter get
    Assert.Single(storage.Files()) |> ignore
    storage.Open().loadAsync geneSortDataMode.Setup query |> Async.RunSynchronously |> get |> ignore
    Assert.False(otherStorage.Open().doesOutPutDataExist geneSortDataMode.Setup query |> Async.RunSynchronously)
    Assert.Empty(Directory.GetFiles(storage.Data, "*.tmp", SearchOption.AllDirectories))
