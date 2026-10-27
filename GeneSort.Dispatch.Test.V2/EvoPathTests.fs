module GeneSort.Dispatch.Test.V2.EvoPathTests

open System
open System.Threading
open FSharp.UMX
open Xunit
open MessagePack
open GeneSort.Core.Mp
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.Mp.V1

let private configured () =
    let dto = WorkflowFixtures.sossSource [0; 1; 2; 3] |> RunDto.fromDomain
    { dto with
        SetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32S
        RunEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32S
        SetupParameterSpans = Array.append dto.SetupParameterSpans [|{ Key = ParamKeys.activationProbabilityKey; Values = [|"0.95"|] }|]
        RunParameterSpans = Array.append dto.RunParameterSpans [|{ Key = ParamKeys.activatorMutationRateKey; Values = [|"0"; "0.01"|] }|]
    } |> RunDto.toDomain

let private prepared () = RunPreparation.prepare { Run = configured (); FirstReplica = 0; ReplicaCount = 1 }
let private first () =
    let batch = prepared ()
    batch.Run, snd batch.Entries[0]

[<Theory>]
[<InlineData(0)>]
[<InlineData(100)>]
let ``attaching a prepared start preserves execution identity and round trips through a path`` (generation: int) =
    let configured, original = first ()
    let path, _ = EvoPath.fromParameters configured original
    let start = { path.Start with FirstGeneration = UMX.tag generation }
    let updated = EvoStart.attach start original
    let reopened, progress = EvoPath.fromParameters configured updated
    Assert.Equal(original.GetId(), updated.GetId())
    Assert.Equal(original.GetMutationMod(), updated.GetMutationMod())
    Assert.Equal(start.Pool.Id, reopened.Start.Pool.Id)
    Assert.Equal(start.Tests.Id, reopened.Start.Tests.Id)
    Assert.Equal(start.FirstGeneration, reopened.Start.FirstGeneration)
    Assert.Equal(Some start.FirstGeneration, progress.CurrentGeneration)
    Assert.Equal(Some false, progress.Finished)
    Assert.Equal(updated, EvoPath.toParameters reopened progress)

[<Fact>]
let ``eight paths have separate identities and share five setup inputs`` () =
    let batch = prepared ()
    let paths = batch.EvolutionPaths |> Array.map (function Ready (path, _) -> path | AwaitingSetup _ -> failwith "Unexpected unresolved path")
    Assert.Equal(8, paths.Length)
    Assert.Equal(8, paths |> Array.distinctBy (fun path -> path.Id) |> Array.length)
    let inputs = paths |> Array.collect (fun path -> path.SetupInputs.Values |> Seq.toArray)
    Assert.Equal(16, inputs.Length)
    let files = inputs |> Array.map SetupInputReferences.fileIdentity |> Set.ofArray
    Assert.Equal(5, files.Count)
    Assert.True((files = (batch.SetupFiles |> Array.map SetupInputReferences.fileIdentity |> Set.ofArray)))
    for _, pair in paths |> Array.groupBy (fun path -> path.Settings[ParamKeys.mutationModKey]) do
        Assert.Equal(2, pair.Length)
        Assert.NotEqual<string>(pair[0].Settings[ParamKeys.activatorMutationRateKey], pair[1].Settings[ParamKeys.activatorMutationRateKey])
        Assert.Equal((EvoPath.setupQuery (geneSortDataType.SorterPoolSet "") pair[0]).Id, (EvoPath.setupQuery (geneSortDataType.SorterPoolSet "") pair[1]).Id)

[<Fact>]
let ``saved parameter bytes round trip including optional state and original spelling`` () =
    MessagePackSetup.configure ()
    let configured, original = first ()
    for values in [original.ParamMap; original.ParamMap.Remove(ParamKeys.generationCurrentKey).Remove(ParamKeys.runFinishedKey)
                   original.ParamMap.Add(ParamKeys.generationCurrentKey, "000").Add(ParamKeys.runFinishedKey, "false").Add(ParamKeys.elapsedTimeKey, "1:23").Add(ParamKeys.messageKey, "resumed")] do
        let row = runParameters.create values
        let path, progress = EvoPath.fromParameters configured row
        let restored = EvoPath.toParameters path progress
        Assert.Equal(row, restored)
        let bytes rp = MessagePackSerializer.Serialize<runParametersDto>(RunParametersDto.fromDomain rp)
        Assert.Equal<byte>(bytes row, bytes restored)
        for key in [ParamKeys.generationCurrentKey; ParamKeys.runFinishedKey; ParamKeys.elapsedTimeKey; ParamKeys.messageKey] do
            Assert.False(path.Settings.ContainsKey key)

[<Fact>]
let ``advancing progress leaves path and all setup bindings unchanged`` () =
    let configured, original = first ()
    let path, progress = EvoPath.fromParameters configured original
    let advanced = progress.WithCurrentGeneration(Some 100<generationNumber>).WithFinished(Some true).WithMessage(Some "done")
    let updated = EvoPath.toParameters path advanced
    let reopened, reopenedProgress = EvoPath.fromParameters configured updated
    Assert.Equal(path.Id, reopened.Id)
    Assert.True(path.Settings = reopened.Settings)
    Assert.True(path.SetupInputs = reopened.SetupInputs)
    Assert.Equal(Some 100<generationNumber>, reopenedProgress.CurrentGeneration)
    let query = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
    Assert.Equal((query original (geneSortDataType.RunParameters configured.RunName) |> Option.get).Id,
                 (query updated (geneSortDataType.RunParameters configured.RunName) |> Option.get).Id)
    Assert.Throws<ArgumentException>(fun () -> EvoPath.progressFromParameters path (updated.WithActivatorMutationRate(Some 0.9)) |> ignore) |> ignore

