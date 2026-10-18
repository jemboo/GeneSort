module GeneSort.Dispatch.Test.V2.WorkflowPhaseTests

open System
open System.Threading
open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Dispatch.V1.SorterSgd.Standard
open GeneSort.Dispatch.V1.SorterSgd.Soss
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.SortingLib.Sorter

let private parameters =
    (runParameters.create Map.empty)
        .WithSortingWidth(Some 2<sortingWidth>)
        .WithPartitionCount(Some 1<partitionCount>)
        .WithRngType(Some Lcg)
        .WithSeedSoss(Some (UMX.tag 1UL))
        .WithSorterEvalMeasure(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
        .WithGenerationCurrent(Some 7<generationNumber>)

let private intervals = { Name = "test"; Min = 0; MaxCount = Some 2; Scale = 1.0; Method = Constant 1 }
let private tests = sortableBinaryTests.create (UMX.tag Guid.Empty) 2<sortingWidth> [||] |> sortableTests.Bools
let private pool soss (generation: int) =
    let standard = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag generation) (Dim1 1) None
    if soss then SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
    else sorterPoolSet.Standard standard

type private MemoryDb(?buildQuery: runParameters -> geneSortDataType -> queryParams option) =
    let data = System.Collections.Generic.Dictionary<geneSortDataMode * Guid, outputData>()
    let reads = ResizeArray<geneSortDataMode * geneSortDataType>()
    let writes = ResizeArray<geneSortDataMode * geneSortDataType>()
    let poolGenerations = ResizeArray<int option * int>()
    member _.Reads = reads
    member _.Writes = writes
    member _.PoolGenerations = poolGenerations
    interface IGeneSortDb with
        member _.databaseName = UMX.tag "phase-test"
        member _.makeQueryParamsFromRunParams rp dataType =
            match buildQuery with
            | Some build -> build rp dataType
            | None -> Some (queryParams.create (UMX.tag "test") (UMX.tag "test") None (rp.GetGenerationCurrent()) dataType [||])
        member _.saveAsync mode query value _ = async {
            writes.Add(mode, query.OutputDataType)
            match value with
            | outputData.SorterPoolSet (sorterPoolSet.Soss pool) -> poolGenerations.Add(query.Generation |> Option.map int, int pool.GenerationNumber)
            | _ -> ()
            data.[(mode, %query.Id)] <- value
            return Ok ()
        }
        member _.doesOutPutDataExist mode query = async { return data.ContainsKey(mode, %query.Id) }
        member _.loadAsync mode query = async {
            reads.Add(mode, query.OutputDataType)
            return match data.TryGetValue((mode, %query.Id)) with
                   | true, value -> Ok value
                   | _ -> Error "Missing prepared input"
        }
        member _.loadIfFoundAsync mode query = async {
            reads.Add(mode, query.OutputDataType)
            return match data.TryGetValue((mode, %query.Id)) with true, value -> Some value | _ -> None
        }
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected parameter read"

let private save (db: IGeneSortDb) mode (generation: int) dataType value =
    let rp = parameters.WithGenerationCurrent(Some (UMX.tag generation))
    let query = db.makeQueryParamsFromRunParams rp dataType |> Option.get
    db.saveAsync mode query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(2)>]
let ``Soss checkpoints label the produced generation and stop exactly at the target`` startingGeneration =
    let sourceRun = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32.Pool_4_Debug ()
    let host = runHost.Create sourceRun 1
    let rp = (Run.makeRunParameters 0<replNumber> 1<replNumber> sourceRun.ParameterSpans host.ParamMapRefiner |> Array.exactlyOne)
                 .WithGenerationIntervalLast(Some (UMX.tag 2))
    let mutator = host.ExecutionContext.MakeMutator.Value rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    let partitions = PartitionOfSortableTests.partition (fun _ -> 0) 1<sortableTestsCount> tests
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 2<sortingWidth> [||]
    let initial = pool true startingGeneration |> SorterPoolSet.asSoss |> Result.defaultWith failwith
    let memory = MemoryDb()
    let config = { intervals with MaxCount = Some 3 }
    let final = EvoOrch_Soss.runSossEvolutionAsync memory config config rp (UMX.tag false) initial partitions prefix mutator CancellationToken.None ignore
                |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(2, int final.GenerationNumber)
    Assert.Equal<(int option * int) list>([for gen in startingGeneration + 1 .. 2 -> Some gen, gen], List.ofSeq memory.PoolGenerations)

