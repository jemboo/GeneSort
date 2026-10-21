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
open GeneSort.Project.V1.Parameters
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.V1.Simple.Ce

let private sourceWithMutationMods modifiers =
    let baseline = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss.Pool_32_Debug ()
    SgdRun.create {
        DatabaseName = baseline.DatabaseName
        ProjectName = baseline.ProjectName
        RunName = baseline.RunName
        Description = baseline.Description
        AllowOverwrite = false
        Setup = {
            FunctionName = baseline.SetupFunctionName
            ParameterSpans = baseline.SetupParameterSpans |> List.map (fun (key, values) ->
                if key = ParamKeys.mutationModKey then key, List.map string modifiers else key, values)
            QueryCatalogName = baseline.SetupQueryCatalogName
            EnhancerFunctionName = baseline.SetupEnhancerFunctionName
        }
        Execution = {
            FunctionName = baseline.ExecuteFunctionName
            ParameterSpans = baseline.RunParameterSpans
            QueryCatalogName = baseline.RunQueryCatalogName
            EnhancerFunctionName = baseline.RunEnhancerFunctionName
        }
    } {
        MakerNames = baseline.MakerNames.Value
        GenSaveIntervalsName = baseline.GenSaveIntervalsName.Value
        GenSaveSubIntervalsName = baseline.GenSaveSubIntervalsName.Value
    } |> run.SgdRun

let private singleSource () = sourceWithMutationMods [0]

let private appendFrom (source: run) interval lastInterval selection execution =
    GeneSort.Dispatch.V1.SorterSgd.EvoAppend.create {
        ProjectName = UMX.tag (sprintf "Tests.Append%d" interval)
        DatabaseName = source.DatabaseName
        RunName = UMX.tag "Append"
        Source = { Run = source; GenerationInterval = interval; ParameterSpans = selection }
        ExecutionParameterSpans = (ParamKeys.generationIntervalLastKey, [string lastInterval]) :: execution
    }

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
    let mutable savedParameters: runParameters array = [||]
    let data = System.Collections.Generic.Dictionary<geneSortDataMode * Guid, outputData>()
    let reads = ResizeArray<geneSortDataMode * geneSortDataType>()
    let writes = ResizeArray<geneSortDataMode * geneSortDataType>()
    let poolGenerations = ResizeArray<int option * int>()
    member _.Reads = reads
    member _.Writes = writes
    member _.PoolGenerations = poolGenerations
    member _.SavedParameters with get() = savedParameters and set value = savedParameters <- value
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
            | outputData.SorterPoolSet (sorterPoolSet.Standard pool) -> poolGenerations.Add(query.Generation |> Option.map int, int pool.GenerationNumber)
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
        member _.getRunParameters _ _ _ _ _ = async { return Ok savedParameters }

let private save (db: IGeneSortDb) mode (generation: int) dataType value =
    let rp = parameters.WithGenerationCurrent(Some (UMX.tag generation))
    let query = db.makeQueryParamsFromRunParams rp dataType |> Option.get
    db.saveAsync mode query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith

[<Fact>]
let ``Pool 4 debug generates split parameters and completes setup and execution`` () =
    let configured = singleSource ()
    let host = runHost.Create configured 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    Assert.Equal(Some 4<sorterCountPerPool>, setup.GetSorterCountPerPool())
    Assert.Equal(Some 8<sorterPoolCount>, setup.GetSorterPoolCount())
    Assert.Equal(setup.GetSorterPoolCount(), rp.GetSorterPoolCount())
    Assert.Equal(setup.GetPrefixLibId(), rp.GetPrefixLibId())
    Assert.True(setup.GetModificationRate().IsNone)
    Assert.True(setup.GetPartitionCount().IsNone)
    Assert.Equal(Some 2<partitionCount>, rp.GetPartitionCount())
    Assert.True(rp.GetAdjustCeLengthFrequency().IsNone)
    Assert.Empty(SetupFileReferences.fromRunParameters rp)
    Assert.Empty(SetupFileReferences.fromSetupParameters setup)
    // Use tiny in-memory inputs to exercise the complete phase boundary without external databases.
    let memory = MemoryDb(host.QueryParamsFromRunParams)
    let bound = host.WithRunDb(memory)
    let tinyTests =
        [| sortableBoolArray.create(Array.create 32 false, 32<sortingWidth>)
           sortableBoolArray.create(Array.create 32 true, 32<sortingWidth>) |]
        |> sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
        |> sortableTests.Bools
    use cts = new CancellationTokenSource()
    SgdEx_Soss.setupEvolutionRunSoss (fun _ -> async { return Ok (tinyTests, [||]) })
        (fun _ -> async { return Ok (pool true 0) }) memory
        host.GenSaveIntervals.Value host.GenSaveSubIntervals.Value rp cts None
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    bound.ExecuteFunction bound.ExecutionContext rp (UMX.tag false) cts None
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Contains(memory.Writes, fun (mode, outputType) ->
        mode = geneSortDataMode.Results && outputType = geneSortDataType.SorterPoolSet "")