[<Fact>]
let ``saved inputs reopen after their setup catalog is retired`` () =
    let configured, original = first ()
    let dto = RunDto.fromDomain configured
    let retired = { dto with SetupQueryCatalogName = "retired.setup.catalog" } |> RunDto.toDomain
    let path, _ = EvoPath.fromParameters retired original
    Assert.Equal(original.GetSetupSorterPoolSetId().Value, (EvoPath.setupQuery (geneSortDataType.SorterPoolSet "") path).Id)
    Assert.Equal(original.GetSetupSortableTestsWithPrefixId().Value, (EvoPath.setupQuery (geneSortDataType.SortableTestsWithPrefix "") path).Id)

[<Fact>]
let ``continuation origin belongs to the path and must be resolved by setup`` () =
    let configured, original = first ()
    let continuation = Run.withParameterSource configured [] configured
    Assert.Equal(AwaitingSetup (original.GetId().Value), EvoPath.prepare continuation original)
    Assert.Throws<ArgumentException>(fun () -> EvoPath.fromParameters continuation original |> ignore) |> ignore
    // Setup supplies the validated import origin alongside the concrete input IDs.
    let imported = original.WithGenerationFirst(Some 100<generationNumber>).WithGenerationCurrent(Some 100<generationNumber>)
    let path, progress = EvoPath.fromParameters continuation imported
    Assert.Equal("100", path.Settings[ParamKeys.generationFirstKey])
    let advanced = EvoPath.toParameters path (progress.WithCurrentGeneration(Some 150<generationNumber>))
    Assert.Equal(Some 100<generationNumber>, advanced.GetGenerationFirst())
    let legacy =
        imported.ParamMap.Remove(ParamKeys.setupSorterPoolSetIdKey).Remove(ParamKeys.setupSortableTestsWithPrefixIdKey)
            .Add("SourcePoolFileId", string (imported.GetSetupSorterPoolSetId().Value))
            .Add("SourceTestsFileId", string (imported.GetSetupSortableTestsWithPrefixId().Value))
        |> runParameters.create
    Assert.Equal(AwaitingSetup (legacy.GetId().Value), EvoPath.prepare continuation legacy)
    Assert.Throws<ArgumentException>(fun () -> EvoPath.fromParameters continuation legacy |> ignore) |> ignore
    Assert.Equal(100, int path.Start.FirstGeneration)
    Assert.Equal(imported.GetSetupSorterPoolSetId().Value, path.Start.Pool.Id)

let private unusedDb () =
    { new IGeneSortDb with
        member _.databaseName = UMX.tag "Unused"
        member _.makeQueryParamsFromRunParams _ _ = failwith "Unexpected query"
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected enumeration"
        member _.saveAsync _ _ _ _ = failwith "Unexpected write"
        member _.loadAsync _ _ = failwith "Unexpected read"
        member _.loadIfFoundAsync _ _ = failwith "Unexpected read"
        member _.doesOutPutDataExist _ _ = failwith "Unexpected probe" }

[<Fact>]
let ``path storage uses planned setup inputs but leaves result queries untouched`` () =
    let configured, row = first ()
    let path, _ = EvoPath.fromParameters configured row
    let observed = ResizeArray<geneSortDataMode * queryParams>()
    let db = { new IGeneSortDb with
        member _.databaseName = configured.DatabaseName
        member _.makeQueryParamsFromRunParams _ _ = None
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected enumeration"
        member _.saveAsync _ _ _ _ = failwith "Unexpected write"
        member _.loadAsync mode query = async { observed.Add(mode, query); return Error "Read observed" }
        member _.loadIfFoundAsync _ _ = failwith "Unexpected read"
        member _.doesOutPutDataExist _ _ = failwith "Unexpected probe"
    }
    let storage = RunStorage.forPath path (runStorage.create { RunDb = db; QueryParamsFromRunParams = fun _ _ -> None })
    let request = queryParams.create configured.DatabaseName configured.ProjectName (Some 0<replNumber>) (Some 100<generationNumber>) (geneSortDataType.SorterPoolSet "") [|"Checkpoint", "100"|]
    storage.RunDb.loadAsync geneSortDataMode.Setup request |> Async.RunSynchronously |> ignore
    storage.RunDb.loadAsync geneSortDataMode.Results request |> Async.RunSynchronously |> ignore
    Assert.Equal((EvoPath.setupQuery request.OutputDataType path).Id, (snd observed[0]).Id)
    Assert.Equal(request, snd observed[1])

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``typed executor returns progress and rejects changes to settings`` changeSettings =
    let configured, row = first ()
    let name = "evo-path-test-" + Guid.NewGuid().ToString("N")
    ExecuteFunctionCatalog.register name (SgdExecution (fun _ _ rp _ _ _ -> async {
        return Ok (if changeSettings then rp.WithActivatorMutationRate(Some 0.9) else rp.WithGenerationCurrent(Some 100<generationNumber>).WithRunFinished(Some true))
    }))
    let configured = { RunDto.fromDomain configured with ExecuteFunctionName = name } |> RunDto.toDomain
    let path, progress = EvoPath.fromParameters configured row
    let storage = runStorage.create { RunDb = unusedDb (); QueryParamsFromRunParams = fun _ _ -> None }
    use cts = new CancellationTokenSource()
    let result = ExecuteFunctionCatalog.bindPath storage path progress (UMX.tag false) cts None |> Async.RunSynchronously
    match result, changeSettings with
    | Ok updated, false -> Assert.Equal(Some true, updated.Finished)
    | Error message, true -> Assert.Contains("changed the evolutionary specification", message)
    | _ -> failwithf "Unexpected result: %A" result
