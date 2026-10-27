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
let ``initializing execution preserves recorded references and round trips through a path`` (generation: int) =
    let configured, saved = first ()
    // Rewriting the same GUID would normalize its spelling; execution initialization must leave it alone.
    let original =
        [ParamKeys.setupSorterPoolSetIdKey; ParamKeys.setupSortableTestsWithPrefixIdKey]
        |> List.fold (fun values key -> Map.add key (saved.ParamMap[key].ToUpperInvariant()) values) saved.ParamMap
        |> runParameters.create
        |> fun rp -> rp.WithGenerationCurrent(Some 250<generationNumber>).WithRunFinished(Some true)
    let path, _ = EvoPath.fromParameters configured original
    let start = { path.Start with FirstGeneration = UMX.tag generation }
    let updated = EvoStart.initializeExecution start original
    let reopened, progress = EvoPath.fromParameters configured updated
    let unchanged (rp: runParameters) =
        rp.ParamMap |> Map.filter (fun key _ -> not (List.contains key [ParamKeys.generationFirstKey; ParamKeys.generationCurrentKey; ParamKeys.runFinishedKey]))
    Assert.True(unchanged original = unchanged updated)
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
let ``typed result queries preserve saved parameter identity and separate activator rates`` () =
    let batch = prepared ()
    let query = QueryParamsCatalogRun.get batch.Run.RunQueryCatalogName batch.Run.ProjectName batch.Run.DatabaseName
    let mutable expectedRow = None
    let db = { new IGeneSortDb with
        member _.databaseName = batch.Run.DatabaseName
        member _.makeQueryParamsFromRunParams row output =
            Assert.Equal(expectedRow.Value, row)
            query row output
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected enumeration"
        member _.saveAsync _ _ _ _ = failwith "Unexpected write"
        member _.loadAsync _ _ = failwith "Unexpected read"
        member _.loadIfFoundAsync _ _ = failwith "Unexpected read"
        member _.doesOutPutDataExist _ _ = failwith "Unexpected probe" }
    for generation in [0<generationNumber>; 100<generationNumber>] do
        for output in [geneSortDataType.RunParameters batch.Run.RunName; geneSortDataType.SorterPoolSet "";
                       geneSortDataType.SorterPoolSetSummarySet ""; geneSortDataType.SorterPoolBinsSetSeries ""] do
            let ids = batch.Entries |> Array.map (fun (_, row) ->
                let path, state = EvoPath.fromParameters batch.Run row
                let saved = row.WithGenerationCurrent(Some generation)
                expectedRow <- Some saved
                let actual = EvolutionFiles.queryAtGeneration db path state generation output |> Option.get
                let expected = query saved output |> Option.get
                Assert.Equal(expected, actual)
                actual.Id)
            Assert.Equal(8, ids |> Array.distinct |> Array.length)

[<Fact>]
let ``typed execution receives the original database and exact start references`` () =
    let configured, row = first ()
    let observed = ResizeArray<geneSortDataMode * queryParams>()
    let db = { new IGeneSortDb with
        member _.databaseName = configured.DatabaseName
        member _.makeQueryParamsFromRunParams _ _ = failwith "Unexpected query"
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected enumeration"
        member _.saveAsync _ _ _ _ = failwith "Unexpected write"
        member _.loadAsync mode query = async { observed.Add(mode, query); return Error "Read observed" }
        member _.loadIfFoundAsync _ _ = failwith "Unexpected read"
        member _.doesOutPutDataExist _ _ = failwith "Unexpected probe"
    }
    let storage = runStorage.create { RunDb = db; QueryParamsFromRunParams = fun _ _ -> None }
    let request = queryParams.create configured.DatabaseName configured.ProjectName (Some 0<replNumber>) (Some 100<generationNumber>) (geneSortDataType.SorterPoolSet "") [|"Checkpoint", "100"|]
    let name = "evo-start-test-" + Guid.NewGuid().ToString("N")
    ExecuteFunctionCatalog.register name (SgdExecution (fun supplied _ path state _ _ _ -> async {
        let start = path.Start
        Assert.Same(db, supplied.RunDb)
        let! _ = supplied.RunDb.loadAsync geneSortDataMode.Setup start.Tests
        let! _ = supplied.RunDb.loadAsync geneSortDataMode.Setup start.Pool
        let! _ = supplied.RunDb.loadAsync geneSortDataMode.Results request
        return Ok state
    }))
    let configured = { RunDto.fromDomain configured with ExecuteFunctionName = name } |> RunDto.toDomain
    let path, progress = EvoPath.fromParameters configured row
    use cts = new CancellationTokenSource()
    ExecuteFunctionCatalog.bindPath storage path progress (UMX.tag false) cts None
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Equal<list<geneSortDataMode * queryParams>>(
        [geneSortDataMode.Setup, path.Start.Tests; geneSortDataMode.Setup, path.Start.Pool; geneSortDataMode.Results, request],
        List.ofSeq observed)