[<Theory>]
[<InlineData(true, true)>]
[<InlineData(true, false)>]
[<InlineData(false, true)>]
[<InlineData(false, false)>]
let ``Soss adjusts only on subset transitions and uses merged evaluations`` changed otherPartitionUnsorted =
    let host = runHost.Create (singleSource ()) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let mutator = host.ExecutionContext.MakeMutator.Value rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    let makeTests values =
        sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
            [| sortableBoolArray.create(values, 32<sortingWidth>) |] |> sortableTests.Bools
    let partitions =
        [ UMX.tag "0", makeTests (Array.init 32 (fun i -> otherPartitionUnsorted && i < 16))
          UMX.tag "1", makeTests (Array.create 32 false) ]
        |> Map.ofList |> partitionOfSortableTests.create
    let model =
        msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 150 1)
        |> simpleSorterModel.Msce
        |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let memberValue =
        spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
        |> SpMember_Soss.withSubsetIdForSelection (UMX.tag (if changed then "0" else "1"))
    let poolId = UMX.tag (Guid.NewGuid())
    let inputPool = sorterPool_Soss.create poolId None (UMX.tag "test") (D1 0) [|memberValue|] 150<ceLength> (UMX.tag 0)
    let input = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 1<generationNumber> (Dim1 1) (Some [inputPool])
    let result =
        Pipeline_Soss.runGenerationStepSoss mutator 1<sorterCountPerPool> 1<sorterCountPerPool> 0<sorterChildCount>
            (UMX.tag false) (UMX.tag false) partitions 1<generationsPerSubset> 1<generationsPerSubset>
            (ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]) sorterEvalType.V2
            (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength)
            input false (UMX.tag false) 1.0<sortedFraction>
    let resultPool = result.SorterPools.[poolId]
    let survivor = resultPool.SorterPoolMembers |> Seq.exactlyOne
    Assert.True(survivor.SubsetIdForSelection = UMX.tag<sortableTestsSubsetId> "1")
    Assert.Equal(changed, Map.containsKey CommonSgdSoss.mergedSubsetId survivor.SorterEvalMap)
    if changed && not otherPartitionUnsorted then
        Assert.True(resultPool.RawCeLength < inputPool.RawCeLength)
    else
        Assert.Equal(inputPool.RawCeLength, resultPool.RawCeLength)

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(2)>]
let ``Soss checkpoints label the produced generation and stop exactly at the target`` startingGeneration =
    let sourceRun = singleSource ()
    let host = runHost.Create sourceRun 1
    let rp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne)
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
[<InlineData(false)>]
[<InlineData(true)>]
let ``Append branches execution modifiers from one source checkpoint`` chained =
    let baseline = singleSource ()
    let original =
        SgdRun.create {
            DatabaseName = baseline.DatabaseName
            ProjectName = baseline.ProjectName
            RunName = baseline.RunName
            Description = baseline.Description
            AllowOverwrite = false
            Setup = {
                FunctionName = baseline.SetupFunctionName
                ParameterSpans = baseline.SetupParameterSpans |> List.map (fun (key, values) ->
                    if key = ParamKeys.mutationModKey then key, ["0"; "1"; "2"] else key, values)
                QueryCatalogName = baseline.SetupQueryCatalogName
                EnhancerFunctionName = baseline.SetupEnhancerFunctionName
            }
            Execution = {
                FunctionName = baseline.ExecuteFunctionName
                ParameterSpans = baseline.RunParameterSpans
                QueryCatalogName = baseline.RunQueryCatalogName
                EnhancerFunctionName = baseline.RunEnhancerFunctionName
            }
        } {
            MakerNames = baseline.MakerNames.Value
            GenSaveIntervalsName = baseline.GenSaveIntervalsName.Value
            GenSaveSubIntervalsName = baseline.GenSaveSubIntervalsName.Value
        } |> run.SgdRun
    let sourceRun =
        if chained then appendFrom original 1 2 [ParamKeys.mutationModKey, ["1"]] [ParamKeys.mutationModKey, ["4"; "5"]]
        else original
    let sourceMod = if chained then 4 else 1
    let interval = if chained then 2 else 1
    let sourceHost = runHost.Create sourceRun 1
    let destinationRun =
        appendFrom sourceRun interval (interval + 1) [ParamKeys.mutationModKey, [string sourceMod]] [ParamKeys.mutationModKey, ["7"; "8"]]
    let destinationHost = runHost.Create destinationRun 1
    Assert.Single(destinationHost.MakeSetupParameters 0<replNumber> 1<replNumber>) |> ignore
    let sourceParameters = sourceHost.MakeRunParameters 0<replNumber> 1<replNumber>
    Assert.Equal((if chained then 2 else 3), sourceParameters.Length)
    let sourceRp = sourceParameters |> Array.filter (fun rp -> rp.GetMutationMod() = Some (UMX.tag sourceMod)) |> Array.exactlyOne
    let branches = destinationHost.MakeRunParameters 0<replNumber> 1<replNumber>
    Assert.Equal(2, branches.Length)
    Assert.Equal(branches.Length, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let generation = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value interval |> Option.get
    let sourcePools =
        [| for index in 0 .. 1 ->
            let memberValue =
                spMember_Soss.create (UMX.tag (Guid.NewGuid())) GeneSort.Model.Sorting.V1.sorterModel.Unknown
                    (UMX.tag 12) (UMX.tag sourceMod) None Map.empty (UMX.tag generation)
            sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 index)
                [|memberValue|] 150<ceLength> (UMX.tag sourceMod) |]
    let sourcePoolSet =
        sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 2) (Some sourcePools)
    let sourceMemory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    sourceMemory.SavedParameters <- sourceParameters
    let sourceDb = sourceMemory :> IGeneSortDb
    let persist mode query data =
        sourceDb.saveAsync mode query data (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist geneSortDataMode.Project (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName) (outputData.Run sourceRun)
    persist geneSortDataMode.Results
        (sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get)
        (sourcePoolSet |> sorterPoolSet.Soss |> outputData.SorterPoolSet)
    let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
    persist geneSortDataMode.Setup
        (sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "") |> Option.get)
        (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    let destinationMemory = MemoryDb(destinationHost.QueryParamsFromRunParams)
    let host = destinationHost.WithRunDb destinationMemory
    use cts = new CancellationTokenSource()
    for rp in branches do
        let updated =
            GeneSort.Dispatch.V1.SorterSgd.EvoAppendSetup.prepare sourceDb host rp (UMX.tag false) cts None
            |> Async.RunSynchronously |> Result.defaultWith failwith
        let imported =
            GeneSort.Dispatch.V1.SorterSgd.Utils.loadSetupOutputData host.RunDb updated (geneSortDataType.SorterPoolSet "")
            |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
        Assert.Equal(2, imported.SorterPools.Count)
        Assert.Equal(sourcePoolSet, imported)
        let executionPool = SgdEx_Soss.prepareImportedPool updated imported |> Result.defaultWith failwith
        for importedPool in executionPool.SorterPools.Values do
            Assert.Equal(rp.GetMutationMod().Value, importedPool.MutationMod)
            Assert.True(sourcePools |> Array.exists (fun pool -> Some pool.SorterPoolId = importedPool.ParentSorterPoolId))
            let memberValue = importedPool.SorterPoolMembers |> Seq.exactlyOne
            Assert.Equal(rp.GetMutationMod().Value, memberValue.MutationMod)
            Assert.Equal(0, int memberValue.MutationIndex)
    Assert.Equal(3, sourceMemory.Writes.Count)
    Assert.Equal(2, destinationMemory.Writes.Count)

[<Theory>]
[<InlineData(true)>]
[<InlineData(false)>]
let ``Repeated append branches share immutable setup without growing parameter history`` soss =
    let mutable sourceRun = if soss then singleSource () else GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_128_Test ()
    let mutable sourceHost = runHost.Create sourceRun 1
    let mutable sourceRp = sourceHost.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let mutable sourceMemory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
    let persist (db: IGeneSortDb) mode query data =
        db.saveAsync mode query data (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist sourceMemory geneSortDataMode.Setup
        (sourceHost.QueryParamsFromRunParams sourceRp (geneSortDataType.SortableTestsWithPrefix "") |> Option.get)
        (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    use cts = new CancellationTokenSource()
    let mutable parameterCount = None
    for interval in 1 .. 4 do
        let sourceGeneration = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value interval |> Option.get
        sourceMemory.SavedParameters <- [|sourceRp|]
        persist sourceMemory geneSortDataMode.Project
            (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName) (outputData.Run sourceRun)
        let checkpointQuery = sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag sourceGeneration))) (geneSortDataType.SorterPoolSet "") |> Option.get
        persist sourceMemory geneSortDataMode.Results checkpointQuery (outputData.SorterPoolSet(pool soss sourceGeneration))
        let nextRun =
            GeneSort.Dispatch.V1.SorterSgd.EvoAppend.create {
                ProjectName = UMX.tag (sprintf "ExplicitChain%d" interval)
                DatabaseName = sourceRun.DatabaseName
                RunName = UMX.tag "Branch"
                Source = { Run = sourceRun; GenerationInterval = interval
                           ParameterSpans = [ParamKeys.idKey, [string %(sourceRp.GetId().Value)]] }
                ExecutionParameterSpans = [
                    ParamKeys.mutationModKey, [interval; interval + 10] |> List.map string
                    ParamKeys.generationIntervalLastKey, [string (interval + 1)]
                ]
            }
        let configured = runHost.Create nextRun 1
        let memory = MemoryDb(configured.QueryParamsFromRunParams)
        let host = configured.WithRunDb memory
        let prepared =
            host.MakeRunParameters 0<replNumber> 1<replNumber>
            |> Array.map (fun rp ->
                let metadataBefore = host.QueryParamsFromRunParams rp (geneSortDataType.RunParameters %nextRun.RunName) |> Option.get
                let updated =
                    GeneSort.Dispatch.V1.SorterSgd.EvoAppendSetup.prepare sourceMemory host rp (UMX.tag false) cts None
                    |> Async.RunSynchronously |> Result.defaultWith failwith
                let metadataAfter = host.QueryParamsFromRunParams updated (geneSortDataType.RunParameters %nextRun.RunName) |> Option.get
                Assert.Equal(metadataBefore.Id, metadataAfter.Id)
                let setupQuery =
                    host.QueryParamsFromRunParams updated (geneSortDataType.SorterPoolSet "") |> Option.get |> QueryParams.forSetup updated
                let expectedId = GuidUtils.guidFromObjs [box %nextRun.ProjectName; box %checkpointQuery.Id] |> UMX.tag<queryParamsId>
                Assert.Equal(expectedId, setupQuery.Id)
                updated)
        Assert.Equal(2, memory.Writes.Count)
        Assert.Equal(prepared.[0].ParamMap.[ParamKeys.setupSorterPoolSetIdKey], prepared.[1].ParamMap.[ParamKeys.setupSorterPoolSetIdKey])
        let resultIds = prepared |> Array.map (fun rp -> (host.QueryParamsFromRunParams rp (geneSortDataType.SorterPoolSet "") |> Option.get).Id)
        Assert.NotEqual(resultIds.[0], resultIds.[1])
        match parameterCount with
        | Some count -> Assert.Equal(count, prepared.[0].ParamMap.Count)
        | None -> parameterCount <- Some prepared.[0].ParamMap.Count
        sourceRun <- nextRun
        sourceHost <- host
        sourceRp <- prepared.[0]
        sourceMemory <- memory