[<Theory>]
[<InlineData("valid")>]
[<InlineData("previous-save-convention")>]
[<InlineData("missing")>]
[<InlineData("wrong-generation")>]
[<InlineData("standard-pool")>]
let ``Soss append copies the selected source checkpoint and tests only after validation`` scenario =
    let sourceRun = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32.Pool_4_Debug ()
    let sourceHost = runHost.Create sourceRun 1
    let appendRun = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32_append.fromSource sourceRun 1 2
    let configured = runHost.Create appendRun 1
    let sourceMemory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    let destinationMemory = MemoryDb(configured.QueryParamsFromRunParams)
    let sourceDb = sourceMemory :> IGeneSortDb
    let host = configured.WithRunDb(destinationMemory)
    let rp = Run.makeRunParameters 0<replNumber> 1<replNumber> appendRun.ParameterSpans host.ParamMapRefiner |> Array.exactlyOne
    let generation = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value 1 |> Option.get
    let persist mode query data = sourceDb.saveAsync mode query data (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist geneSortDataMode.Project (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName) (outputData.Run sourceRun)
    let sourcePoolQuery = sourceHost.QueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get
    if scenario <> "missing" then
        let actualGeneration =
            match scenario with
            | "wrong-generation" -> generation + 2
            | "previous-save-convention" -> generation + 1
            | _ -> generation
        persist geneSortDataMode.Results sourcePoolQuery (outputData.SorterPoolSet (pool (scenario <> "standard-pool") actualGeneration))
    let sourceTestsQuery = sourceHost.QueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "") |> Option.get
    let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
    persist geneSortDataMode.Setup sourceTestsQuery (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    let originalWrites = sourceMemory.Writes.Count
    use cts = new CancellationTokenSource()
    let result = GeneSort.Dispatch.V1.SorterSgd.SossAppendSetup.prepare sourceDb host rp (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(originalWrites, sourceMemory.Writes.Count)
    if scenario <> "valid" && scenario <> "previous-save-convention" then
        Assert.True(Result.isError result)
        Assert.Empty(destinationMemory.Writes)
    else
        let updated = result |> Result.defaultWith failwith
        let importedGeneration = if scenario = "previous-save-convention" then generation + 1 else generation
        Assert.Equal(Some (UMX.tag<generationNumber> importedGeneration), updated.GetGenerationFirst())
        Assert.Equal(updated.GetGenerationFirst(), updated.GetGenerationCurrent())
        Assert.Equal(Some false, updated.GetRunFinished())
        Assert.Equal(2, destinationMemory.Writes.Count)
        Assert.All(destinationMemory.Writes, fun (mode, _) -> Assert.Equal(geneSortDataMode.Setup, mode))
        let saved = GeneSort.Dispatch.V1.SorterSgd.Utils.loadSetupOutputData host.RunDb updated (geneSortDataType.SorterPoolSet "")
                    |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
        Assert.Equal(importedGeneration, %saved.GenerationNumber)
        let loadedTests = GeneSort.Dispatch.V1.SorterSgd.Utils.loadSortableTestsWithPrefix host.RunDb updated |> Async.RunSynchronously
        Assert.True(Result.isOk loadedTests)
        let query = host.QueryParamsFromRunParams rp (geneSortDataType.SorterPoolSet "") |> Option.get
        let other = host.QueryParamsFromRunParams (rp.WithSourceGenerationInterval(Some 2)) (geneSortDataType.SorterPoolSet "") |> Option.get
        Assert.NotEqual(query.Id, other.Id)
        let evolvedGeneration = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value 2 |> Option.get
        let evolvedRp = updated.WithGenerationCurrent(Some (UMX.tag evolvedGeneration))
        let evolvedQuery = host.QueryParamsFromRunParams evolvedRp (geneSortDataType.SorterPoolSet "") |> Option.get
        host.RunDb.saveAsync geneSortDataMode.Results evolvedQuery (outputData.SorterPoolSet(pool true evolvedGeneration)) (UMX.tag false)
        |> Async.RunSynchronously |> Result.defaultWith failwith
        let resumed = GeneSort.Dispatch.V1.SorterSgd.Utils.loadHighestGenSorterPoolSetSoss host.GenSaveIntervals.Value host.RunDb updated
                      |> Async.RunSynchronously |> Result.defaultWith failwith |> Option.get
        Assert.Equal(evolvedGeneration, %resumed.GenerationNumber)
        let reports = GeneSort.Dispatch.V1.SorterSgd.Utils.loadAvailableSorterPoolSets host.GenSaveIntervals.Value host.RunDb (UMX.tag 0) updated cts.Token ignore
                      |> Async.RunSynchronously |> Seq.toArray
        Assert.Single(reports) |> ignore

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``standalone tests have no setup and generate Results during execution`` prefix =
    let memory = MemoryDb()
    let spec = if prefix then SortableTestsSpecsPrefix.Specs.Prefix_Test () else SortableTestsSpecsMerge.Specs.Merge_Test ()
    let host = (runHost.Create spec 1).WithRunDb(memory)
    let rp =
        let basic = (runParameters.create Map.empty).WithRepl(Some 0<replNumber>).WithSortableDataFormat(Some sortableDataFormat.BoolArray)
        if prefix then basic.WithPrefixLibId(Some (prefixLibId.create 8<sortingWidth> 3<stageLength> prefixLibVariant.PrefixA))
        else basic.WithMergeLibId(Some (mergeLibId.create 4<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA))
    use cts = new CancellationTokenSource()
    let prepared = host.SetupExecutor.Execute host rp (UMX.tag false) cts None |> Async.RunSynchronously
    match prepared with
    | Ok unchanged -> Assert.Equal(rp, unchanged)
    | Error error -> failwith error
    Assert.Empty(memory.Reads)
    Assert.Empty(memory.Writes)
    let executed = host.ExecuteFunction host.ExecutionContext rp (UMX.tag false) cts None |> Async.RunSynchronously
    match executed with
    | Ok finished -> Assert.Equal(Some true, finished.GetRunFinished())
    | Error error -> failwith error
    Assert.Empty(memory.Reads)
    Assert.Equal<(geneSortDataMode * geneSortDataType) list>(
        [geneSortDataMode.Results, geneSortDataType.SortableTests ""], List.ofSeq memory.Writes)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``evolution setup saves only Setup and reuses it on subsequent calls`` soss =
    let memory = MemoryDb()
    let db = memory :> IGeneSortDb
    let mutable testCalls, poolCalls = 0, 0
    let makeTests (rp: runParameters) = async {
        Assert.Equal(Some 0<generationNumber>, rp.GetGenerationCurrent())
        testCalls <- testCalls + 1
        return Ok (tests, [||])
    }
    let makePool (rp: runParameters) = async {
        Assert.Equal(Some 0<generationNumber>, rp.GetGenerationCurrent())
        poolCalls <- poolCalls + 1
        return Ok (pool soss 0)
    }
    use cts = new CancellationTokenSource()
    let prepare = if soss then SgdEx_Soss.setupEvolutionRunSoss else SgdEx_Standard.setupEvolutionRunStandard
    for _ in 1..2 do
        let result = prepare makeTests makePool db intervals intervals parameters cts None |> Async.RunSynchronously
        match result with
        | Ok updated -> Assert.Same(parameters, updated)
        | Error error -> failwith error
    Assert.Equal(1, testCalls)
    Assert.Equal(1, poolCalls)
    Assert.Equal<list<geneSortDataMode * geneSortDataType>>(
        [geneSortDataMode.Setup, geneSortDataType.SortableTestsWithPrefix ""; geneSortDataMode.Setup, geneSortDataType.SorterPoolSet ""],
        List.ofSeq memory.Writes)

[<Theory>]
[<InlineData(false, "setup")>]
[<InlineData(true, "setup")>]
[<InlineData(false, "results")>]
[<InlineData(true, "results")>]
[<InlineData(false, "missing")>]
[<InlineData(true, "missing")>]
let ``evolution run reads prepared inputs and prioritizes Results without setup writes`` soss source =
    let memory = MemoryDb()
    let db = memory :> IGeneSortDb
    save db geneSortDataMode.Setup 0 (geneSortDataType.SortableTestsWithPrefix "") (outputData.SortableTestsWithPrefix(tests, [||]))
    if source <> "missing" then
        save db geneSortDataMode.Setup 0 (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet(pool soss 0))
    if source = "results" then
        save db geneSortDataMode.Results 1 (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet(pool soss 1))
    memory.Writes.Clear()
    let mutable selectedGeneration = None
    let mutator (rp: runParameters) = async {
        selectedGeneration <- rp.GetGenerationCurrent()
        return Error "Stop before evolution"
    }
    use cts = new CancellationTokenSource()
    let run = if soss then SgdEx_Soss.evaluateEvolutionRunSoss else SgdEx_Standard.evaluateEvolutionRunStandard
    let result = run mutator db intervals intervals parameters (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(Error (if source = "missing" then "Missing prepared input" else "Stop before evolution"), result)
    Assert.Equal((match source with "results" -> Some 1<generationNumber> | "setup" -> Some 0<generationNumber> | _ -> None), selectedGeneration)
    Assert.Empty(memory.Writes)
    if source = "results" then
        Assert.DoesNotContain((geneSortDataMode.Setup, geneSortDataType.SorterPoolSet ""), memory.Reads)