[<Fact>]
let ``typed executor returns only progress and retains fixed settings`` () =
    let configured, row = first ()
    let name = "evo-path-test-" + Guid.NewGuid().ToString("N")
    ExecuteFunctionCatalog.register name (SgdExecution (fun _ _ _ state _ _ _ -> async {
        return Ok (state.WithCurrentGeneration(Some 100<generationNumber>).WithFinished(Some true))
    }))
    let configured = { RunDto.fromDomain configured with ExecuteFunctionName = name } |> RunDto.toDomain
    let path, progress = EvoPath.fromParameters configured row
    let storage = runStorage.create { RunDb = unusedDb (); QueryParamsFromRunParams = fun _ _ -> None }
    use cts = new CancellationTokenSource()
    let updated = ExecuteFunctionCatalog.bindPath storage path progress (UMX.tag false) cts None |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(Some true, updated.Finished)
    Assert.Equal(Some 100<generationNumber>, updated.CurrentGeneration)
    Assert.Equal(row.GetActivatorMutationRate(), path.GetActivatorMutationRate())

[<Fact>]
let ``live execution retains the exact path and progress is separate`` () =
    let configured, row = first ()
    let start = (EvoPath.fromParameters configured row |> fst).Start
    let row = EvoStart.initializeExecution start row
    let name = "live-start-" + Guid.NewGuid().ToString("N")
    let mutable calls = 0
    let mutable expectedPath = None
    let mutable expectedProgress = None
    ExecuteFunctionCatalog.register name (SgdExecution (fun _ _ actual parameters _ _ _ -> async {
        Assert.Same(expectedPath.Value, actual)
        Assert.Same(expectedProgress.Value, parameters)
        Assert.Same(start, actual.Start)
        calls <- calls + 1
        return Ok (parameters.WithCurrentGeneration(Some 100<generationNumber>))
    }))
    let configured = { RunDto.fromDomain configured with ExecuteFunctionName = name; RunQueryCatalogName = "unavailable-live-query" } |> RunDto.toDomain
    let storage = runStorage.create { RunDb = unusedDb (); QueryParamsFromRunParams = fun _ _ -> failwith "Unexpected query" }
    let path, progress = EvoPath.fromStart configured start row
    expectedPath <- Some path
    expectedProgress <- Some progress
    let execute = ExecuteFunctionCatalog.bindPath storage
    use cts = new CancellationTokenSource()
    let updated = execute path progress (UMX.tag false) cts None |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Same(start, path.Start)
    Assert.Equal(Some 0<generationNumber>, progress.CurrentGeneration)
    Assert.Equal(Some 100<generationNumber>, updated.CurrentGeneration)
    Assert.Equal(row.GetId(), (EvoPath.toParameters path updated).GetId())
    // Inconsistent references are rejected when Setup constructs the path, before execution.
    let stale = row.WithSetupSorterPoolSetId(Some (UMX.tag (Guid.NewGuid())))
    Assert.Throws<ArgumentException>(fun () -> EvoPath.fromStart configured start stale |> ignore) |> ignore
    Assert.Equal(1, calls)