[<Theory>]
[<InlineData(true)>]
[<InlineData(false)>]
let ``Append source branching shares setup in replica folders`` soss =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GeneSort-append-" + Guid.NewGuid().ToString("N"))
    let makeDb (configured: run) =
        let dataRoot = System.IO.Path.Combine(root, %configured.ProjectName, %configured.DatabaseName, "Data")
        dataRoot, (new GeneSort.FileDb.V1.GeneSortDbMp(UMX.tag dataRoot, configured.RunQueryCatalogName) :> IGeneSortDb)
    let persist (db: IGeneSortDb) mode query data =
        db.saveAsync mode query data (UMX.tag true) |> Async.RunSynchronously |> Result.defaultWith failwith
    try
        let sourceRun =
            if soss then sourceWithMutationMods [0; 1; 2]
            else
                let dto = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_128_Test () |> GeneSort.Project.Mp.V1.RunDto.fromDomain
                let spans = dto.SetupParameterSpans |> Array.map (fun span ->
                    if span.Key = ParamKeys.mutationModKey then { span with Values = [|"0"; "1"; "2"|] } else span)
                { dto with SetupParameterSpans = spans }
                |> GeneSort.Project.Mp.V1.RunDto.toDomain
        let sourcePoolCount = if soss then 8 else 4
        let perPool = if soss then 4 else 32
        let sourceHost = runHost.Create sourceRun 1
        let _, sourceDb = makeDb sourceRun
        let sourceParameters = sourceHost.MakeRunParameters 0<replNumber> 2<replNumber>
        let generation = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value 1 |> Option.get
        persist sourceDb geneSortDataMode.Project (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName) (outputData.Run sourceRun)
        let tests =
            sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
                [| sortableBoolArray.create(Array.create 32 false, 32<sortingWidth>)
                   sortableBoolArray.create(Array.create 32 true, 32<sortingWidth>) |]
            |> sortableTests.Bools
        for sourceRp in sourceParameters do
            let sourcePool =
                let pools =
                    [| for index in 0 .. sourcePoolCount - 1 ->
                        let members =
                            // A pruned source can be below its configured capacity of four.
                            [| for _ in 1 .. 3 ->
                                spMember_Standard.create (UMX.tag (Guid.NewGuid())) GeneSort.Model.Sorting.V1.sorterModel.Unknown
                                    (UMX.tag 12) (sourceRp.GetMutationMod().Value) None None (UMX.tag generation) |]
                        sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 index)
                            members 150<ceLength> (sourceRp.GetMutationMod().Value) |]
                let standard = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 sourcePoolCount) (Some pools)
                if soss then SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss else sorterPoolSet.Standard standard
            persist sourceDb geneSortDataMode.Project
                (sourceHost.QueryParamsFromRunParams sourceRp (geneSortDataType.RunParameters %sourceRun.RunName) |> Option.get) (outputData.RunParameters sourceRp)
            persist sourceDb geneSortDataMode.Results
                (sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get)
                (outputData.SorterPoolSet sourcePool)
            persist sourceDb geneSortDataMode.Setup
                (sourceHost.QueryParamsFromRunParams sourceRp (geneSortDataType.SortableTestsWithPrefix "") |> Option.get)
                (outputData.SortableTestsWithPrefix(tests, [||]))
        let configured =
            GeneSort.Dispatch.V1.SorterSgd.EvoAppend.create {
                ProjectName = UMX.tag "Tests.Append"
                DatabaseName = sourceRun.DatabaseName
                RunName = UMX.tag "ExplicitAppend"
                Source = { Run = sourceRun; GenerationInterval = 1; ParameterSpans = [ParamKeys.mutationModKey, ["0"; "1"; "2"]] }
                ExecutionParameterSpans = [
                    ParamKeys.mutationModKey, [0 .. 15] |> List.map string
                    ParamKeys.sorterCountPerPoolSetKey, [sourcePoolCount * perPool; sourcePoolCount * perPool * 2] |> List.map string
                    ParamKeys.generationIntervalLastKey, ["2"]
                ]
            }
        // Round-trip the entire source-generation configuration, including nested append sources.
        let configured = configured |> GeneSort.Project.Mp.V1.RunDto.fromDomain
                         |> MessagePack.MessagePackSerializer.Serialize
                         |> MessagePack.MessagePackSerializer.Deserialize<GeneSort.Project.Mp.V1.runDto>
                         |> GeneSort.Project.Mp.V1.RunDto.toDomain
        let expectedCount = 192
        let destinationRoot, db = makeDb configured
        let host = (runHost.Create configured 1).WithRunDb db
        let pending = host.MakeRunParameters 0<replNumber> 2<replNumber>
        Assert.Equal(expectedCount, pending.Length)
        Assert.Equal(expectedCount, pending |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
        for rp in pending do
            Assert.Contains(rp.ParamMap.[ParamKeys.sourceExecutionIdKey], sourceParameters |> Array.map (fun source -> string %(source.GetId().Value)))
            Assert.Equal(int (rp.GetSorterCountPerPoolSet().Value) / perPool, int (rp.GetSorterPoolCount().Value))
        for rp in pending do
            persist db geneSortDataMode.Project
                (host.QueryParamsFromRunParams rp (geneSortDataType.RunParameters %configured.RunName) |> Option.get) (outputData.RunParameters rp)
        use cts = new CancellationTokenSource()
        let invalid = pending.[0].WithSorterCountPerPool(Some 8<sorterCountPerPool>)
        let result = GeneSort.Dispatch.V1.SorterSgd.EvoAppendSetup.prepare sourceDb host invalid (UMX.tag false) cts None
                     |> Async.RunSynchronously
        Assert.True(Result.isError result)
        let prepared =
            pending |> Array.map (fun rp -> GeneSort.Dispatch.V1.SorterSgd.EvoAppendSetup.prepare sourceDb host rp (UMX.tag false) cts None)
            |> Async.Parallel |> Async.RunSynchronously |> Array.map (Result.defaultWith failwith)
        Assert.Equal(expectedCount, prepared |> Array.map (fun rp ->
            (host.QueryParamsFromRunParams rp (geneSortDataType.SorterPoolSet "") |> Option.get).Id) |> Array.distinct |> Array.length)
        for rp in prepared do
            persist db geneSortDataMode.Project
                (host.QueryParamsFromRunParams rp (geneSortDataType.RunParameters %configured.RunName) |> Option.get) (outputData.RunParameters rp)
        let _, reopened = makeDb configured
        let loaded = reopened.getRunParameters configured.RunName (Some 0<replNumber>) (Some 2<replNumber>) None None
                     |> Async.RunSynchronously |> Result.defaultWith failwith
        Assert.Equal(expectedCount, loaded.Length)
        let files = System.IO.Directory.GetFiles(System.IO.Path.Combine(destinationRoot, "Setup"), "*.msgpack", System.IO.SearchOption.AllDirectories)
        Assert.Equal(12, files.Length)
        Assert.All(files, fun path -> Assert.Contains(System.IO.Directory.GetParent(path).Name, [|"0"; "1"|]))
        Assert.All(loaded |> Array.groupBy (fun rp -> rp.GetRepl(), rp.ParamMap.[ParamKeys.setupSorterPoolSetIdKey]),
            fun (_, branches) -> Assert.Equal(32, branches.Length))
        for rp in loaded do
            let savedData = GeneSort.Dispatch.V1.SorterSgd.Utils.loadSetupOutputData reopened rp (geneSortDataType.SorterPoolSet "")
                            |> Async.RunSynchronously |> Result.defaultWith failwith
            if soss then
                let saved = OutputData.asSorterPoolSetSoss savedData |> Result.defaultWith failwith
                Assert.Equal(generation, int saved.GenerationNumber)
                let execution = SgdEx_Soss.prepareImportedPool rp saved |> Result.defaultWith failwith
                Assert.Equal(sourcePoolCount, saved.SorterPools.Count)
                Assert.Equal(int (rp.GetSorterPoolCount().Value), execution.SorterPools.Count)
                Assert.All(execution.SorterPools.Values, fun pool -> Assert.Equal(rp.GetMutationMod().Value, pool.MutationMod))
            else
                let saved = OutputData.asSorterPoolSetStandard savedData |> Result.defaultWith failwith
                Assert.Equal(generation, int saved.GenerationNumber)
                let execution = GeneSort.Dispatch.V1.SorterSgd.EvoAppendPools.prepareStandard rp saved |> Result.defaultWith failwith
                Assert.Equal(sourcePoolCount, saved.SorterPools.Count)
                Assert.Equal(int (rp.GetSorterPoolCount().Value), execution.SorterPools.Count)
                Assert.All(execution.SorterPools.Values, fun pool -> Assert.Equal(rp.GetMutationMod().Value, pool.MutationMod))
            GeneSort.Dispatch.V1.SorterSgd.Utils.loadSortableTestsWithPrefix reopened rp
            |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
        // A final Results checkpoint must bypass Setup preparation entirely, even when the checkpoint is empty (which fresh-branch preparation rejects).
        let rp = loaded.[0]
        let finalGeneration = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value 2 |> Option.get
        let resumedRp = rp.WithGenerationCurrent(Some (UMX.tag finalGeneration))
        let finalQuery = host.QueryParamsFromRunParams resumedRp (geneSortDataType.SorterPoolSet "") |> Option.get
        persist db geneSortDataMode.Results finalQuery (outputData.SorterPoolSet(pool soss finalGeneration))
        let result = host.ExecuteFunction host.ExecutionContext rp (UMX.tag false) cts None
                     |> Async.RunSynchronously |> Result.defaultWith failwith
        Assert.Equal(Some (UMX.tag<generationNumber> finalGeneration), result.GetGenerationCurrent())
    finally
        if System.IO.Directory.Exists root then System.IO.Directory.Delete(root, true)

[<Theory>]
[<InlineData("valid")>]
[<InlineData("previous-save-convention")>]
[<InlineData("missing")>]
[<InlineData("wrong-generation")>]
[<InlineData("standard-pool")>]
[<InlineData("append-source")>]
[<InlineData("retired-configuration")>]
[<InlineData("missing-source-id")>]
[<InlineData("invalid-source-id")>]
[<InlineData("unknown-source-id")>]
[<InlineData("duplicate-source-id")>]
let ``Soss append copies the selected source checkpoint and tests only after validation`` scenario =
    let chained = scenario = "append-source"
    let sourceRun =
        if chained then appendFrom (singleSource ()) 1 2 [] []
        else singleSource ()
    let sourceInterval = if chained then 2 else 1
    let lastInterval = sourceInterval + 1
    let sourceHost = runHost.Create sourceRun 1
    let appendRun = appendFrom sourceRun sourceInterval lastInterval [] []
    Assert.Equal(appendRun.RunParameterSpans.Length, appendRun.RunParameterSpans |> List.map fst |> List.distinct |> List.length)
    let configured = runHost.Create appendRun 1
    let sourceMemory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    let destinationMemory = MemoryDb(configured.QueryParamsFromRunParams)
    let sourceDb = sourceMemory :> IGeneSortDb
    let host = configured.WithRunDb(destinationMemory)
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let sourceRp = sourceHost.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    sourceMemory.SavedParameters <- [|sourceRp|]
    let rp =
        match scenario with
        | "missing-source-id" -> rp.ParamMap |> Map.remove ParamKeys.sourceExecutionIdKey |> runParameters.create
        | "invalid-source-id" -> rp.ParamMap |> Map.add ParamKeys.sourceExecutionIdKey "invalid" |> runParameters.create
        | "unknown-source-id" -> rp.ParamMap |> Map.add ParamKeys.sourceExecutionIdKey (string (Guid.NewGuid())) |> runParameters.create
        | _ -> rp
    if scenario = "duplicate-source-id" then sourceMemory.SavedParameters <- [|sourceRp; sourceRp|]
    Assert.Equal(Some sourceRun.RunName, rp.GetSourceRunName())
    if scenario = "retired-configuration" then
        // This optional parameter was removed from the spans, but its old files remain on disk.
        let retired = sourceRp.WithAdjustCeLengthFrequency(Some 10<adjustCeLengthFrequency>)
        let retiredIdentity = sourceHost.QueryParamsFromRunParams retired (geneSortDataType.Run sourceRun.RunName) |> Option.get
        sourceMemory.SavedParameters <- [|retired.WithId(Some retiredIdentity.Id); sourceRp|]
    let generation = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value sourceInterval |> Option.get
    let persist mode query data = sourceDb.saveAsync mode query data (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist geneSortDataMode.Project (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName) (outputData.Run sourceRun)
    let sourcePoolQuery = sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get
    if scenario <> "missing" then
        let actualGeneration =
            match scenario with
            | "wrong-generation" -> generation + 2
            | "previous-save-convention" -> generation + 1
            | _ -> generation
        persist geneSortDataMode.Results sourcePoolQuery (outputData.SorterPoolSet (pool (scenario <> "standard-pool") actualGeneration))
    let sourceTestsQuery = sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "") |> Option.get
    let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
    persist geneSortDataMode.Setup sourceTestsQuery (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    let originalWrites = sourceMemory.Writes.Count
    use cts = new CancellationTokenSource()
    let result = GeneSort.Dispatch.V1.SorterSgd.EvoAppendSetup.prepare sourceDb host rp (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(originalWrites, sourceMemory.Writes.Count)
    if scenario <> "valid" && scenario <> "previous-save-convention" && scenario <> "retired-configuration" && not chained then
        Assert.True(Result.isError result)
        if scenario = "missing" then
            match result with
            | Error message ->
                Assert.Contains("Source checkpoint not found", message)
                Assert.Contains(sprintf "interval %d (generation %d)" sourceInterval generation, message)
            | Ok _ -> failwith "Expected a missing-checkpoint error."
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
        let query = host.QueryParamsFromRunParams rp (geneSortDataType.Run appendRun.RunName) |> Option.get
        let other = host.QueryParamsFromRunParams (rp.WithSourceGenerationInterval(Some lastInterval)) (geneSortDataType.Run appendRun.RunName) |> Option.get
        Assert.NotEqual(query.Id, other.Id)
        let evolvedGeneration = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value lastInterval |> Option.get
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

[<Theory>]
[<InlineData(16, 0)>]
[<InlineData(32, 0)>]
[<InlineData(64, 0)>]
[<InlineData(64, 7)>]
let ``Explicit append resizes whole pools and preserves imported data`` (total: int) (modifier: int) =
    let sourcePools =
        [| for index in 0 .. 7 ->
            let members =
                [| for _ in 1 .. 4 ->
                    spMember_Soss.create (UMX.tag (Guid.NewGuid())) GeneSort.Model.Sorting.V1.sorterModel.Unknown
                        (UMX.tag 12) (UMX.tag 0) None Map.empty 5<generationNumber> |]
            sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 index)
                members 150<ceLength> (UMX.tag 0) |]
    let original = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 5<generationNumber> (Dim1 8) (Some sourcePools)
    let rp =
        Map.ofList [ParamKeys.sourceExecutionIdKey, string (Guid.NewGuid()); ParamKeys.sourcePoolFileIdKey, string (Guid.NewGuid())]
        |> runParameters.create
        |> fun rp -> rp.WithSorterCountPerPoolSet(Some (UMX.tag total)).WithSorterCountPerPool(Some 4<sorterCountPerPool>).WithMutationMod(Some (UMX.tag modifier))
    let resized = SgdEx_Soss.prepareImportedPool rp original |> Result.defaultWith failwith
    Assert.Equal(total / 4, resized.SorterPools.Count)
    Assert.Equal(Dim1 (total / 4), resized.LatticeBounds)
    Assert.Equal(original.GenerationNumber, resized.GenerationNumber)
    let members = resized.SorterPools.Values |> Seq.collect (fun pool -> pool.SorterPoolMembers) |> Seq.toArray
    Assert.Equal(total, members.Length)
    Assert.Equal(total, members |> Array.map (fun memberValue -> memberValue.SorterPoolMemberId) |> Array.distinct |> Array.length)
    Assert.All(members, fun memberValue -> Assert.Equal(modifier, int memberValue.MutationMod))
    Assert.Equal(8, original.SorterPools.Count)
    Assert.All(sourcePools |> Array.collect (fun pool -> pool.SorterPoolMembers |> Seq.toArray),
        fun memberValue -> Assert.Equal(12, int memberValue.MutationIndex))
    if total = 16 then
        Assert.Equal<Guid<sorterPoolId> array>(original.SorterPools |> Map.keys |> Seq.take 4 |> Seq.toArray,
                                              resized.SorterPools |> Map.keys |> Seq.toArray)
    if total = 32 && modifier = 0 then Assert.Equal(original, resized)
    if total = 64 then
        Assert.All(resized.SorterPools.Values |> Seq.filter (fun pool -> pool.ParentSorterPoolId.IsSome), fun pool ->
            Assert.All(pool.SorterPoolMembers, fun memberValue -> Assert.Equal(0, int memberValue.MutationIndex)))
        let tags = resized.SorterPools.Values |> Seq.map (fun pool -> pool.SorterPoolTag) |> Seq.distinct |> Seq.length
        Assert.Equal(16, tags)

[<Theory>]
[<InlineData(0, 4)>]
[<InlineData(33, 4)>]
[<InlineData(32, 0)>]
[<InlineData(-32, 4)>]
let ``Explicit append rejects invalid or changed pool sizes`` (total: int) (perPool: int) =
    let memberValue = spMember_Soss.create (UMX.tag (Guid.NewGuid())) GeneSort.Model.Sorting.V1.sorterModel.Unknown
                          (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    let sourcePool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 0) [|memberValue|] 150<ceLength> (UMX.tag 0)
    let source = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) (Some [sourcePool])
    let rp = Map.ofList [ParamKeys.sourceExecutionIdKey, "selected"; ParamKeys.sourcePoolFileIdKey, "imported"] |> runParameters.create
    let rp = rp.WithMutationMod(Some (UMX.tag 0)).WithSorterCountPerPoolSet(Some (UMX.tag total)).WithSorterCountPerPool(Some (UMX.tag perPool))
    Assert.True(SgdEx_Soss.prepareImportedPool rp source |> Result.isError)

[<Fact>]
let ``Explicit chained appends retain each immediate source identity across serialization`` () =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let mutable source = sourceWithMutationMods [0; 1; 2]
    for interval in 1 .. 3 do
        let previous = RunParameterGeneration.generate source 0<replNumber> 2<replNumber>
        let configured = GeneSort.Dispatch.V1.SorterSgd.EvoAppend.create {
            ProjectName = UMX.tag (sprintf "ExplicitAppend%d" interval)
            DatabaseName = source.DatabaseName
            RunName = UMX.tag "Branch"
            Source = { Run = source; GenerationInterval = interval; ParameterSpans = [] }
            ExecutionParameterSpans = [ParamKeys.mutationModKey, ["7"; "8"]; ParamKeys.generationIntervalLastKey, [string (interval + 1)]]
        }
        let restored = configured |> GeneSort.Project.Mp.V1.RunDto.fromDomain
                       |> MessagePack.MessagePackSerializer.Serialize
                       |> MessagePack.MessagePackSerializer.Deserialize<GeneSort.Project.Mp.V1.runDto>
                       |> GeneSort.Project.Mp.V1.RunDto.toDomain
        let branches = RunParameterGeneration.generate restored 0<replNumber> 2<replNumber>
        Assert.Equal(previous.Length * 2, branches.Length)
        Assert.Equal(branches.Length, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
        for rp in branches do
            let parent = previous |> Array.find (fun parent -> string %(parent.GetId().Value) = rp.ParamMap.[ParamKeys.sourceExecutionIdKey])
            Assert.Equal(parent.GetRepl(), rp.GetRepl())
            Assert.Equal(parent.GetModificationRate(), rp.GetModificationRate())
        source <- restored



[<Fact>]
let ``Append debug preset generates 18 runs per replica`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32_append.Pool_4_Debug ()
    let branches = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
    Assert.Equal(18, branches.Length)
    Assert.Equal(18, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    Assert.Equal("SorterSgd.Soss_append.32p4a.Msuf", string configured.ProjectName)

[<Fact>]
let ``Saved runs without explicit source fields remain readable`` () =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let configured = singleSource ()
    let bytes = configured |> GeneSort.Project.Mp.V1.RunDto.fromDomain |> MessagePack.MessagePackSerializer.Serialize
    let json = MessagePack.MessagePackSerializer.ConvertToJson(bytes) |> System.Text.Json.Nodes.JsonNode.Parse
    json.AsObject().Remove("ParameterSource") |> ignore
    json.AsObject().Remove("SourceParameterSpans") |> ignore
    let restored = MessagePack.MessagePackSerializer.ConvertFromJson(json.ToJsonString())
                   |> MessagePack.MessagePackSerializer.Deserialize<GeneSort.Project.Mp.V1.runDto>
                   |> GeneSort.Project.Mp.V1.RunDto.toDomain
    Assert.True(restored.ParameterSource.IsNone)
    Assert.Equal<runParameters array>(RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>,
                                      RunParameterGeneration.generate restored 0<replNumber> 1<replNumber>)

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(2)>]
let ``Standard checkpoints label produced generations and stop at the target`` (startingGeneration: int) =
    let host = runHost.Create (GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_128_Test ()) 1
    let rp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne).WithGenerationIntervalLast(Some (UMX.tag 2))
    let mutator = host.ExecutionContext.MakeMutator.Value rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    let initial = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag startingGeneration) (Dim1 1) None
    let memory = MemoryDb()
    let config = { intervals with MaxCount = Some 3 }
    let final = EvoOrch_Standard.runStandardEvolutionAsync memory config config rp (UMX.tag false) initial tests
                    (ceBlock.create (UMX.tag Guid.Empty) 2<sortingWidth> [||]) mutator CancellationToken.None ignore
                |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(2, int final.GenerationNumber)
    Assert.Equal<(int option * int) list>([for gen in startingGeneration + 1 .. 2 -> Some gen, gen], List.ofSeq memory.PoolGenerations)

[<Fact>]
let ``Standard append starts from imported setup and resumes its resulting checkpoint`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_append.Pool_128_Test ()
    let host = runHost.Create configured 1
    let pending = host.MakeRunParameters 0<replNumber> 1<replNumber>
    Assert.Equal(6, pending.Length)
    Assert.Equal(6, pending |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let rp = pending |> Array.find (fun rp -> int (rp.GetMutationMod().Value) = 1 && int (rp.GetSorterCountPerPoolSet().Value) = 256)
    let poolSourceId = Guid.NewGuid() |> UMX.tag<queryParamsId>
    let testsSourceId = Guid.NewGuid() |> UMX.tag<queryParamsId>
    let poolQuery = queryParams.createForImportedFile configured.DatabaseName configured.ProjectName (rp.GetRepl()) (geneSortDataType.SorterPoolSet "") poolSourceId
    let testsQuery = queryParams.createForImportedFile configured.DatabaseName configured.ProjectName (rp.GetRepl()) (geneSortDataType.SortableTestsWithPrefix "") testsSourceId
    let rp = rp.ParamMap |> Map.add ParamKeys.sourcePoolFileIdKey (string %poolSourceId)
                        |> Map.add ParamKeys.sourceTestsFileIdKey (string %testsSourceId) |> runParameters.create
    let rp = rp |> SetupFileReferences.attach [ParamKeys.setupSorterPoolSetIdKey, string %poolQuery.Id; ParamKeys.setupSortableTestsWithPrefixIdKey, string %testsQuery.Id]
    let rp = rp.WithGenerationFirst(Some 1<generationNumber>).WithGenerationCurrent(Some 1<generationNumber>)
                .WithGenerationIntervalLast(Some (UMX.tag 2)).WithSorterChildCount(Some 0<sorterChildCount>)
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 150 1)
                |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let memberValue = spMember_Standard.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 12) (UMX.tag 0) None None 1<generationNumber>
    let sourcePool = sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 0) [|memberValue|] 150<ceLength> (UMX.tag 0)
    let source = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) 1<generationNumber> (Dim1 1) (Some [sourcePool])
    let tinyTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
                        [|sortableBoolArray.create(Array.create 32 false, 32<sortingWidth>)|] |> sortableTests.Bools
    let memory = MemoryDb(host.QueryParamsFromRunParams)
    let db = memory :> IGeneSortDb
    let persist query value = db.saveAsync geneSortDataMode.Setup query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist poolQuery (outputData.SorterPoolSet(sorterPoolSet.Standard source))
    persist testsQuery (outputData.SortableTestsWithPrefix(tinyTests, [||]))
    let config = { intervals with MaxCount = Some 3 }
    let summaries = { intervals with Min = 100; MaxCount = Some 1 }
    use cts = new CancellationTokenSource()
    let execute () = SgdEx_Standard.evaluateEvolutionRunStandard host.ExecutionContext.MakeMutator.Value db config summaries rp (UMX.tag false) cts None
                     |> Async.RunSynchronously |> Result.defaultWith failwith
    execute () |> ignore
    let saved = GeneSort.Dispatch.V1.SorterSgd.Utils.loadHighestGenSorterPoolSet config db rp
                |> Async.RunSynchronously |> Result.defaultWith failwith |> Option.get
    Assert.Equal(2, int saved.GenerationNumber)
    Assert.Equal(8, saved.SorterPools.Count)
    Assert.All(saved.SorterPools.Values, fun pool -> Assert.Equal(1, int pool.MutationMod))
    let writes = memory.Writes.Count
    execute () |> ignore
    Assert.Equal(writes, memory.Writes.Count)
    let imported = db.loadAsync geneSortDataMode.Setup poolQuery |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith
    Assert.Equal(source, imported)

[<Theory>]
[<InlineData(false, 384)>]
[<InlineData(true, 240)>]
let ``Standard parameter sweeps append each source combination once`` v2 expected =
    let source = if v2 then GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_512_V2a ()
                 else GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_512_V1 ()
    let appended = if v2 then GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_append.Pool_512_V2a ()
                   else GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_append.Pool_512_V1 ()
    let original = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
    let branches = RunParameterGeneration.generate appended 0<replNumber> 1<replNumber>
    Assert.Equal(expected, original.Length)
    Assert.Equal(expected, branches.Length)
    Assert.Equal(expected, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    Assert.Equal<string>(%source.DatabaseName, %appended.DatabaseName)
