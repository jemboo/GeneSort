module GeneSort.Dispatch.Test.V2.WorkflowPhaseTests

open System
open System.Threading
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.SorterSgd
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup.SorterSgd
open GeneSort.Project.V1.Catalogs.Reports.SorterSgd
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Soss
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Standard
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterSgd
open GeneSort.Dispatch.Test.V2.WorkflowFixtures

// Exercise the actual Setup binding with an explicit source database.
let private importStart sourceDb configured storage =
    let sources = workflowSources.create {
        OpenDatabase = fun _ _ _ -> sourceDb
        Evolution = Unchecked.defaultof<_>
    }
    SetupExecutorCatalog.get sources configured storage

let private sourceSetupQuery (host: runHost) parameters output =
    let requested = host.QueryParamsFromRunParams parameters output |> Option.get
    SetupInputReferences.resolveSavedInput host.QueryParamsFromSetupParams parameters requested

let private sourceWithMutationMods modifiers = sossSource modifiers

let private singleSource () = sourceWithMutationMods [0]

let private hostMutator = function
    | SgdRunHost host -> host.MakeMutator
    | SimpleRunHost _ -> failwith "Expected an SGD test host."

let private sourceSelection (source: run) interval selection =
    let modifiers =
        selection |> List.tryFind (fst >> ParamKeys.sourceParameterKey >> (=) ParamKeys.mutationModKey)
        |> Option.map snd
        |> Option.defaultWith (fun () -> RunParameterGeneration.effectiveSpans source |> Map.find ParamKeys.mutationModKey)
        |> List.map int
    let count =
        selection |> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey)
        |> Option.map (snd >> List.exactlyOne >> int)
        |> Option.defaultWith (fun () ->
            match source.ParameterSource with
            | Some (_, spans) when selection.IsEmpty ->
                let parents = spans |> List.find (fst >> (=) ParamKeys.selectedSorterPoolCountKey) |> snd |> List.exactlyOne |> int
                let branches = source.RunParameterSpans |> List.tryFind (fst >> (=) ParamKeys.mutationModKey) |> Option.map (snd >> List.length) |> Option.defaultValue 1
                parents * branches
            | _ -> modifiers.Length)
    evoContinuationSource.create { SourceRun = source; GenerationInterval = interval; SourceMutationMods = modifiers; SelectedSorterPoolCount = count }

let private continuationConfig (source: run) interval lastInterval selection execution : evoContinuationConfig =
    evoContinuationConfig.create {
        ProjectName = UMX.tag (sprintf "Tests.Append%d" interval)
        DatabaseName = source.DatabaseName
        RunName = UMX.tag "Append"
        RunQueryCatalogName =
            if source.ExecuteFunctionName = ExecuteFunctionNames.soss then QueryCatalogNamesRun.sorterSgdMsufSossAppend
            else QueryCatalogNamesRun.sorterSgdMsufStandardAppend
        Source = sourceSelection source interval selection
        ExecutionParameterSpans = (ParamKeys.generationIntervalLastKey, [string lastInterval]) :: execution
    }

let private appendFrom (source: run) interval lastInterval selection execution =
    let create = if source.ExecuteFunctionName = ExecuteFunctionNames.soss then EvoAppend.createSoss else EvoAppend.createStandard
    continuationConfig source interval lastInterval selection execution |> create

let private condenseFrom source interval lastInterval selection execution =
    continuationConfig source interval lastInterval selection execution |> EvoCondense.createStandard

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Continuation results are isolated by destination run and source checkpoint`` soss =
    let source = if soss then sossSource [0] else standardSource ()
    let host = runHost.Create (appendFrom source 1 2 [] []) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let query parameters = host.QueryParamsFromRunParams parameters (geneSortDataType.SorterPoolSet "") |> Option.get
    let original = query rp
    Assert.NotEqual(original.Id, (query (rp.WithRunName(Some (UMX.tag "Condense")))).Id)
    Assert.NotEqual(original.Id, (query (rp.WithSourceGenerationInterval(Some 0))).Id)

[<Fact>]
let ``Continuation workflows use explicitly configured query catalogs`` () =
    let selection = [ParamKeys.selectedSorterPoolCountKey, ["1"]; ParamKeys.mutationModKey, ["0"]]
    for source, spans, create in [
        standardSource (), [], EvoAppend.createStandard
        sossSource [0], [], EvoAppend.createSoss
        standardSource (), selection, EvoCondense.createStandard
        sossSource [0], selection, EvoCondense.createSoss
    ] do
        let original = continuationConfig source 1 2 spans []
        let config = evoContinuationConfig.create {
            ProjectName = original.ProjectName
            DatabaseName = original.DatabaseName
            RunName = original.RunName
            RunQueryCatalogName = "explicit-run-catalog"
            Source = original.Source
            ExecutionParameterSpans = original.ExecutionParameterSpans
        }
        let configured = create config
        Assert.Equal(QueryCatalogNamesSetup.importedStart, configured.SetupQueryCatalogName)
        Assert.Equal(config.RunQueryCatalogName, configured.RunQueryCatalogName)
        Assert.Equal(source.ExecuteFunctionName, configured.ExecuteFunctionName)
        Assert.Equal<evolutionMakerNames option>(source.MakerNames, configured.MakerNames)
        Assert.Equal<string option>(source.GenSaveIntervalsName, configured.GenSaveIntervalsName)
        Assert.Equal<string option>(source.GenSaveSubIntervalsName, configured.GenSaveSubIntervalsName)

[<Fact>]
let ``Continuation workflows require Soss transition settings and share explicit selection`` () =
    let standard = continuationConfig (standardSource ()) 1 2 [] []
    let soss = continuationConfig (sossSource [0]) 1 2 [] []
    Assert.Equal(ExecuteFunctionNames.standard, (EvoAppend.createStandard soss).ExecuteFunctionName)
    Assert.Throws<ArgumentException>(fun () -> EvoAppend.createSoss standard |> ignore) |> ignore
    Assert.Equal(ExecuteFunctionNames.standard, (EvoCondense.createStandard standard).ExecuteFunctionName)
    let selection = [ParamKeys.selectedSorterPoolCountKey, ["1"]; ParamKeys.mutationModKey, ["0"]]
    let condensed = continuationConfig (standardSource ()) 1 2 selection []
    Assert.Equal(ExecuteFunctionNames.standard, (EvoAppend.createStandard condensed).ExecuteFunctionName)
    let wrongSource = continuationConfig (sossSource [0]) 1 2 selection []
    Assert.Equal(ExecuteFunctionNames.standard, (EvoCondense.createStandard wrongSource).ExecuteFunctionName)
    Assert.Throws<ArgumentException>(fun () -> EvoCondense.createSoss condensed |> ignore) |> ignore

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

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Report discovery respects a short run horizon when longer results exist`` soss =
    let db = MemoryDb() :> IGeneSortDb
    let intervals = GenIntervalRegistry.genIntervalConfigsDict.[GenIntervalNames.uniformInterval10]
    for generation in [10; 20] do
        save db geneSortDataMode.Results generation (geneSortDataType.SorterPoolSet "")
            (outputData.SorterPoolSet(pool soss generation))
    let load (lastInterval: int) =
        let rp = parameters.WithGenerationIntervalLast(Some (UMX.tag lastInterval))
        Utils.loadAvailableSorterPoolSets intervals db (UMX.tag 0) rp CancellationToken.None ignore
        |> Async.RunSynchronously |> Seq.toArray
    Assert.Single(load 0) |> ignore
    Assert.Equal(2, (load 1).Length)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Summary reporting rejects saved slices with no report rows`` soss =
    let memory = MemoryDb()
    let db = memory :> IGeneSortDb
    let configured = if soss then singleSource () else standardSource ()
    let host = (runHost.Create configured 1).WithRunDb(db)
    let generation = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value 0 |> Option.get
    let summary =
        if soss then
            let poolSet = sorterPoolSet_Soss.create (UMX.tag Guid.Empty) (UMX.tag generation) (Dim1 1) None
            spSummarySet_Soss.create (UMX.tag Guid.Empty) (UMX.tag generation) [|SorterPoolSetSummary_Soss.fromPoolSet poolSet|]
            |> spSummarySet.Soss
        else
            let poolSet = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag generation) (Dim1 1) None
            spSummarySet_Standard.create (UMX.tag Guid.Empty) (UMX.tag generation) [|SorterPoolSetSummary_Standard.fromPoolSet poolSet|]
            |> spSummarySet.Standard
    save db geneSortDataMode.Results generation (geneSortDataType.SorterPoolSetSummarySet "") (outputData.SorterPoolSetSummarySet summary)
    let rp = parameters.WithGenerationFirst(Some (UMX.tag 0)).WithGenerationIntervalLast(Some (UMX.tag 0))
    use cts = new CancellationTokenSource()
    let result = Reporting.summaryReportExecutor host.Run host.Storage rp (UMX.tag true) cts None |> Async.RunSynchronously
    match result with
    | Ok _ -> failwith "An empty summary was reported as successful."
    | Error message -> Assert.Contains("no report rows", message)
    Assert.DoesNotContain(memory.Writes, fun (_, kind) -> kind = geneSortDataType.TextReport (UMX.tag "SummaryReport"))

let private verifySavedSossReport (db: IGeneSortDb) (rp: runParameters) (generation: int)
                                  partitions prefix (expectedSubset: string) hasSummary =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let load kind =
        let query = db.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag generation))) kind |> Option.get
        db.loadAsync geneSortDataMode.Results query |> Async.RunSynchronously |> Result.defaultWith failwith
    let saved = load (geneSortDataType.SorterPoolSet "") |> OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
    let bytes = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto saved |> MessagePack.MessagePackSerializer.Serialize
    let checkpoint = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(bytes)
                     |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto
    Assert.Equal(generation, int checkpoint.GenerationNumber)
    let fresh =
        checkpoint |> SorterPoolSet_Soss.clearEvaluations
        |> Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 true
            (UMX.tag false) (UMX.tag expectedSubset) true
    for KeyValue(poolId, pool) in checkpoint.SorterPools do
        let freshMembers = fresh.SorterPools.[poolId].SorterPoolMembers |> Seq.map (fun m -> m.SorterPoolMemberId, m) |> Map.ofSeq
        for memberValue in pool.SorterPoolMembers do
            Assert.Equal(expectedSubset, %memberValue.SubsetIdForSelection)
            let freshMember = freshMembers.[memberValue.SorterPoolMemberId]
            Assert.Equal(float freshMember.Score, float memberValue.Score)
            for KeyValue(subset, evaluation) in memberValue.SorterEvalMap do
                Assert.True((evaluation = freshMember.SorterEvalMap.[subset]))
    let summarySet =
        match load (geneSortDataType.SorterPoolSetSummarySet "") with
        | outputData.SorterPoolSetSummarySet (spSummarySet.Soss value) -> value
        | _ -> failwith "Expected Soss summary set"
    let summaryBytes = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummarySetDto_Soss.toDto summarySet |> MessagePack.MessagePackSerializer.Serialize
    let restoredSummary = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetSummarySetDto_Soss>(summaryBytes)
                          |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummarySetDto_Soss.fromDto
    Assert.Equal(generation, int restoredSummary.LastGeneration)
    if not hasSummary then Assert.Empty(restoredSummary.SorterPoolSetSummaries)
    else
        let snapshot = restoredSummary.SorterPoolSetSummaries |> Array.exactlyOne
        Assert.Equal(generation, int snapshot.GenerationNumber)
        Assert.True(snapshot.SorterPoolSetId = checkpoint.SorterPoolSetId)
        Assert.Equal(fresh.SortedSorterEvalPercentage, snapshot.SortedSorterEvalPercentage)
        let expectedRows = fresh.SorterPools |> Map.values |> Seq.sumBy (fun p ->
            p.SorterPoolMembers |> Seq.collect (fun m -> Map.keys m.SorterEvalMap) |> Seq.distinct |> Seq.length)
        Assert.Equal(expectedRows, snapshot.SorterPoolSummaries.Length)
        for row in snapshot.SorterPoolSummaries do
            let pool = fresh.SorterPools.[row.SorterPoolId]
            let evaluations = pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterEvalMap.[row.SortableTestsSubsetId]) |> Seq.toArray
            let scores = evaluations |> Array.map (SorterEvalFunctions.getFunctionForMeasure (rp.GetSorterEvalMeasure().Value) >> float)
            Assert.Equal(int pool.RawCeLength, int row.RawCeLength)
            Assert.Equal(Array.average scores, float row.AverageSorterPoolScore)
            Assert.Equal(Array.min scores, float row.MinimumSorterPoolScore)
            Assert.Equal(evaluations |> Array.averageBy (SorterEval.getUnsortedCount >> float), row.AverageUnsortedCount)

    let bins =
        match load (geneSortDataType.SorterPoolBinsSetSeries "") with
        | outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Soss value) -> value
        | _ -> failwith "Expected Soss evaluation bins"
    let binBytes = GeneSort.Eval.Mp.V1.Bins.Soss.SorterPoolBinsSetSeriesDto_Soss.fromDomain bins |> MessagePack.MessagePackSerializer.Serialize
    let restoredBins = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Bins.Soss.sorterPoolBinsSetSeriesDto_Soss>(binBytes)
                       |> GeneSort.Eval.Mp.V1.Bins.Soss.SorterPoolBinsSetSeriesDto_Soss.toDomain
    if not hasSummary then Assert.Equal(0, restoredBins.Count)
    else
        let binSet = restoredBins.SorterPoolEvalBinsSets |> Map.values |> Seq.exactlyOne
        Assert.Equal(generation, int binSet.GenerationNumber)
        Assert.True(binSet.SorterPoolSetId = checkpoint.SorterPoolSetId)
        Assert.Equal(checkpoint.SorterPools.Count, binSet.SorterPoolEvalBinsMap.Count)
        let poolBinsById = binSet.SorterPoolEvalBinsMap |> Map.values |> Seq.map (fun b -> b.SorterPoolId, b) |> Map.ofSeq
        Assert.Equal(checkpoint.SorterPools.Count, poolBinsById.Count)
        // Bins intentionally store V1 evaluations. Compare full downgraded payloads
        // as multisets: copied models may share a sorter ID and must not be deduplicated.
        let payloadCounts evaluations =
            evaluations |> Seq.map (fun evaluation ->
                SorterEval.downgradeTo sorterEvalType.V1 evaluation
                |> GeneSort.SortingOps.Mp.SorterEvalDto.fromDomain
                |> MessagePack.MessagePackSerializer.Serialize |> Convert.ToHexString)
            |> Seq.countBy id |> Seq.sortBy fst |> Seq.toArray
        let snapshot = restoredSummary.SorterPoolSetSummaries |> Array.exactlyOne
        for KeyValue(poolId, pool) in fresh.SorterPools do
            let poolBins = poolBinsById.[poolId]
            let expected = pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterEvalForSelection.Value) |> Seq.toArray
            let actual = poolBins.Bins |> Map.values |> Seq.collect (fun b -> b.SorterEvals) |> Seq.toArray
            Assert.True((payloadCounts expected = payloadCounts actual), sprintf "Saved bins diverged at generation %d pool %A" generation poolId)
            for KeyValue(key, bin) in poolBins.Bins do
                Assert.Equal(bin.SorterEvals.Count, bin.EvalCount)
                Assert.Equal(bin.EvalCount, bin.SortedCount + bin.UnsortedCount)
                Assert.Equal(bin.SorterEvals |> Seq.filter SorterEval.getIsSorted |> Seq.length, bin.SortedCount)
                for evaluation in bin.SorterEvals do
                    Assert.Equal(expectedSubset, %(SorterEval.getSortableTestsSubsetId evaluation))
                    Assert.Equal(int key.CeCount, int (SorterEval.getCeLength evaluation))
                    Assert.Equal(int key.StageLength, int (SorterEval.getStageLength evaluation))
            let summaryRow = snapshot.SorterPoolSummaries |> Array.find (fun row ->
                row.SorterPoolId = poolId && %row.SortableTestsSubsetId = expectedSubset)
            let scores = actual |> Array.map (SorterEvalFunctions.getFunctionForMeasure (rp.GetSorterEvalMeasure().Value) >> float)
            // Grouping changes summation order, so allow floating-point roundoff.
            Assert.Equal(Array.average scores, float summaryRow.AverageSorterPoolScore, 12)
            Assert.Equal(Array.min scores, float summaryRow.MinimumSorterPoolScore)
            Assert.Equal(actual |> Array.averageBy (SorterEval.getUnsortedCount >> float), summaryRow.AverageUnsortedCount)

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
    Assert.NotEmpty(SetupFileReferences.fromRunParameters rp)
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
    EvoStartCreation.createSossStart host.Run.ProjectName host.Run.DatabaseName (fun _ -> async { return Ok (tinyTests, [||]) })
        (fun _ -> async { return Ok (pool true 0) }) memory
        rp cts.Token
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    bound.ExecuteFunction rp (UMX.tag false) cts None
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Contains(memory.Writes, fun (mode, outputType) ->
        mode = geneSortDataMode.Results && outputType = geneSortDataType.SorterPoolSet "")

[<Theory>]
[<InlineData(true, true, 2)>]
[<InlineData(true, false, 2)>]
[<InlineData(false, true, 2)>]
[<InlineData(false, false, 2)>]
[<InlineData(true, false, 1)>]
[<InlineData(false, false, 1)>]
let ``Soss adjusts on merged evaluations but retains the scheduled partition for selection`` changed otherPartitionUnsorted partitionCount =
    let host = runHost.Create (singleSource ()) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let mutator = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    let makeTests values =
        sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
            [| sortableBoolArray.create(values, 32<sortingWidth>) |] |> sortableTests.Bools
    let partitions =
        [ UMX.tag "0", makeTests (Array.init 32 (fun i -> otherPartitionUnsorted && i < 16))
          UMX.tag "1", makeTests (Array.create 32 false) ]
        |> List.filter (fun (id, _) -> partitionCount > 1 || id = UMX.tag "1")
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
    Assert.Equal(changed && otherPartitionUnsorted && partitionCount > 1, Map.containsKey CommonSgdSoss.mergedSubsetId survivor.SorterEvalMap)
    Assert.True(survivor.SorterEvalForSelection.Value = survivor.SorterEvalMap.[UMX.tag "1"])
    if changed && otherPartitionUnsorted then
        Assert.True(SorterEval.getIsSorted survivor.SorterEvalForSelection.Value)
        Assert.False(SorterEval.getIsSorted survivor.SorterEvalMap.[CommonSgdSoss.mergedSubsetId])
    if changed && not otherPartitionUnsorted then
        Assert.True(resultPool.RawCeLength < inputPool.RawCeLength)
    else
        Assert.Equal(inputPool.RawCeLength, resultPool.RawCeLength)

[<Theory>]
[<InlineData(1, false, false)>]
[<InlineData(1, true, false)>]
[<InlineData(2, false, false)>]
[<InlineData(2, true, false)>]
[<InlineData(2, false, true)>]
[<InlineData(2, true, true)>]
let ``Soss length changes invalidate every subset before parent and survivor ranking`` (partitionCount: int) forReport mergedSelection =
    let host = runHost.Create (singleSource ()) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let mutator = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    let values = Array.create 32 true
    values.[1] <- false
    let test = sortableBoolArray.create(values, 32<sortingWidth>)
    // Two failures have a strictly worse UnsortedCount score than zero failures.
    let tests = sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> [|test; test|] |> sortableTests.Bools
    let partitions = [0 .. partitionCount - 1] |> List.map (fun i -> UMX.tag (string i), tests)
                     |> Map.ofList |> partitionOfSortableTests.create
    let makeMember (id: Guid) cePosition =
        let codes = Array.create 150 0
        codes.[cePosition] <- 1 // Comparator (0,1).
        let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> codes
                    |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
        spMember_Soss.create (UMX.tag id) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    // At the original length both sort; at the adjusted budget of one CE only B sorts.
    // A sorts first by ID, so stale equal scores would select A instead of B.
    let memberA = makeMember Guid.Empty 1
    let memberB = makeMember (Guid.Parse "00000000-0000-0000-0000-000000000001") 0
    let poolId = UMX.tag (Guid.NewGuid())
    let pool = sorterPool_Soss.create poolId None (UMX.tag "length") (D1 0) [|memberA; memberB|] 150<ceLength> (UMX.tag 0)
    let generation = if mergedSelection then 2<generationNumber> else 0<generationNumber>
    let input = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) generation (Dim1 1) (Some [pool])
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let selectedId = if mergedSelection then CommonSgdSoss.mergedSubsetId else UMX.tag<sortableTestsSubsetId> "0"
    let adjustmentId = if partitionCount = 1 then selectedId else CommonSgdSoss.mergedSubsetId
    let evaluate selected force poolSet =
        Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 force
            (UMX.tag false) selected true poolSet
    let cached = input |> SorterPoolSet_Soss.withSubsetIdForSelection adjustmentId |> evaluate adjustmentId true
    let adjusted = cached |> SorterPoolSet_Soss.adjustCeLengths 1.0<sortedFraction>
    Assert.Equal(1, int adjusted.SorterPools.[poolId].RawCeLength)
    for memberValue in adjusted.SorterPools.[poolId].SorterPoolMembers do
        Assert.Empty(memberValue.SorterEvalMap)
        Assert.True(memberValue.SorterEvalForSelection.IsNone)
    // An unchanged length must retain its caches.
    let unchanged =
        sorterPool_Soss.create poolId None pool.Name pool.SorterPoolTag
            (cached.SorterPools.[poolId].SorterPoolMembers |> Seq.toArray) 1<ceLength> pool.MutationMod
        |> SorterPool_Soss.adjustCeLengthByThreshold 1.0<sortedFraction>
    Assert.All(unchanged.SorterPoolMembers, fun memberValue -> Assert.NotEmpty(memberValue.SorterEvalMap))
    let measure = unsortedCountMeasure.create false |> sorterEvalMeasure.UnsortedCount
    for parentSelection in [true; false] do
        // Parent test starts after adjustment; survivor test adjusts inside the step.
        let start =
            if parentSelection then adjusted |> SorterPoolSet_Soss.withSubsetIdForSelection selectedId
            else cached |> SorterPoolSet_Soss.withSubsetIdForSelection SortableTestsSubsetId.Default
        let result =
            Pipeline_Soss.runGenerationStepSoss mutator
                (if parentSelection then 2<sorterCountPerPool> else 1<sorterCountPerPool>)
                (if parentSelection then 1<sorterCountPerPool> else 2<sorterCountPerPool>)
                0<sorterChildCount> (UMX.tag false) (UMX.tag false) partitions
                1<generationsPerSubset> 1<generationsPerSubset> prefix sorterEvalType.V2 measure
                start forReport (UMX.tag false) 1.0<sortedFraction>
        let survivor = result.SorterPools.[poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.True(survivor.SorterPoolMemberId = memberB.SorterPoolMemberId)
        Assert.Equal(0, int (SorterEval.getUnsortedCount survivor.SorterEvalForSelection.Value))
        let fresh = evaluate selectedId true result
        let freshMember = fresh.SorterPools.[poolId].SorterPoolMembers |> Seq.exactlyOne
        for KeyValue(subset, evaluation) in survivor.SorterEvalMap do
            Assert.True((evaluation = freshMember.SorterEvalMap.[subset]))

[<Theory>]
[<InlineData(1, true)>]
[<InlineData(1, false)>]
[<InlineData(2, true)>]
[<InlineData(2, false)>]
let ``Soss resume replaces legacy length caches before parent and survivor selection`` (partitionCount: int) parentSelection =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let host = runHost.Create (singleSource ()) 1
    let baseRp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne)
                     .WithPartitionCount(Some (UMX.tag partitionCount))
                     .WithGenerationsPerSubset(Some 3<generationsPerSubset>)
                     .WithGenerationsPerMergedSet(Some 2<generationsPerSubset>)
                     .WithSorterCountPerPool(Some (if parentSelection then 2<sorterCountPerPool> else 1<sorterCountPerPool>))
                     .WithSelectedSorterCountPerPool(Some (if parentSelection then 1<sorterCountPerPool> else 2<sorterCountPerPool>))
                     .WithSorterChildCount(Some 0<sorterChildCount>)
                     .WithSorterCountCycle(None)
                     .WithSorterPoolExpansionRate(None)
                     .WithSorterEvalMeasure(Some (unsortedCountMeasure.create false |> sorterEvalMeasure.UnsortedCount))
    let values = Array.create 32 true
    values.[1] <- false
    let test = sortableBoolArray.create(values, 32<sortingWidth>)
    let tests = sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 4 test) |> sortableTests.Bools
    let partitions = SgdEx_Soss.makeSortableTestsPartitions (baseRp.GetRngType().Value) (baseRp.GetSeedSoss().Value)
                         (UMX.tag partitionCount) tests
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let makeMember (id: Guid) cePosition =
        let codes = Array.create 150 0
        codes.[cePosition] <- 1
        let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> codes
                    |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
        spMember_Soss.create (UMX.tag id) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    let memberA = makeMember Guid.Empty 140
    let memberB = makeMember (Guid.Parse "00000000-0000-0000-0000-000000000001") 20
    let poolId = UMX.tag (Guid.NewGuid())
    let fullPool = sorterPool_Soss.create poolId None (UMX.tag "legacy") (D1 0) [|memberA; memberB|] 150<ceLength> (UMX.tag 0)
    let selectedFor generation =
        if partitionCount = 1 then UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId> SortableTestsPartitionId.Default
        elif generation % 8 < 3 then UMX.tag "0"
        elif generation % 8 < 6 then UMX.tag "1"
        else CommonSgdSoss.mergedSubsetId
    // Resume within a phase and at every kind of transition, without summary
    // reporting that might accidentally refresh caches before parent ranking.
    for generation in [1; 3; 6; 8] do
        let previousSubset = selectedFor (generation - 1)
        let fullSet =
            sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 1) (Some [fullPool])
            |> SorterPoolSet_Soss.withSubsetIdForSelection previousSubset
            |> Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 true
                (UMX.tag false) previousSubset true
        let evaluatedPool = fullSet.SorterPools.[poolId]
        Assert.All(evaluatedPool.SorterPoolMembers, fun memberValue -> Assert.Equal(0, int (SorterEval.getUnsortedCount memberValue.SorterEvalForSelection.Value)))
        // Reproduce the old bug: shorten the pool but preserve evaluations from 150 CEs.
        let legacyPool = sorterPool_Soss.create poolId None fullPool.Name fullPool.SorterPoolTag
                             (evaluatedPool.SorterPoolMembers |> Seq.toArray) 100<ceLength> fullPool.MutationMod
        let legacy = fullSet |> SorterPoolSet_Soss.upsertPool legacyPool
        let bytes = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto legacy |> MessagePack.MessagePackSerializer.Serialize
        let restored = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(bytes)
                       |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto
        let rp = baseRp.WithGenerationIntervalLast(Some (UMX.tag (generation + 1))).WithGenerationCurrent(Some 0<generationNumber>)
        let memory = MemoryDb()
        let db = memory :> IGeneSortDb
        let query gen kind = db.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some gen)) kind |> Option.get
        let save mode gen kind value =
            let query = query gen kind
            let query = if mode = geneSortDataMode.Setup then (SetupInputReferences.resolveRecorded rp query) else query
            db.saveAsync mode query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
        save geneSortDataMode.Setup 0<generationNumber> (geneSortDataType.SortableTestsWithPrefix "") (outputData.SortableTestsWithPrefix(tests, [||]))
        save geneSortDataMode.Results (UMX.tag generation) (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet (sorterPoolSet.Soss restored))
        use cts = new CancellationTokenSource()
        SgdEx_Soss.evaluateEvolutionRunSoss (hostMutator host) db
            { intervals with MaxCount = Some 10 } { intervals with Method = Constant 100 }
            rp (UMX.tag false) cts None
        |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
        let result = db.loadAsync geneSortDataMode.Results (query (UMX.tag (generation + 1)) (geneSortDataType.SorterPoolSet ""))
                     |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
        let survivor = result.SorterPools.[poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.True(survivor.SorterPoolMemberId = memberB.SorterPoolMemberId,
            sprintf "Stale cache selected A on resume at generation %d (parent=%b, partitions=%d)" generation parentSelection partitionCount)
        Assert.True(survivor.SubsetIdForSelection = selectedFor generation)
        let fresh = Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 true
                        (UMX.tag false) (selectedFor generation) true result
        let freshMember = fresh.SorterPools.[poolId].SorterPoolMembers |> Seq.exactlyOne
        for KeyValue(subset, evaluation) in survivor.SorterEvalMap do
            Assert.True((evaluation = freshMember.SorterEvalMap.[subset]))

[<Theory>]
[<InlineData(false, false, 2)>]
[<InlineData(false, true, 2)>]
[<InlineData(true, false, 2)>]
[<InlineData(true, true, 2)>]
[<InlineData(false, false, 1)>]
[<InlineData(false, true, 1)>]
[<InlineData(true, false, 1)>]
[<InlineData(true, true, 1)>]
let ``Soss parent survivor and pool rankings follow every cycle boundary`` cached forReport partitionCount =
    let host = runHost.Create (singleSource ()) 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let mutator = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    // Each test has one adjacent inversion at a different position.
    let makeTests cuts =
        cuts |> Array.map (fun cut ->
            let values = Array.init 32 (fun i -> i >= cut)
            values.[cut - 1] <- true
            values.[cut] <- false
            sortableBoolArray.create(values, 32<sortingWidth>))
        |> sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth>
        |> sortableTests.Bools
    let partitions =
        [ UMX.tag "0", makeTests [|1; 2; 3; 4; 5|]
          UMX.tag "1", makeTests [|6; 7; 8; 9; 10|] ]
        |> List.take partitionCount |> Map.ofList |> partitionOfSortableTests.create
    // A wins partition 0, B wins partition 1, C wins Merge (4 failures vs 5).
    // Use at least two failures to avoid the log-score tie between zero and one.
    let members =
        [| [|1; 2; 3; 4; 5|]; [|6; 7; 8; 9; 10|]; [|1; 2; 3; 6; 7; 8|] |]
        |> Array.map (fun cuts ->
            let codes = Array.create 150 0
            cuts |> Array.iteri (fun i cut ->
                codes.[i] <- GeneSort.Sorting.Sorter.ce.create (cut - 1) cut |> GeneSort.Sorting.Sorter.Ce.toIndex)
            let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> codes
                        |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
            spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>)
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let measure = unsortedCountMeasure.create false |> sorterEvalMeasure.UnsortedCount
    let expectedSubset generation =
        if partitionCount = 1 || generation % 7 < 2 then "0"
        elif generation % 7 < 4 then "1"
        else "Merge"
    // Two complete cycles plus wraparound, with unequal subset/merged durations.
    for generation in 0 .. 14 do
        for selectParents in [true; false] do
            let previous = if generation = 0 then "Default" else expectedSubset (generation - 1)
            let selected = expectedSubset generation
            let expectedIndex = match selected with "0" -> 0 | "1" -> 1 | _ -> 2
            let poolId = UMX.tag (Guid.NewGuid())
            let inputPool = sorterPool_Soss.create poolId None (UMX.tag "boundary") (D1 0) members 150<ceLength> (UMX.tag 0)
            let input =
                sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 1) (Some [inputPool])
                |> SorterPoolSet_Soss.withSubsetIdForSelection (UMX.tag previous)
            let prepared =
                if cached then
                    Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 false
                        (UMX.tag false) (UMX.tag selected) true input
                else input
            // Isolate each ranking: either parent retention or survivor pruning
            // reduces three candidates to one. No mutation obscures their identities.
            let result =
                Pipeline_Soss.runGenerationStepSoss mutator
                    (if selectParents then 3<sorterCountPerPool> else 1<sorterCountPerPool>)
                    (if selectParents then 1<sorterCountPerPool> else 3<sorterCountPerPool>)
                    0<sorterChildCount> (UMX.tag false) (UMX.tag false) partitions
                    2<generationsPerSubset> 3<generationsPerSubset> prefix sorterEvalType.V2 measure
                    prepared forReport (UMX.tag false) 1.0<sortedFraction>
            let survivor = result.SorterPools.[poolId].SorterPoolMembers |> Seq.exactlyOne
            Assert.True(survivor.SorterPoolMemberId = members.[expectedIndex].SorterPoolMemberId,
                sprintf "generation=%d subset=%s parentSelection=%b cached=%b report=%b partitions=%d"
                    generation selected selectParents cached forReport partitionCount)
            Assert.Equal(selected, %survivor.SubsetIdForSelection)
            Assert.Equal(selected, %(SorterEval.getSortableTestsSubsetId survivor.SorterEvalForSelection.Value))
            Assert.Equal(generation + 1, int result.GenerationNumber)

        // Exercise milestone trimming/expansion through the orchestrator as well.
        let previous = if generation = 0 then "Default" else expectedSubset (generation - 1)
        let selected = expectedSubset generation
        let expectedIndex = match selected with "0" -> 0 | "1" -> 1 | _ -> 2
        let pools = members |> Array.mapi (fun i memberValue ->
            sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag (string i)) (D1 i)
                [|memberValue|] 150<ceLength> (UMX.tag 0))
        let input =
            sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 3) (Some pools)
            |> SorterPoolSet_Soss.withSubsetIdForSelection (UMX.tag previous)
        let prepared =
            if cached then
                Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 false
                    (UMX.tag false) (UMX.tag selected) true input
            else input
        let poolMeasure = stDevPoolMeasure.create 0.0<stDevWeight> measure |> sorterPoolMeasure.StDevPool
        let config = { intervals with MaxCount = Some 16 }
        let expansionRp = rp.WithGenerationIntervalLast(Some (UMX.tag (generation + 1)))
                            .WithGenerationsPerSubset(Some 2<generationsPerSubset>)
                            .WithGenerationsPerMergedSet(Some 3<generationsPerSubset>)
                            .WithSorterPoolExpansionRate(Some 3<sorterPoolExpansionRate>)
                            .WithSorterPoolSelectionIntervals(Some config)
                            .WithSorterPoolMeasure(Some poolMeasure)
                            .WithSorterCountPerPool(Some 1<sorterCountPerPool>)
                            .WithSelectedSorterCountPerPool(Some 1<sorterCountPerPool>)
                            .WithSorterChildCount(Some 0<sorterChildCount>)
                            .WithSorterCountCycle(None)
                            .WithSorterEvalMeasure(Some measure)
        let reportConfig = if forReport then config else { config with Method = Constant 100 }
        let reportingDb = MemoryDb()
        let expanded =
            EvoOrch_Soss.runSossEvolutionAsync reportingDb config reportConfig expansionRp (UMX.tag false)
                prepared partitions prefix mutator CancellationToken.None ignore
            |> Async.RunSynchronously |> Result.defaultWith failwith
        Assert.Equal(3, int expanded.SorterPoolCount)
        let copiedIds = expanded.SorterPools |> Map.values |> Seq.collect (fun p -> p.SorterPoolMembers) |> Seq.map (fun m -> m.SorterPoolMemberId) |> Seq.toArray
        Assert.Equal(3, copiedIds |> Array.distinct |> Array.length)
        for childPool in expanded.SorterPools |> Map.values do
            Assert.True(childPool.ParentSorterPoolId = Some pools.[expectedIndex].SorterPoolId,
                sprintf "Wrong pool expanded at generation=%d subset=%s cached=%b report=%b" generation selected cached forReport)
            let memberValue = childPool.SorterPoolMembers |> Seq.exactlyOne
            Assert.Equal(selected, %memberValue.SubsetIdForSelection)
            Assert.DoesNotContain(memberValue.SorterPoolMemberId, members |> Array.map (fun m -> m.SorterPoolMemberId))
            if partitionCount = 1 && generation = 0 then
                Assert.True(childPool.RawCeLength < 150<ceLength>, "Expansion must preserve CE adjustment transition detection.")
        verifySavedSossReport reportingDb expansionRp (generation + 1) partitions prefix selected forReport

        if forReport && partitionCount = 2 && List.contains generation [0; 2; 4; 7] then
            // Keep multiple score bins and duplicate model IDs in a single pool.
            // This detects grouping errors or accidental deduplication that singleton
            // expanded pools would not expose.
            let copy = SpMember_Soss.deriveForChildPool (UMX.tag (Guid.NewGuid())) (UMX.tag 0) members.[0]
            let groupedPool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "bin-groups") (D1 0)
                                  (Array.append members [|copy|]) 150<ceLength> (UMX.tag 0)
            let groupedInput = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 1) (Some [groupedPool])
                               |> SorterPoolSet_Soss.withSubsetIdForSelection (UMX.tag previous)
            let groupedRp = expansionRp.WithSorterPoolExpansionRate(None)
                                       .WithSorterCountPerPool(Some 4<sorterCountPerPool>)
                                       .WithSelectedSorterCountPerPool(Some 4<sorterCountPerPool>)
                                       .WithDistinctSorterHashes(Some (UMX.tag false))
            let groupedDb = MemoryDb()
            let groupedResult =
                EvoOrch_Soss.runSossEvolutionAsync groupedDb config config groupedRp (UMX.tag false)
                    groupedInput partitions prefix mutator CancellationToken.None ignore
                |> Async.RunSynchronously |> Result.defaultWith failwith
            Assert.Equal(4, groupedResult.SorterPools.[groupedPool.SorterPoolId].SorterPoolMembers |> Seq.length)
            let distinctKeys = groupedResult.SorterPools.[groupedPool.SorterPoolId].SorterPoolMembers
                               |> Seq.map (fun m -> GeneSort.Eval.V1.Bins.SorterEvalKey.fromSorterEval m.SorterEvalForSelection.Value)
                               |> Seq.distinct |> Seq.length
            Assert.True(distinctKeys > 1)
            verifySavedSossReport groupedDb groupedRp (generation + 1) partitions prefix selected true

[<Fact>]
let ``Soss copied members reevaluate independently at destination pool lengths`` () =
    let values = Array.create 32 true
    values.[1] <- false
    let test = sortableBoolArray.create(values, 32<sortingWidth>)
    let tests = sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> [|test; test|] |> sortableTests.Bools
    let partitions = Map.ofList [UMX.tag "0", tests; UMX.tag "1", tests] |> partitionOfSortableTests.create
    let codes = Array.create 150 0
    codes.[140] <- 1
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> codes
                |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let original = spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 5) (UMX.tag 0) None Map.empty 0<generationNumber>
    let sourcePool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 0) [|original|] 150<ceLength> (UMX.tag 0)
    let sourceSet = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) (Some [sourcePool])
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let evaluate poolSet = Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 false
                               (UMX.tag false) CommonSgdSoss.mergedSubsetId true poolSet
    let cachedSet = sourceSet |> SorterPoolSet_Soss.withSubsetIdForSelection CommonSgdSoss.mergedSubsetId |> evaluate
    let cachedPool = cachedSet.SorterPools.[sourcePool.SorterPoolId]
    let cachedMember = cachedPool.SorterPoolMembers |> Seq.exactlyOne
    Assert.Equal(3, cachedMember.SorterEvalMap.Count)
    Assert.Equal(0, int (SorterEval.getUnsortedCount cachedMember.SorterEvalForSelection.Value))
    let children =
        [|100; 150|] |> Array.mapi (fun i length ->
            let derived = SorterPool_Soss.deriveChildPool (UMX.tag (Guid.NewGuid())) (UMX.tag i) cachedPool
            let copied = derived.SorterPoolMembers |> Seq.exactlyOne
            Assert.Empty(copied.SorterEvalMap)
            Assert.True(copied.SorterPoolMemberId <> cachedMember.SorterPoolMemberId)
            Assert.True(copied.SorterModel = cachedMember.SorterModel)
            Assert.Equal(0, int copied.MutationIndex)
            sorterPool_Soss.create derived.SorterPoolId derived.ParentSorterPoolId derived.Name (D1 i)
                [|copied|] (UMX.tag length) derived.MutationMod)
    let result = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 2) (Some children) |> evaluate
    for i in 0 .. 1 do
        let memberValue = result.SorterPools.[children.[i].SorterPoolId].SorterPoolMembers |> Seq.exactlyOne
        for KeyValue(subset, evaluation) in memberValue.SorterEvalMap do
            let expected = if i = 1 then 0 elif subset = CommonSgdSoss.mergedSubsetId then 4 else 2
            Assert.Equal(expected, int (SorterEval.getUnsortedCount evaluation))
    // Derivation never invalidates the original source member's own cache.
    Assert.Equal(3, cachedMember.SorterEvalMap.Count)

[<Theory>]
[<InlineData("Lcg", false)>]
[<InlineData("Net", false)>]
[<InlineData("Smx", false)>]
[<InlineData("Lcg", true)>]
[<InlineData("Net", true)>]
[<InlineData("Smx", true)>]
let ``Soss SeedSoss recreates ordered partition contents and cached evaluations after serialization`` rngName bitVector =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let rngType = match rngName with "Lcg" -> Lcg | "Net" -> Net | _ -> Smx
    // Unique inputs span several SIMD blocks, including a partially filled block.
    let rows =
        [| for value in 0 .. 1026 ->
            Array.init 32 (fun bit -> (value &&& (1 <<< bit)) <> 0)
            |> fun values -> sortableBoolArray.create(values, 32<sortingWidth>) |]
    let setupTests =
        if bitVector then
            SortableBitv512Tests.fromBoolArrays (UMX.tag (Guid.NewGuid())) 32<sortingWidth> rows |> sortableTests.Bitv512
        else
            sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> rows |> sortableTests.Bools
    let setupBytes =
        GeneSort.Sorting.Mp.Sortable.SortableTestsDto.fromDomain setupTests
        |> MessagePack.MessagePackSerializer.Serialize
    let restoredTests =
        MessagePack.MessagePackSerializer.Deserialize<GeneSort.Sorting.Mp.Sortable.sortableTestsDto>(setupBytes)
        |> GeneSort.Sorting.Mp.Sortable.SortableTestsDto.toDomain
    let contents (partitions: partitionOfSortableTests) =
        partitions.SortableTests |> Map.map (fun _ tests ->
            let arrays =
                match tests with
                | sortableTests.Bools values -> values.SortableBinaryArrays
                | sortableTests.Bitv512 values -> values.SimdSortBlocks |> Array.collect SortBlockBitv512.toSortableBoolArrays
                | _ -> failwith "Unexpected test format"
            arrays |> Array.map (fun value -> value.Values))
    let model =
        msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.init 150 (fun i -> 1 + i % 90))
        |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let memberValue = spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    let inputPool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "cache") (D1 0) [|memberValue|] 150<ceLength> (UMX.tag 0)
    let input = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 7<generationNumber> (Dim1 1) (Some [inputPool])
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let serializePool value =
        GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto value |> MessagePack.MessagePackSerializer.Serialize
    for count in [1; 3; 7] do
        for seed in [1UL; 0x123456789ABCDEF0UL] do
            let make tests (seed: uint64) = SgdEx_Soss.makeSortableTestsPartitions rngType (UMX.tag seed) (UMX.tag count) tests
            let original = make setupTests seed
            let recreated = make restoredTests seed
            Assert.True(contents original = contents recreated,
                sprintf "Partition contents/order changed for RNG=%s count=%d seed=%d SIMD=%b" rngName count seed bitVector)
            if count > 1 then
                // Ensure this fixture detects accidentally ignoring or changing SeedSoss.
                Assert.False(contents original = contents (make restoredTests (seed + 1UL)))
            let subset = original.SortableTests |> Map.keys |> Seq.head |> UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId>
            let evaluate partitions force pool =
                Pipeline_Soss.evaluateRequiredPartitions partitions prefix sorterEvalType.V2 force
                    (UMX.tag false) subset true pool
            let cached = input |> SorterPoolSet_Soss.withSubsetIdForSelection subset |> evaluate original true
            let checkpointBytes = serializePool cached
            let restored =
                MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(checkpointBytes)
                |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto
            let reused = evaluate recreated false restored |> serializePool
            let fresh = evaluate recreated true restored |> serializePool
            // Compare the entire member evaluation map, including Merge, not only scores.
            Assert.True((checkpointBytes = reused))
            Assert.True((checkpointBytes = fresh),
                sprintf "Cached evaluations changed for RNG=%s count=%d seed=%d SIMD=%b" rngName count seed bitVector)

[<Theory>]
[<InlineData(1, false)>]
[<InlineData(2, false)>]
[<InlineData(1, true)>]
[<InlineData(2, true)>]
let ``Soss serialized checkpoint resumes match uninterrupted cycles at every boundary`` (partitionCount: int) sparse =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let host = runHost.Create (singleSource ()) 1
    let rp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne)
                 .WithPartitionCount(Some (UMX.tag partitionCount))
                 .WithGenerationsPerSubset(Some 2<generationsPerSubset>)
                 .WithGenerationsPerMergedSet(Some 3<generationsPerSubset>)
                 .WithGenerationIntervalLast(Some (UMX.tag 15))
                 .WithSorterCountPerPool(Some 1<sorterCountPerPool>)
                 .WithSelectedSorterCountPerPool(Some 1<sorterCountPerPool>)
                 .WithSorterChildCount(Some 0<sorterChildCount>)
                 .WithSorterCountCycle(None)
                 .WithSorterPoolExpansionRate(None)
                 .WithSorterEvalMeasure(Some (unsortedCountMeasure.create false |> sorterEvalMeasure.UnsortedCount))
    let config = { intervals with MaxCount = Some 16 }
    let testSet =
        [| for cut in 1 .. 10 ->
            let values = Array.init 32 (fun i -> i >= cut)
            values.[cut - 1] <- true
            values.[cut] <- false
            sortableBoolArray.create(values, 32<sortingWidth>) |]
        |> sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth>
        |> sortableTests.Bools
    // Keep a nonempty, deterministic population so evaluation caches and subset IDs
    // are checked without random child IDs obscuring exact checkpoint comparisons.
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 150 0)
                |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let memberValue = spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    let seedPool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "resume") (D1 0) [|memberValue|] 150<ceLength> (UMX.tag 0)
    let seed = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) (Some [seedPool])
    let serialize value =
        GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto value
        |> MessagePack.MessagePackSerializer.Serialize
    let restore (bytes: byte array) =
        MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(bytes)
        |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto
    let query (db: IGeneSortDb) (generation: int) kind =
        db.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag generation))) kind |> Option.get
    let saveValue (db: IGeneSortDb) mode generation kind value =
        let query = query db generation kind
        let query = if mode = geneSortDataMode.Setup then (SetupInputReferences.resolveRecorded rp query) else query
        db.saveAsync mode query value (UMX.tag false)
        |> Async.RunSynchronously |> Result.defaultWith failwith
    let saveCheckpoint db (value: sorterPoolSet_Soss) =
        saveValue db geneSortDataMode.Results (int value.GenerationNumber) (geneSortDataType.SorterPoolSet "")
            (outputData.SorterPoolSet (sorterPoolSet.Soss value))
    let initialize (db: IGeneSortDb) =
        saveValue db geneSortDataMode.Setup 0 (geneSortDataType.SortableTestsWithPrefix "")
            (outputData.SortableTestsWithPrefix(testSet, [||]))
    let loadCheckpoint (db: IGeneSortDb) generation =
        db.loadAsync geneSortDataMode.Results (query db generation (geneSortDataType.SorterPoolSet ""))
        |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
    let execute db =
        use cts = new CancellationTokenSource()
        // Deliberately stale caller generation: checkpoint discovery must supply it.
        SgdEx_Soss.evaluateEvolutionRunSoss (hostMutator host) db config config
            (rp.WithGenerationCurrent(Some 0<generationNumber>)) (UMX.tag false) cts None
        |> Async.RunSynchronously |> Result.defaultWith failwith
    let uninterrupted = MemoryDb()
    initialize uninterrupted
    saveCheckpoint uninterrupted seed
    execute uninterrupted |> ignore
    let baseline = [| for generation in 0 .. 15 -> loadCheckpoint uninterrupted generation |> serialize |]
    let partitions = SgdEx_Soss.makeSortableTestsPartitions (rp.GetRngType().Value) (rp.GetSeedSoss().Value)
                         (UMX.tag partitionCount) testSet
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    // Includes before/at/after partition transitions (2,9), Merge entry (4,11),
    // wraparound (7,14), and an already-complete checkpoint (15).
    for checkpointGeneration in 1 .. 15 do
        let checkpoint = restore baseline.[checkpointGeneration]
        Assert.Equal(checkpointGeneration, int checkpoint.GenerationNumber)
        let previousPosition = (checkpointGeneration - 1) % 7
        let expectedPrevious =
            if partitionCount = 1 then %SortableTestsPartitionId.Default
            elif previousPosition < 2 then "0"
            elif previousPosition < 4 then "1"
            else "Merge"
        let savedMember = checkpoint.SorterPools |> Map.values |> Seq.exactlyOne |> fun p -> p.SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(expectedPrevious, %savedMember.SubsetIdForSelection)
        let resumed = MemoryDb()
        initialize resumed
        saveCheckpoint resumed seed
        if not sparse then
            for earlier in 1 .. checkpointGeneration - 1 do
                saveCheckpoint resumed (restore baseline.[earlier])
        saveCheckpoint resumed checkpoint
        resumed.PoolGenerations.Clear()
        let finished = execute resumed
        Assert.Equal(Some 15<generationNumber>, finished.GetGenerationCurrent())
        Assert.Equal<(int option * int) list>(
            [for generation in checkpointGeneration + 1 .. 15 -> Some generation, generation],
            List.ofSeq resumed.PoolGenerations)
        for generation in checkpointGeneration .. 15 do
            let actual = loadCheckpoint resumed generation |> serialize
            Assert.True(baseline.[generation] = actual,
                sprintf "Resuming checkpoint %d diverged at generation %d (%d partitions)" checkpointGeneration generation partitionCount)
            if generation > checkpointGeneration then
                let position = (generation - 1) % 7
                let selected = if partitionCount = 1 then %SortableTestsPartitionId.Default
                               elif position < 2 then "0" elif position < 4 then "1" else "Merge"
                verifySavedSossReport resumed rp generation partitions prefix selected true

let private verifyBufferedSossReports (saveEvery: int) (reportEvery: int) (cancelAfter: int option) =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let host = runHost.Create (singleSource ()) 1
    let target = saveEvery * 4
    let rp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne)
                 .WithPartitionCount(Some 2<partitionCount>)
                 .WithGenerationsPerSubset(Some 2<generationsPerSubset>)
                 .WithGenerationsPerMergedSet(Some 3<generationsPerSubset>)
                 .WithSorterCountPerPool(Some 1<sorterCountPerPool>)
                 .WithSelectedSorterCountPerPool(Some 1<sorterCountPerPool>)
                 .WithSorterChildCount(Some 0<sorterChildCount>)
                 .WithSorterCountCycle(None).WithSorterPoolExpansionRate(None)
                 .WithSorterEvalMeasure(Some (unsortedCountMeasure.create false |> sorterEvalMeasure.UnsortedCount))
    let tests =
        [| for cut in 1 .. 11 ->
            let values = Array.init 32 (fun i -> i >= cut)
            values.[cut - 1] <- true
            values.[cut] <- false
            sortableBoolArray.create(values, 32<sortingWidth>) |]
        |> sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> |> sortableTests.Bools
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 150 0)
                |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let memberValue = spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    let pool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "buffered") (D1 0) [|memberValue|] 150<ceLength> (UMX.tag 0)
    let seed = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) (Some [pool])
    let config step = { intervals with Method = Constant step; MaxCount = Some (target + 1) }
    let query (db: IGeneSortDb) (generation: int) kind =
        db.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag generation))) kind |> Option.get
    let saveValue (db: IGeneSortDb) mode generation kind value =
        let query = query db generation kind
        let query = if mode = geneSortDataMode.Setup then (SetupInputReferences.resolveRecorded rp query) else query
        db.saveAsync mode query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    let load (db: IGeneSortDb) generation kind =
        db.loadAsync geneSortDataMode.Results (query db generation kind) |> Async.RunSynchronously |> Result.defaultWith failwith
    let initialize db =
        saveValue db geneSortDataMode.Setup 0 (geneSortDataType.SortableTestsWithPrefix "") (outputData.SortableTestsWithPrefix(tests, [||]))
        saveValue db geneSortDataMode.Results 0 (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet (sorterPoolSet.Soss seed))
    let execute db saveStep =
        use cts = new CancellationTokenSource()
        SgdEx_Soss.evaluateEvolutionRunSoss (hostMutator host) db (config saveStep) (config reportEvery)
            (rp.WithGenerationIntervalLast(Some (UMX.tag (target / saveStep)))) (UMX.tag false) cts None
        |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    let summaries db generation =
        let source = match load db generation (geneSortDataType.SorterPoolSetSummarySet "") with
                     | outputData.SorterPoolSetSummarySet (spSummarySet.Soss value) -> value
                     | _ -> failwith "Expected summaries"
        let bytes = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummarySetDto_Soss.toDto source |> MessagePack.MessagePackSerializer.Serialize
        MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetSummarySetDto_Soss>(bytes)
        |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummarySetDto_Soss.fromDto
    let bins db generation =
        let source = match load db generation (geneSortDataType.SorterPoolBinsSetSeries "") with
                     | outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Soss value) -> value
                     | _ -> failwith "Expected bins"
        let bytes = GeneSort.Eval.Mp.V1.Bins.Soss.SorterPoolBinsSetSeriesDto_Soss.fromDomain source |> MessagePack.MessagePackSerializer.Serialize
        MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Bins.Soss.sorterPoolBinsSetSeriesDto_Soss>(bytes)
        |> GeneSort.Eval.Mp.V1.Bins.Soss.SorterPoolBinsSetSeriesDto_Soss.toDomain
    let summaryPayload snapshot =
        GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.toDto snapshot |> MessagePack.MessagePackSerializer.Serialize |> Convert.ToHexString
    let binPayload (snapshot: GeneSort.Eval.V1.Sgd.Bins.Soss.sorterPoolBinsSet_Soss) =
        snapshot.SorterPoolEvalBinsMap |> Map.values |> Seq.map (fun poolBins ->
            let evaluations = poolBins.Bins |> Map.values |> Seq.collect (fun b -> b.SorterEvals)
                              |> Seq.map (fun e -> GeneSort.SortingOps.Mp.SorterEvalDto.fromDomain e |> MessagePack.MessagePackSerializer.Serialize |> Convert.ToHexString)
                              |> Seq.sort |> Seq.toArray
            poolBins.SorterPoolId, evaluations) |> Map.ofSeq
    // Reference saves each generation but reports on the same absolute schedule.
    let reference = MemoryDb()
    initialize reference
    execute reference 1
    let partitions = SgdEx_Soss.makeSortableTestsPartitions (rp.GetRngType().Value) (rp.GetSeedSoss().Value) 2<partitionCount> tests
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let expectedSubset generation =
        let position = (generation - 1) % 7
        if position < 2 then "0" elif position < 4 then "1" else "Merge"
    for generation in reportEvery .. reportEvery .. target do
        verifySavedSossReport reference rp generation partitions prefix (expectedSubset generation) true
    let uninterrupted = MemoryDb()
    initialize uninterrupted
    match cancelAfter with
    | None -> ()
    | Some generation ->
        Assert.NotEqual(0, generation % saveEvery)
        Assert.Equal(0, generation % reportEvery)
        use cts = new CancellationTokenSource()
        let mutable cancellationTriggered = false
        let mutator = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                      |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
        // This synchronous callback requests cancellation during the chosen step.
        // That step finishes (including its buffered report); the next loop exits.
        let log message =
            if message = sprintf "Starting evolution step. Generation %d of %d" generation target then
                cancellationTriggered <- true
                cts.Cancel()
        uninterrupted.PoolGenerations.Clear()
        let outcome =
            EvoOrch_Soss.runSossEvolutionAsync uninterrupted (config saveEvery) (config reportEvery)
                (rp.WithGenerationIntervalLast(Some (UMX.tag (target / saveEvery)))) (UMX.tag false)
                seed partitions prefix mutator cts.Token log
            |> Async.RunSynchronously
        Assert.True(cancellationTriggered)
        match outcome with
        | Error message -> Assert.Contains("cancelled", message)
        | Ok _ -> failwith "Expected cancellation"
        let lastSaved = generation / saveEvery * saveEvery
        Assert.Equal<(int option * int) list>([for g in saveEvery .. saveEvery .. lastSaved -> Some g, g], List.ofSeq uninterrupted.PoolGenerations)
        for g in lastSaved + 1 .. generation do
            for kind in [geneSortDataType.SorterPoolSet ""; geneSortDataType.SorterPoolSetSummarySet ""; geneSortDataType.SorterPoolBinsSetSeries ""] do
                let exists = (uninterrupted :> IGeneSortDb).doesOutPutDataExist geneSortDataMode.Results (query uninterrupted g kind)
                             |> Async.RunSynchronously
                Assert.False(exists)
        let checkpoint = Utils.loadHighestGenSorterPoolSetSoss (config saveEvery) uninterrupted rp
                         |> Async.RunSynchronously |> Result.defaultWith failwith |> Option.get
        Assert.Equal(lastSaved, int checkpoint.GenerationNumber)
        // Resume from serialized persisted state, with all unsaved accumulators gone.
        let bytes = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto checkpoint |> MessagePack.MessagePackSerializer.Serialize
        let restored = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(bytes)
                       |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto
        saveValue uninterrupted geneSortDataMode.Results lastSaved (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet (sorterPoolSet.Soss restored))
        uninterrupted.PoolGenerations.Clear()
    execute uninterrupted saveEvery
    match cancelAfter with
    | Some generation ->
        let lastSaved = generation / saveEvery * saveEvery
        Assert.Equal<(int option * int) list>([for g in lastSaved + saveEvery .. saveEvery .. target -> Some g, g], List.ofSeq uninterrupted.PoolGenerations)
    | None -> ()
    // Check uninterrupted execution and resume from each saved checkpoint,
    // including checkpoints between reports and an already-completed run.
    for resumeAt in 0 .. saveEvery .. target do
        let db =
            if resumeAt = 0 then uninterrupted
            else
                let resumed = MemoryDb()
                initialize resumed
                let source = load uninterrupted resumeAt (geneSortDataType.SorterPoolSet "") |> OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
                let bytes = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto source |> MessagePack.MessagePackSerializer.Serialize
                let restored = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(bytes)
                               |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto
                saveValue resumed geneSortDataMode.Results resumeAt (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet (sorterPoolSet.Soss restored))
                resumed.PoolGenerations.Clear()
                execute resumed saveEvery
                Assert.Equal<(int option * int) list>([for g in resumeAt + saveEvery .. saveEvery .. target -> Some g, g], List.ofSeq resumed.PoolGenerations)
                resumed
        for savedGeneration in resumeAt + saveEvery .. saveEvery .. target do
            let expected = [| for g in savedGeneration - saveEvery + 1 .. savedGeneration do if g % reportEvery = 0 then yield g |]
            let summarySet = summaries db savedGeneration
            let binSeries = bins db savedGeneration
            let summarySnapshots = summarySet.SorterPoolSetSummaries |> Array.sortBy (fun s -> int s.GenerationNumber)
            let binSnapshots = binSeries.SorterPoolEvalBinsSets |> Map.values |> Seq.sortBy (fun s -> int s.GenerationNumber) |> Seq.toArray
            Assert.Equal(savedGeneration, int summarySet.LastGeneration)
            Assert.True((expected = (summarySnapshots |> Array.map (fun s -> int s.GenerationNumber))))
            Assert.True((expected = (binSnapshots |> Array.map (fun s -> int s.GenerationNumber))))
            for index in 0 .. expected.Length - 1 do
                let generation = expected.[index]
                let expectedSummary = (summaries reference generation).SorterPoolSetSummaries |> Array.exactlyOne
                let expectedBins = (bins reference generation).SorterPoolEvalBinsSets |> Map.values |> Seq.exactlyOne
                Assert.Equal(summaryPayload expectedSummary, summaryPayload summarySnapshots.[index])
                Assert.True((binPayload expectedBins = binPayload binSnapshots.[index]))
                Assert.True(binSnapshots.[index].SorterPoolSetId = summarySnapshots.[index].SorterPoolSetId)
                for poolBins in binSnapshots.[index].SorterPoolEvalBinsMap |> Map.values do
                    for bin in poolBins.Bins |> Map.values do
                        for evaluation in bin.SorterEvals do
                            Assert.Equal(expectedSubset generation, %(SorterEval.getSortableTestsSubsetId evaluation))

[<Theory>]
[<InlineData(5, 1)>]
[<InlineData(5, 2)>]
[<InlineData(3, 7)>]
[<InlineData(7, 3)>]
let ``Soss buffered reports retain their own generations and subsets across saves and resume`` saveEvery reportEvery =
    verifyBufferedSossReports saveEvery reportEvery None

[<Theory>]
[<InlineData(5, 1, 6)>]
[<InlineData(5, 1, 7)>]
[<InlineData(5, 1, 8)>]
[<InlineData(5, 1, 9)>]
[<InlineData(3, 1, 4)>]
[<InlineData(3, 1, 5)>]
[<InlineData(5, 2, 8)>]
let ``Soss cancellation discards unsaved work and resume regenerates reports on the original cycle`` saveEvery reportEvery cancelAfter =
    verifyBufferedSossReports saveEvery reportEvery (Some cancelAfter)

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(2)>]
let ``Soss checkpoints label the produced generation and stop exactly at the target`` startingGeneration =
    let sourceRun = singleSource ()
    let host = runHost.Create sourceRun 1
    let rp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne)
                 .WithGenerationIntervalLast(Some (UMX.tag 2))
    let mutator = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
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
                           |> Array.map (fun rp ->
                               if chained then
                                   rp.ParamMap.Add(ParamKeys.setupSortableTestsWithPrefixIdKey, string (Guid.NewGuid())) |> runParameters.create
                               else rp)
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
        (sourceSetupQuery sourceHost (sourceRp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix ""))
        (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    let destinationMemory = MemoryDb(destinationHost.QueryParamsFromRunParams)
    let host = destinationHost.WithRunDb destinationMemory
    use cts = new CancellationTokenSource()
    for rp in branches do
        let updated =
            importStart sourceDb host.Run host.Storage rp (UMX.tag false) cts None
            |> Async.RunSynchronously |> Result.defaultWith failwith
        let imported =
            GeneSort.Project.V1.Catalogs.Evolution.Utils.loadSetupOutputData host.RunDb updated (geneSortDataType.SorterPoolSet "")
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
    let mutable sourceRun = if soss then singleSource () else standardSource ()
    let mutable sourceHost = runHost.Create sourceRun 1
    let mutable sourceRp = sourceHost.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let mutable sourceMemory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
    let persist (db: IGeneSortDb) mode query data =
        db.saveAsync mode query data (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist sourceMemory geneSortDataMode.Setup
        (sourceSetupQuery sourceHost sourceRp (geneSortDataType.SortableTestsWithPrefix ""))
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
            evoContinuationConfig.create {
                ProjectName = UMX.tag (sprintf "ExplicitChain%d" interval)
                DatabaseName = sourceRun.DatabaseName
                RunName = UMX.tag "Branch"
                RunQueryCatalogName = if soss then QueryCatalogNamesRun.sorterSgdMsufSossAppend else QueryCatalogNamesRun.sorterSgdMsufStandardAppend
                Source = sourceSelection sourceRun interval [ParamKeys.sourceMutationModKey, [string %(sourceRp.GetMutationMod().Value)]]
                ExecutionParameterSpans = [ ParamKeys.mutationModKey, [interval; interval + 10] |> List.map string; ParamKeys.generationIntervalLastKey, [string (interval + 1)] ]
            }
            |> (if soss then EvoAppend.createSoss else EvoAppend.createStandard)
        let configured = runHost.Create nextRun 1
        let memory = MemoryDb(configured.QueryParamsFromRunParams)
        let host = configured.WithRunDb memory
        let prepared =
            host.MakeRunParameters 0<replNumber> 1<replNumber>
            |> Array.map (fun rp ->
                let metadataBefore = host.QueryParamsFromRunParams rp (geneSortDataType.RunParameters %nextRun.RunName) |> Option.get
                let updated =
                    importStart sourceMemory host.Run host.Storage rp (UMX.tag false) cts None
                    |> Async.RunSynchronously |> Result.defaultWith failwith
                let metadataAfter = host.QueryParamsFromRunParams updated (geneSortDataType.RunParameters %nextRun.RunName) |> Option.get
                Assert.Equal(metadataBefore.Id, metadataAfter.Id)
                let setupQuery =
                    host.QueryParamsFromRunParams updated (geneSortDataType.SorterPoolSet "") |> Option.get |> fun query -> SetupInputReferences.resolveRecorded updated query
                let expectedId = checkpointQuery.Id
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
let ``Append source branching shares one database copy of each checkpoint and test file`` soss =
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
                standardSource () |> withSetupSpans [ParamKeys.mutationModKey, ["0"; "1"; "2"]]
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
                (sourceSetupQuery sourceHost sourceRp (geneSortDataType.SortableTestsWithPrefix ""))
                (outputData.SortableTestsWithPrefix(tests, [||]))
        let configured =
            evoContinuationConfig.create {
                ProjectName = UMX.tag "Tests.Append"
                DatabaseName = sourceRun.DatabaseName
                RunName = UMX.tag "ExplicitAppend"
                RunQueryCatalogName = if soss then QueryCatalogNamesRun.sorterSgdMsufSossAppend else QueryCatalogNamesRun.sorterSgdMsufStandardAppend
                Source = sourceSelection sourceRun 1 [ParamKeys.sourceMutationModKey, ["0"; "1"; "2"]]
                ExecutionParameterSpans = [ ParamKeys.mutationModKey, [0 .. 15] |> List.map string; ParamKeys.sorterCountPerPoolSetKey, [sourcePoolCount * perPool * 2] |> List.map string; ParamKeys.generationIntervalLastKey, ["2"] ]
            }
            |> (if soss then EvoAppend.createSoss else EvoAppend.createStandard)
        // Round-trip the entire source-generation configuration, including nested append sources.
        let configured = configured |> GeneSort.Project.Mp.V1.RunDto.fromDomain
                         |> MessagePack.MessagePackSerializer.Serialize
                         |> MessagePack.MessagePackSerializer.Deserialize<GeneSort.Project.Mp.V1.runDto>
                         |> GeneSort.Project.Mp.V1.RunDto.toDomain
        let expectedCount = 96
        let destinationRoot, db = makeDb configured
        let host = (runHost.Create configured 1).WithRunDb db
        let pending = host.MakeRunParameters 0<replNumber> 2<replNumber>
        Assert.Equal(expectedCount, pending.Length)
        Assert.Equal(expectedCount, pending |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
        for rp in pending do
            Assert.Contains(rp.ParamMap.[ParamKeys.sourceExecutionIdKey], sourceParameters |> Array.map (fun source -> string %(source.GetId().Value)))
            Assert.Equal(sourcePoolCount, int (rp.GetSorterPoolCount().Value))
            Assert.Equal(int (rp.GetSorterCountPerPoolSet().Value) / sourcePoolCount, int (rp.GetSorterCountPerPool().Value))
        for rp in pending do
            persist db geneSortDataMode.Project
                (host.QueryParamsFromRunParams rp (geneSortDataType.RunParameters %configured.RunName) |> Option.get) (outputData.RunParameters rp)
        use cts = new CancellationTokenSource()
        let invalid = pending.[0].WithSorterCountPerPool(Some 0<sorterCountPerPool>)
        let result = importStart sourceDb host.Run host.Storage invalid (UMX.tag false) cts None
                     |> Async.RunSynchronously
        Assert.True(Result.isError result)
        let prepared =
            pending |> Array.map (fun rp -> importStart sourceDb host.Run host.Storage rp (UMX.tag false) cts None)
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
        Assert.Equal(sourceParameters.Length + 1, files.Length)
        Assert.Single(loaded |> Array.map (fun rp -> rp.GetSetupSortableTestsWithPrefixId()) |> Array.distinct) |> ignore
        Assert.All(files, fun path -> Assert.Contains(System.IO.Directory.GetParent(path).Name, [|"SorterPoolSet"; "SortableTestsWithPrefix"|]))
        Assert.All(loaded |> Array.groupBy (fun rp -> rp.GetRepl(), rp.ParamMap.[ParamKeys.setupSorterPoolSetIdKey]),
            fun (_, branches) -> Assert.Equal(16, branches.Length))
        for rp in loaded do
            let savedData = GeneSort.Project.V1.Catalogs.Evolution.Utils.loadSetupOutputData reopened rp (geneSortDataType.SorterPoolSet "")
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
                let execution = GeneSort.Project.V1.Catalogs.Evolution.EvoAppendPools.prepareStandard rp saved |> Result.defaultWith failwith
                Assert.Equal(sourcePoolCount, saved.SorterPools.Count)
                Assert.Equal(int (rp.GetSorterPoolCount().Value), execution.SorterPools.Count)
                Assert.All(execution.SorterPools.Values, fun pool -> Assert.Equal(rp.GetMutationMod().Value, pool.MutationMod))
            GeneSort.Project.V1.Catalogs.Evolution.Utils.loadSortableTestsWithPrefix reopened rp
            |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
        // A final Results checkpoint must bypass Setup preparation entirely, even when the checkpoint is empty (which fresh-branch preparation rejects).
        let rp = loaded.[0]
        let finalGeneration = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value 2 |> Option.get
        let resumedRp = rp.WithGenerationCurrent(Some (UMX.tag finalGeneration))
        let finalQuery = host.QueryParamsFromRunParams resumedRp (geneSortDataType.SorterPoolSet "") |> Option.get
        persist db geneSortDataMode.Results finalQuery (outputData.SorterPoolSet(pool soss finalGeneration))
        let result = host.ExecuteFunction rp (UMX.tag false) cts None
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
    let sourceRp =
        if chained then sourceRp.ParamMap.Add(ParamKeys.setupSortableTestsWithPrefixIdKey, string (Guid.NewGuid())) |> runParameters.create
        else sourceRp
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
    let sourceTestsQuery = sourceSetupQuery sourceHost (sourceRp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "")
    let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
    persist geneSortDataMode.Setup sourceTestsQuery (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    let originalWrites = sourceMemory.Writes.Count
    use cts = new CancellationTokenSource()
    let result = importStart sourceDb host.Run host.Storage rp (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(originalWrites, sourceMemory.Writes.Count)
    if scenario <> "valid" && scenario <> "retired-configuration" && not chained then
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
        let importedGeneration = generation
        Assert.Equal(Some (UMX.tag<generationNumber> importedGeneration), updated.GetGenerationFirst())
        Assert.Equal(updated.GetGenerationFirst(), updated.GetGenerationCurrent())
        Assert.Equal(Some false, updated.GetRunFinished())
        Assert.Equal(2, destinationMemory.Writes.Count)
        Assert.All(destinationMemory.Writes, fun (mode, _) -> Assert.Equal(geneSortDataMode.Setup, mode))
        let saved = GeneSort.Project.V1.Catalogs.Evolution.Utils.loadSetupOutputData host.RunDb updated (geneSortDataType.SorterPoolSet "")
                    |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
        Assert.Equal(importedGeneration, %saved.GenerationNumber)
        let loadedTests = GeneSort.Project.V1.Catalogs.Evolution.Utils.loadSortableTestsWithPrefix host.RunDb updated |> Async.RunSynchronously
        Assert.True(Result.isOk loadedTests)
        let query = host.QueryParamsFromRunParams rp (geneSortDataType.Run appendRun.RunName) |> Option.get
        let other = host.QueryParamsFromRunParams (rp.WithSourceGenerationInterval(Some lastInterval)) (geneSortDataType.Run appendRun.RunName) |> Option.get
        Assert.NotEqual(query.Id, other.Id)
        let evolvedGeneration = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value lastInterval |> Option.get
        let evolvedRp = updated.WithGenerationCurrent(Some (UMX.tag evolvedGeneration))
        let evolvedQuery = host.QueryParamsFromRunParams evolvedRp (geneSortDataType.SorterPoolSet "") |> Option.get
        host.RunDb.saveAsync geneSortDataMode.Results evolvedQuery (outputData.SorterPoolSet(pool true evolvedGeneration)) (UMX.tag false)
        |> Async.RunSynchronously |> Result.defaultWith failwith
        let resumed = GeneSort.Project.V1.Catalogs.Evolution.Utils.loadHighestGenSorterPoolSetSoss host.GenSaveIntervals.Value host.RunDb updated
                      |> Async.RunSynchronously |> Result.defaultWith failwith |> Option.get
        Assert.Equal(evolvedGeneration, %resumed.GenerationNumber)
        let reports = GeneSort.Project.V1.Catalogs.Evolution.Utils.loadAvailableSorterPoolSets host.GenSaveIntervals.Value host.RunDb (UMX.tag 0) updated cts.Token ignore
                      |> Async.RunSynchronously |> Seq.toArray
        Assert.Single(reports) |> ignore

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``standalone tests have no setup and generate Results during execution`` prefix =
    let memory = MemoryDb()
    let spec = if prefix then WorkflowFixtures.sortableTestsRun true else WorkflowFixtures.sortableTestsRun false
    let host = (runHost.Create spec 1).WithRunDb(memory)
    let rp =
        let basic = (runParameters.create Map.empty).WithRepl(Some 0<replNumber>).WithSortableDataFormat(Some sortableDataFormat.BoolArray)
        if prefix then basic.WithPrefixLibId(Some (prefixLibId.create 8<sortingWidth> 3<stageLength> prefixLibVariant.PrefixA))
        else basic.WithMergeLibId(Some (mergeLibId.create 4<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA))
    use cts = new CancellationTokenSource()
    let prepared = host.SetupExecutor rp (UMX.tag false) cts None |> Async.RunSynchronously
    match prepared with
    | Ok unchanged -> Assert.Equal(rp, unchanged)
    | Error error -> failwith error
    Assert.Empty(memory.Reads)
    Assert.Empty(memory.Writes)
    let executed = host.ExecuteFunction rp (UMX.tag false) cts None |> Async.RunSynchronously
    match executed with
    | Ok finished -> Assert.Equal(Some true, finished.GetRunFinished())
    | Error error -> failwith error
    Assert.Empty(memory.Reads)
    Assert.Equal<(geneSortDataMode * geneSortDataType) list>(
        [geneSortDataMode.Results, geneSortDataType.SortableTests ""], List.ofSeq memory.Writes)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``evolution setup reuses recorded files across activator rates without Run queries`` soss =
    let project = UMX.tag<projectName> "RecordedSetup"
    let database = UMX.tag<databaseName> "SharedStarts"
    let testsId = Guid.Parse "10000000-0000-0000-0000-000000000001" |> UMX.tag<queryParamsId>
    let poolId = Guid.Parse "20000000-0000-0000-0000-000000000002" |> UMX.tag<queryParamsId>
    let recorded = parameters.WithSetupSortableTestsWithPrefixId(Some testsId).WithSetupSorterPoolSetId(Some poolId)
    let memory = MemoryDb(fun _ _ -> failwith "Setup must not build a Run query.")
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
    let prepare = if soss then EvoStartCreation.createSossStart else EvoStartCreation.createStandardStart
    for rate in [0.0; 0.01] do
        let rp = recorded.WithActivatorMutationRate(Some rate)
        let result = prepare project database makeTests makePool db rp cts.Token |> Async.RunSynchronously
        match result with
        | Ok start ->
            Assert.Equal(0, int start.FirstGeneration)
            Assert.Equal(testsId, start.Tests.Id)
            Assert.Equal(poolId, start.Pool.Id)
            for query in [start.Tests; start.Pool] do
                // This is the address returned by the previous Run-query-then-reference flow.
                let oldQuery = queryParams.create database project (Some 0<replNumber>) (Some 0<generationNumber>) query.OutputDataType [|"ExecutionRate", string rate|]
                               |> SetupInputReferences.resolveRecorded rp
                Assert.Equal(oldQuery, query)
                let path value = GeneSort.FileDb.V1.OutputDataFile.getFullOutputDataFilePath (UMX.tag "C:\\RecordedSetup\\Data") geneSortDataMode.Setup value
                Assert.Equal<string>(string (path oldQuery), string (path query))
        | Error error -> failwith error
    Assert.Equal(1, testCalls)
    Assert.Equal(1, poolCalls)
    Assert.Equal<list<geneSortDataMode * geneSortDataType>>(
        [geneSortDataMode.Setup, geneSortDataType.SortableTestsWithPrefix ""; geneSortDataMode.Setup, geneSortDataType.SorterPoolSet ""],
        List.ofSeq memory.Writes)

[<Theory>]
[<InlineData(false, "SetupSortableTestsWithPrefixId", false)>]
[<InlineData(false, "SetupSortableTestsWithPrefixId", true)>]
[<InlineData(false, "SetupSorterPoolSetId", false)>]
[<InlineData(false, "SetupSorterPoolSetId", true)>]
[<InlineData(true, "SetupSortableTestsWithPrefixId", false)>]
[<InlineData(true, "SetupSortableTestsWithPrefixId", true)>]
[<InlineData(true, "SetupSorterPoolSetId", false)>]
[<InlineData(true, "SetupSorterPoolSetId", true)>]
let ``start creation rejects missing or invalid recorded references before accessing storage`` soss key malformed =
    let recorded = parameters.WithSetupSortableTestsWithPrefixId(Some (UMX.tag (Guid.NewGuid())))
                             .WithSetupSorterPoolSetId(Some (UMX.tag (Guid.NewGuid())))
    let invalid =
        (if malformed then recorded.ParamMap.Add(key, "not-a-guid") else recorded.ParamMap.Remove key)
        |> runParameters.create
    let memory = MemoryDb(fun _ _ -> failwith "Setup must not build a Run query.")
    let prepare = if soss then EvoStartCreation.createSossStart else EvoStartCreation.createStandardStart
    use cts = new CancellationTokenSource()
    let result = prepare (UMX.tag "RecordedSetup") (UMX.tag "SharedStarts")
                     (fun _ -> failwith "Tests must not be generated without references.")
                     (fun _ -> failwith "A seed must not be generated without references.")
                     memory invalid cts.Token |> Async.RunSynchronously
    match result with
    | Error message -> Assert.Contains(key, message)
    | Ok _ -> failwith "Expected invalid Setup reference to fail."
    Assert.Empty(memory.Reads)
    Assert.Empty(memory.Writes)

[<Fact>]
let ``Soss seed setup labels its full test evaluation Merge rather than Default`` () =
    let model =
        msce.create (UMX.tag (Guid.NewGuid())) 2<sortingWidth> [|1|]
        |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let memberValue = spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
    let seedPool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "seed") (D1 0) [|memberValue|] 1<ceLength> (UMX.tag 0)
    let seed = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) (Some [seedPool])
    let fullTests =
        sortableBinaryTests.create (UMX.tag Guid.Empty) 2<sortingWidth>
            [|sortableBoolArray.create([|true; false|], 2<sortingWidth>)|] |> sortableTests.Bools
    let result =
        EvoStartCreation.evaluateSossSeed parameters fullTests
            (ceBlock.create (UMX.tag Guid.Empty) 2<sortingWidth> [||]) (sorterPoolSet.Soss seed)
        |> Result.bind SorterPoolSet.asSoss |> Result.defaultWith failwith
    let evaluated = result.SorterPools |> Map.values |> Seq.exactlyOne |> fun p -> p.SorterPoolMembers |> Seq.exactlyOne
    Assert.Equal(1, evaluated.SorterEvalMap.Count)
    Assert.True(Map.containsKey CommonSgdSoss.mergedSubsetId evaluated.SorterEvalMap)
    Assert.False(Map.containsKey SortableTestsSubsetId.Default evaluated.SorterEvalMap)

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
        Map.ofList [ParamKeys.sourceExecutionIdKey, string (Guid.NewGuid())]
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
    let rp = Map.ofList [ParamKeys.sourceExecutionIdKey, "selected"] |> runParameters.create
    let rp = rp.WithMutationMod(Some (UMX.tag 0)).WithSorterCountPerPoolSet(Some (UMX.tag total)).WithSorterCountPerPool(Some (UMX.tag perPool))
    Assert.True(SgdEx_Soss.prepareImportedPool rp source |> Result.isError)

[<Fact>]
let ``Explicit chained appends retain each immediate source identity across serialization`` () =
    GeneSort.Core.Mp.MessagePackSetup.configure()
    let mutable source = sourceWithMutationMods [0; 1; 2]
    for interval in 1 .. 3 do
        let previous = RunParameterGeneration.generate source 0<replNumber> 2<replNumber>
        let configured =
            evoContinuationConfig.create {
                ProjectName = UMX.tag (sprintf "ExplicitAppend%d" interval)
                DatabaseName = source.DatabaseName
                RunName = UMX.tag "Branch"
                RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
                Source = sourceSelection source interval []
                ExecutionParameterSpans = [ParamKeys.mutationModKey, ["7"; "8"]; ParamKeys.generationIntervalLastKey, [string (interval + 1)]]
            }
            |> EvoAppend.createSoss
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
let ``Append combines three sources with three modifiers and a single size override`` () =
    let configured = appendFrom (sossSource [0; 1; 2]) 1 2 [] [
        ParamKeys.mutationModKey, ["0"; "1"; "2"]
        ParamKeys.sorterCountPerPoolSetKey, ["64"]
    ]
    let branches = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
    Assert.Equal(9, branches.Length)
    Assert.Equal(9, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    Assert.Equal("Tests.Append1", string configured.ProjectName)

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
    let host = runHost.Create (standardSource ()) 1
    let rp = (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne).WithGenerationIntervalLast(Some (UMX.tag 2))
    let mutator = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                  |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    let initial = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag startingGeneration) (Dim1 1) None
    let memory = MemoryDb()
    let config = { intervals with MaxCount = Some 3 }
    let final = EvoOrch_Standard.runStandardEvolutionAsync memory config config rp (UMX.tag false) initial tests
                    (ceBlock.create (UMX.tag Guid.Empty) 2<sortingWidth> [||]) mutator CancellationToken.None ignore
                |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(2, int final.GenerationNumber)
    Assert.Equal<(int option * int) list>([for gen in startingGeneration + 1 .. 2 -> Some gen, gen], List.ofSeq memory.PoolGenerations)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Pool evaluation preserves copied models and their pool-specific lengths`` reEvaluateParents =
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 200 1)
                |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let tinyTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
                        [|sortableBoolArray.create(Array.create 32 false, 32<sortingWidth>)|] |> sortableTests.Bools
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let standardPools =
        [| for index in 0 .. 3 do
            let memberValue = spMember_Standard.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None None 0<generationNumber>
            yield sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "copy") (D1 index)
                [|memberValue|] (UMX.tag (150 + index * 10)) (UMX.tag 0) |]
    let standardSet (pools: sorterPool_Standard[]) = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 pools.Length) (Some pools)
    let evaluateStandard = PoolRunner_Standard.evaluatePoolSet tinyTests prefix sorterEvalType.V2 reEvaluateParents (UMX.tag false)
    let standard = standardSet standardPools
    let evaluations = evaluateStandard standard
    Assert.Equal(4, evaluations.Count)
    for pool in standardPools do
        let memberId = pool.SorterPoolMembers |> Seq.exactlyOne |> fun m -> m.SorterPoolMemberId
        let expected = evaluateStandard (standardSet [|pool|])
        Assert.Equal(expected.[memberId], evaluations.[memberId])
    let updated = SorterPoolSet_Standard.updateSorterEvals evaluations standard
    Assert.All(updated.SorterPools.Values, fun pool -> Assert.Single(pool.SorterPoolMembers) |> ignore)
    let sossPools =
        [| for index in 0 .. 3 do
            let memberValue = spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None Map.empty 0<generationNumber>
            yield sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "copy") (D1 index)
                [|memberValue|] (UMX.tag (150 + index * 10)) (UMX.tag 0) |]
    let sossSet (pools: sorterPool_Soss[]) = sorterPoolSet_Soss.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 pools.Length) (Some pools)
    let evaluateSoss = PoolRunner_Soss.evaluatePoolSet tinyTests prefix sorterEvalType.V2 reEvaluateParents (UMX.tag false)
    let evaluations = evaluateSoss (sossSet sossPools)
    Assert.Equal(4, evaluations.Count)
    for pool in sossPools do
        let memberId = pool.SorterPoolMembers |> Seq.exactlyOne |> fun m -> m.SorterPoolMemberId
        let expected = evaluateSoss (sossSet [|pool|])
        Assert.Equal(expected.[memberId], evaluations.[memberId])

[<Fact>]
let ``Current Standard append config produces three single-pool runs at its single overridden capacity`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Standard_append.Test ()
    let parameters = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
    Assert.Equal(3, parameters.Length)
    Assert.Equal(3, parameters |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    Assert.All(parameters, fun rp ->
        Assert.Equal("1.25", rp.ParamMap.[ParamKeys.paraRateKey])
        Assert.Equal("2.25", rp.ParamMap.[ParamKeys.selfSymRateKey]))
    for capacity in [128] do
        let matching = parameters |> Array.filter (fun rp -> int (rp.GetSorterCountPerPool().Value) = capacity)
        Assert.Equal(3, matching.Length)
        Assert.Equal<Set<int>>(set [0; 1; 2], matching |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Set.ofArray)
        Assert.All(matching, fun rp ->
            Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
            Assert.Equal(capacity, int (rp.GetSorterCountPerPoolSet().Value))
            Assert.Equal(capacity, int (rp.GetSelectedSorterCountPerPool().Value)))

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``OnePool append test presets continue their bases and preserve one pool of 32 sorters`` soss =
    let configured =
        if soss then GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.Continuation.TestSossAppend ()
        else GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.Continuation.TestStandardAppend ()
    let source, spans = configured.ParameterSource.Value
    Assert.Equal((if soss then "TestSoss" else "TestStandard"), %source.RunName)
    Assert.Equal("SorterSgd.RunCfg.p32.Test.OnePool", %configured.ProjectName)
    Assert.Equal("Continuation", %configured.DatabaseName)
    Assert.Contains((ParamKeys.sourceMutationModKey, ["0"]), spans)
    let parameters = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
    Assert.Equal(3, parameters.Length)
    for capacity in [32] do
        let matching = parameters |> Array.filter (fun rp -> int (rp.GetSorterCountPerPool().Value) = capacity)
        Assert.Equal(3, matching.Length)
        Assert.All(matching, fun rp ->
            Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
            Assert.Equal("0", rp.ParamMap.[ParamKeys.sourceGenerationIntervalKey])
            Assert.Equal("1", rp.ParamMap.[ParamKeys.generationIntervalLastKey]))

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``OnePool condense test presets select the best two of four source candidates`` soss =
    let configured =
        if soss then GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.Continuation.TestSossCondense ()
        else GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.Continuation.TestStandardCondense ()
    Assert.Equal((if soss then ExecuteFunctionNames.soss else ExecuteFunctionNames.standard), configured.ExecuteFunctionName)
    Assert.Equal(QueryCatalogNamesSetup.importedStart, configured.SetupQueryCatalogName)
    Assert.Equal((if soss then QueryCatalogNamesRun.sorterSgdMsufSossAppend else QueryCatalogNamesRun.sorterSgdMsufStandardAppend), configured.RunQueryCatalogName)
    let source, spans = configured.ParameterSource.Value
    Assert.Equal((if soss then "TestSoss" else "TestStandard"), %source.RunName)
    Assert.Contains((ParamKeys.selectedSorterPoolCountKey, ["2"]), spans)
    Assert.Contains((ParamKeys.sourceMutationModKey, ["0"; "1"; "2"; "3"]), spans)
    let candidates = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
    Assert.Equal(4, candidates.Length)
    Assert.Equal<Set<int>>(set [0; 1; 2; 3], candidates |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Set.ofArray)
    Assert.All(candidates, fun candidate ->
        Assert.Equal(Some 1<sorterPoolCount>, candidate.GetSorterPoolCount())
        Assert.Equal(32, int (candidate.GetSorterCountPerPool().Value)))

[<Fact>]
let ``Append execution spans override inherited values and enhancer defaults`` () =
    let overrides = [
        ParamKeys.paraRateKey, ["1.25"]
        ParamKeys.selfSymRateKey, ["2.25"]
        ParamKeys.sorterChildCountKey, ["3"]
        ParamKeys.distinctSorterHashesKey, ["False"]
        ParamKeys.selectedSorterCountPerPoolKey, ["8"]
    ]
    let source = standardSource ()
    let original = RunParameterGeneration.generate source 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let configured = appendFrom source 1 3 [] overrides
    let generated = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
    Assert.Equal(1, generated.Length)
    Assert.Equal(1, generated |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    for rp in generated do
        for key, values in overrides do
            Assert.Contains(rp.ParamMap.[key], values)
        Assert.Equal(original.ParamMap.[ParamKeys.modificationRateKey], rp.ParamMap.[ParamKeys.modificationRateKey])
        Assert.Equal(original.GetId().Value.ToString(), rp.ParamMap.[ParamKeys.sourceExecutionIdKey])

[<Theory>]
[<InlineData(256, 64)>]
[<InlineData(64, 16)>]
let ``Append total preserves multiple source pools when growing or shrinking capacity`` total perPool =
    let configured = appendFrom (standardSource ()) 1 3 [] [ParamKeys.sorterCountPerPoolSetKey, [string total]]
    let rp = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> Array.exactlyOne
    Assert.Equal(Some 4<sorterPoolCount>, rp.GetSorterPoolCount())
    Assert.Equal(perPool, int (rp.GetSorterCountPerPool().Value))

[<Theory>]
[<InlineData(0)>]
[<InlineData(129)>]
let ``Append rejects totals that cannot fill source pools evenly`` total =
    let configured = appendFrom (standardSource ()) 1 3 [] [ParamKeys.sorterCountPerPoolSetKey, [string total]]
    let error = Assert.Throws<ArgumentException>(fun () -> RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> ignore)
    Assert.Contains("divisible by the source SorterPoolCount", error.Message)

[<Fact>]
let ``Standard append starts from imported setup and resumes its resulting checkpoint`` () =
    let sourceRun = standardSource () |> withSetupSpans [ParamKeys.sorterCountPerPoolSetKey, ["32"]]
    let configured = appendFrom sourceRun 1 3 [] [
        ParamKeys.mutationModKey, ["0"; "1"; "2"]
        ParamKeys.sorterCountPerPoolSetKey, ["256"]
    ]
    let host = runHost.Create configured 1
    let pending = host.MakeRunParameters 0<replNumber> 1<replNumber>
    Assert.Equal(3, pending.Length)
    Assert.Equal(3, pending |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let rp = pending |> Array.find (fun rp -> int (rp.GetMutationMod().Value) = 1 && int (rp.GetSorterCountPerPoolSet().Value) = 256)
    let poolSourceId = Guid.NewGuid() |> UMX.tag<queryParamsId>
    let testsSourceId = Guid.NewGuid() |> UMX.tag<queryParamsId>
    let poolQuery = queryParams.createForImportedFile configured.DatabaseName configured.ProjectName (rp.GetRepl()) (geneSortDataType.SorterPoolSet "") poolSourceId
    let testsQuery = queryParams.createForImportedFile configured.DatabaseName configured.ProjectName (rp.GetRepl()) (geneSortDataType.SortableTestsWithPrefix "") testsSourceId
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
    let execute () = SgdEx_Standard.evaluateEvolutionRunStandard (hostMutator host) db config summaries rp (UMX.tag false) cts None
                     |> Async.RunSynchronously |> Result.defaultWith failwith
    let completed = execute ()
    Assert.Equal(Some true, completed.GetRunFinished())
    Assert.Equal(Some 2<generationNumber>, completed.GetGenerationCurrent())
    let saved = GeneSort.Project.V1.Catalogs.Evolution.Utils.loadHighestGenSorterPoolSet config db rp
                |> Async.RunSynchronously |> Result.defaultWith failwith |> Option.get
    Assert.Equal(2, int saved.GenerationNumber)
    Assert.Equal(1, saved.SorterPools.Count)
    Assert.All(saved.SorterPools.Values, fun pool -> Assert.Equal(1, int pool.MutationMod))
    Assert.All(saved.SorterPools.Values, fun pool -> Assert.NotEmpty(pool.SorterPoolMembers))
    let writes = memory.Writes.Count
    execute () |> ignore
    Assert.Equal(writes, memory.Writes.Count)
    let imported = db.loadAsync geneSortDataMode.Setup poolQuery |> Async.RunSynchronously |> Result.bind OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith
    Assert.Equal(source, imported)

[<Theory>]
[<InlineData(false, 384)>]
[<InlineData(true, 240)>]
let ``Standard parameter sweeps append each source combination once`` v2 expected =
    let source = standardSweep v2
    let appended = appendFrom source 3 4 [] []
    let original = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
    let branches = RunParameterGeneration.generate appended 0<replNumber> 1<replNumber>
    Assert.Equal(expected, original.Length)
    Assert.Equal(expected, branches.Length)
    Assert.Equal(expected, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    Assert.Equal<string>(%source.DatabaseName, %appended.DatabaseName)

[<Fact>]
let ``Append interval validation identifies the run and conflicting values`` () =
    let source = standardSource ()
    let error = Assert.Throws<ArgumentException>(fun () -> appendFrom source 1 1 [] [] |> ignore)
    Assert.Contains("Append run 'Append'", error.Message)
    Assert.Contains(ParamKeys.generationIntervalLastKey + " [1]", error.Message)
    Assert.Contains("greater than Source.GenerationInterval (1)", error.Message)
    Assert.Contains(%source.RunName, error.Message)

let private condenseFixtureFor soss (scenario: string) =
    let source = (if soss then sossSource [0; 1; 2; 3] else standardSource ()) |> withSetupSpans [
        ParamKeys.mutationModKey, ["0"; "1"; "2"; "3"]
        ParamKeys.sorterCountPerPoolKey, ["32"]
        ParamKeys.sorterCountPerPoolSetKey, ["32"]
    ]
    let source =
        if scenario.StartsWith("partition-") then
            let dto = GeneSort.Project.Mp.V1.RunDto.fromDomain source
            let spans = dto.RunParameterSpans |> Array.map (fun span ->
                if span.Key = ParamKeys.partitionCountKey then { span with Values = [|"1"; "4"|] }
                elif scenario = "partition-combinations" && span.Key = ParamKeys.paraRateKey then { span with Values = [|"1.15"; "1.25"|] }
                else span)
            { dto with RunParameterSpans = spans }
            |> GeneSort.Project.Mp.V1.RunDto.toDomain
        else source
    let sourceHost = runHost.Create source 1
    let candidates = sourceHost.MakeRunParameters 0<replNumber> 2<replNumber>
    let memory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    let db = memory :> IGeneSortDb
    memory.SavedParameters <- candidates
    let persist mode query value =
        db.saveAsync mode query value (UMX.tag true) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist geneSortDataMode.Project (queryParams.createForRun source.DatabaseName source.ProjectName source.RunName) (outputData.Run source)
    let generation = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value 1 |> Option.get
    for rp in candidates do
        let modifier = int (rp.GetMutationMod().Value)
        let replica = int (rp.GetRepl().Value)
        let partition = rp.GetPartitionCount() |> Option.map int |> Option.defaultValue 0
        let subset = (if scenario.StartsWith("partition-") then string partition
                      elif scenario = "different-subsets" && modifier = 3 then "other" else "all") |> UMX.tag<sortableTestsSubsetId>
        let lengths =
            if scenario.StartsWith("partition-") then
                let score =
                    if scenario = "partition-combinations" && rp.ParamMap.[ParamKeys.paraRateKey] = "1.25" then
                        if modifier = 1 || modifier = 2 then 200 + modifier else 300 + modifier
                    elif partition = 1 then modifier + 1 else 100 - modifier
                [score; score]
            elif scenario = "ties" then [10; 10]
            elif replica = 0 then [| [1; 99]; [10; 10]; [20; 20]; [30; 30] |].[modifier]
            else [| [40; 40]; [30; 30]; [20; 20]; [10; 10] |].[modifier]
        let members = lengths |> List.map (fun length ->
            let evaluation =
                sorterEvalV1.create (UMX.tag (Guid.NewGuid())) subset 32<sortingWidth>
                    0<sortableCount> (UMX.tag 1) (UMX.tag 1) (UMX.tag length) 100<ceLength>
                    (UMX.tag false) (UMX.tag 0) (UMX.tag 0) |> sorterEval.V1
            spMember_Standard.create (UMX.tag (Guid.NewGuid())) GeneSort.Model.Sorting.V1.sorterModel.Unknown
                (UMX.tag 0) (rp.GetMutationMod().Value) None
                (if scenario = "unevaluated" then None else Some evaluation) (UMX.tag generation)) |> List.toArray
        let pools = [| for index in 0 .. (if scenario = "multiple-pools" then 1 else 0) ->
                        sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 index)
                            members 150<ceLength> (rp.GetMutationMod().Value) |]
        let poolSet = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 pools.Length) (Some pools)
        if scenario <> "missing-checkpoint" then
            persist geneSortDataMode.Results
                (sourceHost.QueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get)
                (outputData.SorterPoolSet(if soss then SorterPoolSet_Soss.fromStandard poolSet |> SorterPoolSet_Soss.withSubsetIdForSelection subset |> sorterPoolSet.Soss else sorterPoolSet.Standard poolSet))
        let sourceTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth> [||] |> sortableTests.Bools
        let testsQuery = sourceHost.QueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "") |> Option.get
        persist geneSortDataMode.Setup ((SetupInputReferences.resolveRecorded rp testsQuery)) (outputData.SortableTestsWithPrefix(sourceTests, [||]))
    let selection = [
        ParamKeys.selectedSorterPoolCountKey, [if scenario = "all" then "4" else "2"]
        ParamKeys.mutationModKey, (if scenario = "missing-mod" then ["0"; "8"] else ["0"; "1"; "2"; "3"])
    ]
    let configured =
        continuationConfig source 1 2 selection [ParamKeys.mutationModKey, ["7"; "8"; "9"; "10"]]
        |> (if soss then EvoCondense.createSoss else EvoCondense.createStandard)
    let generate run =
        RunParameterGeneration.generateWithSourceSelection
            (GeneSort.Project.V1.Catalogs.Setup.SorterSgd.EvoCondenseSelection.select (fun _ -> db)) run 0<replNumber> 2<replNumber>
    configured, candidates, db, generate

[<Fact>]
let ``Soss condense selects and evaluates within each partition count before branching`` () =
    let configured, candidates, _, generate = condenseFixtureFor true "partition-rank"
    let branches = generate configured
    Assert.Equal(32, branches.Length) // 2 replicas x 2 partitions x 2 winners x 4 mutations
    Assert.Equal(32, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    for replica in [0; 1] do
        for partition, winningMods in [1, set [0; 1]; 4, set [2; 3]] do
            let group = branches |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica && int (rp.GetPartitionCount().Value) = partition)
            Assert.Equal(8, group.Length)
            Assert.Equal<Set<int>>(winningMods, group |> Array.map (fun rp -> int (rp.GetSourceMutationMod().Value)) |> Set.ofArray)
            for sourceId, variants in group |> Array.groupBy (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) do
                let source = candidates |> Array.find (fun rp -> string %(rp.GetId().Value) = sourceId)
                Assert.Equal(Some (UMX.tag partition), source.GetPartitionCount())
                Assert.Equal<Set<int>>(set [7; 8; 9; 10], variants |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Set.ofArray)

[<Fact>]
let ``Condense selects within every source parameter combination rather than just partition count`` () =
    let configured, _, _, generate = condenseFixtureFor true "partition-combinations"
    let branches = generate configured
    Assert.Equal(64, branches.Length)
    for replica in [0; 1] do
        for partition in [1; 4] do
            for rate in ["1.15"; "1.25"] do
                let group = branches |> Array.filter (fun rp ->
                    int (rp.GetRepl().Value) = replica && int (rp.GetPartitionCount().Value) = partition
                    && rp.ParamMap.[ParamKeys.paraRateKey] = rate)
                let expected = if rate = "1.25" then set [1; 2] elif partition = 1 then set [0; 1] else set [2; 3]
                Assert.Equal(8, group.Length)
                Assert.Equal<Set<int>>(expected, group |> Array.map (fun rp -> int (rp.GetSourceMutationMod().Value)) |> Set.ofArray)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Continuation refuses multiplexed execution overrides`` multipleSourceValues =
    let source = standardSource () |> withSetupSpans [ParamKeys.paraRateKey, (if multipleSourceValues then ["1.1"; "1.2"] else ["1.1"])]
    let values = if multipleSourceValues then ["1.3"] else ["1.3"; "1.4"]
    let error = Assert.Throws<ArgumentException>(fun () -> appendFrom source 1 2 [] [ParamKeys.paraRateKey, values] |> ignore)
    Assert.Contains(ParamKeys.paraRateKey, error.Message)
    Assert.Contains((if multipleSourceValues then "must be inherited" else "exactly one value"), error.Message)

[<Fact>]
let ``Continuation refuses overriding a multiplexed inherited source setting`` () =
    let source = standardSource () |> withSetupSpans [ParamKeys.paraRateKey, ["1.1"; "1.2"]]
    let first = appendFrom source 1 2 [] [ParamKeys.mutationModKey, ["7"; "8"]]
    Assert.Equal(4, (RunParameterGeneration.generate first 0<replNumber> 1<replNumber>).Length)
    let error = Assert.Throws<ArgumentException>(fun () -> appendFrom first 2 3 [] [ParamKeys.paraRateKey, ["1.3"]] |> ignore)
    Assert.Contains("must be inherited", error.Message)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Continuation source validates its generation interval at construction`` soss =
    let source = if soss then sossSource [0] else standardSource ()
    let error = Assert.Throws<ArgumentException>(fun () -> sourceSelection source -1 [] |> ignore)
    Assert.Equal("GenerationInterval", error.ParamName)
    Assert.Contains("nonnegative", error.Message)
    let selected = sourceSelection source 0 []
    Assert.Equal(0, selected.GenerationInterval)
    Assert.Equal<run>(source, selected.SourceRun)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Continuation source rejects a simple run with an evolution function name`` soss =
    let source = if soss then sossSource [0] else standardSource ()
    let simple = run.SimpleRun source.BaseRun
    let error = Assert.Throws<ArgumentException>(fun () -> sourceSelection simple 0 [] |> ignore)
    Assert.Equal("SourceRun", error.ParamName)
    Assert.Contains("Standard or Soss SGD source run", error.Message)

[<Fact>]
let ``Continuation source rejects an unsupported SGD execution family at construction`` () =
    let dto = GeneSort.Project.Mp.V1.RunDto.fromDomain (standardSource ())
    let source = GeneSort.Project.Mp.V1.RunDto.toDomain { dto with ExecuteFunctionName = ExecuteFunctionNames.sorterEval }
    let error = Assert.Throws<ArgumentException>(fun () -> sourceSelection source 0 [] |> ignore)
    Assert.Equal("SourceRun", error.ParamName)
    Assert.Contains("Standard or Soss SGD source run", error.Message)

[<Fact>]
let ``Continuation source exposes only required typed selection fields`` () =
    let source = standardSource ()
    for modifiers, count in [[], 1; [0; 0], 1; [0], 0; [0], -1] do
        Assert.Throws<ArgumentException>(fun () ->
            evoContinuationSource.create { SourceRun = source; GenerationInterval = 1; SourceMutationMods = modifiers; SelectedSorterPoolCount = count }
            |> ignore) |> ignore
    let configured = continuationConfig source 1 2 [] [] |> EvoCondense.createStandard
    let _, spans = configured.ParameterSource.Value
    Assert.Equal<Set<string>>(set [ParamKeys.sourceMutationModKey; ParamKeys.selectedSorterPoolCountKey], spans |> List.map fst |> Set.ofList)

[<Fact>]
let ``Condense rejects insufficient candidates in any source combination`` () =
    let configured, _, db, _ = condenseFixtureFor true "partition-rank"
    let source = configured.ParameterSource.Value |> fst
    let candidates = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
                     |> Array.filter (fun rp -> rp.GetPartitionCount() = Some 1<partitionCount> || rp.GetMutationMod() = Some (UMX.tag 0))
    let error = Assert.Throws<InvalidOperationException>(fun () -> EvoCondenseSelection.select (fun _ -> db) configured source candidates |> ignore)
    Assert.Contains("source combination", error.Message)
    Assert.Contains("at least", error.Message)

[<Fact>]
let ``Source mutation filtering cannot silently drop a source parameter combination`` () =
    let configured, _, db, generate = condenseFixtureFor true "partition-rank"
    let sourceIdentity = configured.ParameterSource.Value |> fst
    let source = db.loadAsync geneSortDataMode.Project (queryParams.createForRun sourceIdentity.DatabaseName sourceIdentity.ProjectName sourceIdentity.RunName)
                 |> Async.RunSynchronously |> Result.bind OutputData.asRun |> Result.defaultWith failwith
    let first = continuationConfig source 1 2
                    [ParamKeys.sourceMutationModKey, ["0"; "1"; "2"; "3"]; ParamKeys.selectedSorterPoolCountKey, ["2"]] []
                |> EvoCondense.createSoss
    let second = continuationConfig first 2 3
                    [ParamKeys.sourceMutationModKey, ["0"]; ParamKeys.selectedSorterPoolCountKey, ["1"]] []
                 |> EvoCondense.createSoss
    let error = Assert.Throws<InvalidOperationException>(fun () -> generate second |> ignore)
    Assert.Contains("0 matching executions", error.Message)

let private poolXSelection (configured: run) (candidates: runParameters[]) score =
    let source, spans = configured.ParameterSource.Value
    let count = spans |> List.find (fst >> (=) ParamKeys.selectedSorterPoolCountKey) |> snd |> List.exactlyOne |> int
    let modifiers = spans |> List.find (fst >> (=) ParamKeys.sourceMutationModKey) |> snd |> List.map int |> Set.ofList
    candidates
    |> Array.filter (fun rp -> Set.contains (int (rp.GetMutationMod().Value)) modifiers)
    |> Array.groupBy (RunParameterGeneration.sourceCombinationKey source)
    |> Array.collect (fun (_, group) -> group |> Array.sortBy (fun rp -> score rp, rp.GetId().Value) |> Array.take count)

let private assertPoolXExpansion (configured: run) (selected: runParameters[]) (branches: runParameters[]) =
    let modifiers = RunParameterGeneration.effectiveSpans configured |> Map.find ParamKeys.mutationModKey |> List.map int |> Set.ofList
    Assert.Equal(selected.Length * modifiers.Count, branches.Length)
    Assert.Equal(branches.Length, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let groups = branches |> Array.groupBy (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Map.ofArray
    let overriddenKeys = configured.RunParameterSpans |> List.map fst |> Set.ofList
    let inheritedCombination rp =
        RunParameterGeneration.sourceCombinationKey (fst configured.ParameterSource.Value) rp
        |> List.filter (fun (key, _) -> not (Set.contains key overriddenKeys))
    Assert.Equal<Set<string>>(selected |> Array.map (fun rp -> string %(rp.GetId().Value)) |> Set.ofArray, groups |> Map.keys |> Set.ofSeq)
    for source in selected do
        let variants = groups.[string %(source.GetId().Value)]
        Assert.Equal(modifiers.Count, variants.Length)
        Assert.Equal<Set<int>>(modifiers, variants |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Set.ofArray)
        Assert.All(variants, fun rp ->
            Assert.Equal(source.GetMutationMod(), rp.GetSourceMutationMod())
            Assert.Equal(source.GetSorterPoolCount(), rp.GetSorterPoolCount())
            Assert.Equal(source.GetSorterCountPerPool(), rp.GetSorterCountPerPool())
            Assert.Equal<(string * string option) list>(inheritedCombination source, inheritedCombination rp))

[<Fact>]
let ``Pool X Step2 ranks repeated mutation values from distinct Step1 winners within each combination`` () =
    let baseRun = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Soss_condense.Pool_X.Base ()
    let step1 = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Soss_condense.Pool_X.Step1 ()
    let step2 = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Soss_condense.Pool_X.Step2 ()
    let databases = System.Collections.Generic.Dictionary<string, IGeneSortDb>()
    let seed (configured: run) sourceInterval (parameters: runParameters[]) (score: runParameters -> int) =
        let host = runHost.Create configured 1
        let memory = MemoryDb(host.QueryParamsFromRunParams)
        memory.SavedParameters <- parameters
        let db = memory :> IGeneSortDb
        databases.[%configured.RunName] <- db
        let persist mode query value = db.saveAsync mode query value (UMX.tag true) |> Async.RunSynchronously |> Result.defaultWith failwith
        persist geneSortDataMode.Project (queryParams.createForRun configured.DatabaseName configured.ProjectName configured.RunName) (outputData.Run configured)
        let generation = GenIntervalConfig.getSampleAtIndex host.GenSaveIntervals.Value sourceInterval |> Option.get
        for rp in parameters do
            let subset = string (int (rp.GetPartitionCount().Value)) |> UMX.tag<sortableTestsSubsetId>
            let evaluation = sorterEvalV1.create (UMX.tag (Guid.NewGuid())) subset (rp.GetSortingWidth().Value)
                                 0<sortableCount> (UMX.tag 1) (UMX.tag 1) (UMX.tag (score rp)) 100<ceLength>
                                 (UMX.tag false) (UMX.tag 0) (UMX.tag 0) |> sorterEval.V1
            let members = Array.init (int (rp.GetSorterCountPerPool().Value)) (fun _ ->
                spMember_Standard.create (UMX.tag (Guid.NewGuid())) GeneSort.Model.Sorting.V1.sorterModel.Unknown
                    (UMX.tag 0) (rp.GetMutationMod().Value) None (Some evaluation) (UMX.tag generation))
            let pool = sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "source") (D1 0) members 150<ceLength> (rp.GetMutationMod().Value)
            let poolSet = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 1) (Some [pool])
                          |> SorterPoolSet_Soss.fromStandard |> SorterPoolSet_Soss.withSubsetIdForSelection subset
            let query = host.QueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get
            persist geneSortDataMode.Results query (outputData.SorterPoolSet (sorterPoolSet.Soss poolSet))
    let generate configured =
        RunParameterGeneration.generateWithSourceSelection
            (EvoCondenseSelection.select (fun source -> databases.[%source.RunName])) configured 0<replNumber> 1<replNumber>
    let baseParameters = RunParameterGeneration.generate baseRun 0<replNumber> 1<replNumber>
    let sourceInterval (configured: run) =
        configured.RunParameterSpans |> List.find (fst >> (=) ParamKeys.sourceGenerationIntervalKey) |> snd |> List.exactlyOne |> int
    // Unique positive scores keep the ranking deterministic regardless of the configured mutation range.
    let scores (parameters: runParameters[]) =
        let ranked = parameters |> Array.sortBy (fun rp -> rp.GetMutationMod(), rp.GetId())
                     |> Array.mapi (fun index rp -> rp.GetId().Value, index + 1) |> Map.ofArray
        fun (rp: runParameters) -> ranked.[rp.GetId().Value]
    let baseScore = scores baseParameters
    seed baseRun (sourceInterval step1) baseParameters baseScore
    let first = generate step1
    assertPoolXExpansion step1 (poolXSelection step1 baseParameters baseScore) first
    // Rank by mutation first so children from distinct winners compete with the same mutation value.
    let firstScore = scores first
    seed step1 (sourceInterval step2) first firstScore
    let second = generate step2
    assertPoolXExpansion step2 (poolXSelection step2 first firstScore) second

[<Fact>]
let ``Pool X Step1 expands configured winners within each source combination`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Soss_condense.Pool_X.Step1 ()
    let source = fst configured.ParameterSource.Value
    let candidates = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
    let score (rp: runParameters) = int (rp.GetMutationMod().Value)
    let select run _ candidates = poolXSelection run candidates score
    let branches = RunParameterGeneration.generateWithSourceSelection select configured 0<replNumber> 1<replNumber>
    assertPoolXExpansion configured (poolXSelection configured candidates score) branches

let private condenseFixture scenario = condenseFixtureFor false scenario

[<Fact>]
let ``OnePool Soss condense preset selects single-pool Soss sources with explicit catalogs`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.Continuation.TestSossCondense ()
    Assert.Equal(ExecuteFunctionNames.soss, configured.ExecuteFunctionName)
    Assert.Equal(QueryCatalogNamesSetup.importedStart, configured.SetupQueryCatalogName)
    Assert.Equal(QueryCatalogNamesRun.sorterSgdMsufSossAppend, configured.RunQueryCatalogName)
    let source, spans = configured.ParameterSource.Value
    Assert.Contains((ParamKeys.selectedSorterPoolCountKey, ["2"]), spans)
    let candidates = RunParameterGeneration.generate source 0<replNumber> 1<replNumber>
    Assert.Equal(4, candidates.Length)
    Assert.Equal<Set<int>>(set [0; 1; 2; 3], candidates |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Set.ofArray)
    Assert.All(candidates, fun rp ->
        Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
        Assert.Equal(Some 32<sorterCountPerPool>, rp.GetSorterCountPerPool()))

[<Fact>]
let ``Soss condense ties are stable and can select every candidate`` () =
    let configured, candidates, _, generate = condenseFixtureFor true "ties"
    let branches = generate configured
    for replica in [0; 1] do
        let expected = candidates |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
                                  |> Array.sortBy (fun rp -> rp.GetId().Value) |> Array.take 2
                                  |> Array.map (fun rp -> string %(rp.GetId().Value)) |> Set.ofArray
        let actual = branches |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
                              |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray
        Assert.Equal<Set<string>>(expected, actual)
    let all, _, _, generateAll = condenseFixtureFor true "all"
    Assert.Equal(32, (generateAll all).Length)

[<Fact>]
let ``Soss condense ranks per replica and copies selected Soss checkpoints into Setup`` () =
    let configured, candidates, sourceDb, generate = condenseFixtureFor true "rank"
    let restored = configured |> GeneSort.Project.Mp.V1.RunDto.fromDomain |> GeneSort.Project.Mp.V1.RunDto.toDomain
    let branches = generate restored
    Assert.Equal(16, branches.Length)
    let host = (runHost.Create restored 1).WithRunDb(MemoryDb())
    use cts = new CancellationTokenSource()
    for replica in [0; 1] do
        let expectedMods = if replica = 0 then [1; 2] else [2; 3]
        let expectedIds = candidates |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica && List.contains (int (rp.GetMutationMod().Value)) expectedMods)
                                    |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.idKey]) |> Set.ofArray
        let selected = branches |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
        Assert.Equal<Set<string>>(expectedIds, selected |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray)
        for rp in selected do
            let prepared = importStart sourceDb host.Run host.Storage rp (UMX.tag false) cts None
                           |> Async.RunSynchronously |> Result.defaultWith failwith
            let query = host.QueryParamsFromRunParams prepared (geneSortDataType.SorterPoolSet "") |> Option.get |> fun query -> SetupInputReferences.resolveRecorded prepared query
            let pool = host.RunDb.loadAsync geneSortDataMode.Setup query |> Async.RunSynchronously
                       |> Result.bind OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
            let branched = EvoAppendPools.prepareSoss prepared pool |> Result.defaultWith failwith
            Assert.Single(branched.SorterPools) |> ignore
            Assert.All(branched.SorterPools.Values, fun pool ->
                Assert.Equal(prepared.GetMutationMod().Value, pool.MutationMod)
                Assert.All(pool.SorterPoolMembers, fun m -> Assert.Empty(m.SorterEvalMap)))

[<Theory>]
[<InlineData("multiple-pools", "exactly one")>]
[<InlineData("unevaluated", "fully evaluated")>]
[<InlineData("missing-checkpoint", "Missing prepared input")>]
[<InlineData("missing-mod", "subset")>]
[<InlineData("different-subsets", "selection subsets must match")>]
let ``Soss condense rejects unsuitable sources`` scenario message =
    let configured, _, _, generate = condenseFixtureFor true scenario
    let error = Assert.Throws<InvalidOperationException>(fun () -> generate configured |> ignore)
    Assert.Contains(message, error.Message)

[<Fact>]
let ``Condense ranks averages per replica and branches only winners through normal append setup`` () =
    let configured, candidates, sourceDb, generate = condenseFixture "rank"
    let roundTripped = configured |> GeneSort.Project.Mp.V1.RunDto.fromDomain |> GeneSort.Project.Mp.V1.RunDto.toDomain
    let branches = generate roundTripped
    Assert.Equal(16, branches.Length)
    Assert.Equal(16, branches |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let host = (runHost.Create configured 1).WithRunDb(MemoryDb())
    use cts = new CancellationTokenSource()
    for replica in [0; 1] do
        let selected = branches |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
        let expectedMods = if replica = 0 then [1; 2] else [2; 3]
        let expectedIds = candidates |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica && List.contains (int (rp.GetMutationMod().Value)) expectedMods)
                                    |> Array.map (fun rp -> string %(rp.GetId().Value)) |> Set.ofArray
        Assert.Equal<Set<string>>(expectedIds, selected |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray)
        for rp in selected do
            let prepared = importStart sourceDb host.Run host.Storage rp (UMX.tag false) cts None
                           |> Async.RunSynchronously |> Result.defaultWith failwith
            Assert.True(prepared.GetSelectedSorterPoolCount().IsNone)
            // Reuse the ordinary imported-file query and execution branching.
            let poolQuery = host.QueryParamsFromRunParams prepared (geneSortDataType.SorterPoolSet "") |> Option.get |> fun query -> SetupInputReferences.resolveRecorded prepared query
            let pool = host.RunDb.loadAsync geneSortDataMode.Setup poolQuery |> Async.RunSynchronously
                       |> Result.bind OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith
            let branched = GeneSort.Project.V1.Catalogs.Evolution.EvoAppendPools.prepareStandard prepared pool |> Result.defaultWith failwith
            Assert.Equal(1, branched.SorterPools.Count)
            Assert.Equal(prepared.GetMutationMod().Value, (branched.SorterPools |> Map.values |> Seq.exactlyOne).MutationMod)

[<Theory>]
[<InlineData("multiple-pools", "exactly one")>]
[<InlineData("unevaluated", "fully evaluated")>]
[<InlineData("missing-checkpoint", "Missing prepared input")>]
[<InlineData("missing-mod", "subset")>]
let ``Condense rejects unsuitable source checkpoints before creating branches`` scenario message =
    let configured, _, _, generate = condenseFixture scenario
    let error = Assert.Throws<InvalidOperationException>(fun () -> generate configured |> ignore)
    Assert.Contains(message, error.Message)

[<Fact>]
let ``Condense permits selecting every requested source mutation variant`` () =
    let configured, _, _, generate = condenseFixture "all"
    Assert.Equal(32, (generate configured).Length)

[<Fact>]
let ``Condense validates source selection count and mutation subset size`` () =
    let source = standardSource ()
    for counts, modifiers in [(["1"; "2"], ["0"; "1"]); (["0"], ["0"]); (["2"], ["0"; "0"])] do
        Assert.Throws<ArgumentException>(fun () ->
            let selection = [
                ParamKeys.selectedSorterPoolCountKey, counts
                ParamKeys.mutationModKey, modifiers
            ]
            condenseFrom source 1 2 selection [] |> ignore) |> ignore

[<Fact>]
let ``Condense source mutation span filters candidates without branching winners`` () =
    let _, candidates, _, generate = condenseFixture "rank"
    let source = standardSource () |> withSetupSpans [
        ParamKeys.mutationModKey, ["0"; "1"; "2"; "3"]
        ParamKeys.sorterCountPerPoolSetKey, ["32"]
    ]
    let selection = [
        ParamKeys.selectedSorterPoolCountKey, ["2"]
        ParamKeys.mutationModKey, ["0"; "1"; "3"]
    ]
    let condensed = condenseFrom source 1 2 selection []
    let selected = generate condensed
    Assert.Equal(4, selected.Length)
    for replica in [0; 1] do
        let expectedMods = [1; 3]
        let expected = candidates |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica && List.contains (int (rp.GetMutationMod().Value)) expectedMods)
                                  |> Array.map (fun rp -> string %(rp.GetId().Value)) |> Set.ofArray
        let actual = selected |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
                              |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray
        Assert.Equal<Set<string>>(expected, actual)

[<Fact>]
let ``Condense ties use stable source identities and pure expansion requires checkpoint access`` () =
    let configured, candidates, _, generate = condenseFixture "ties"
    let first = generate configured
    let second = generate configured
    Assert.Equal<Guid<queryParamsId> option array>(first |> Array.map (fun rp -> rp.GetId()), second |> Array.map (fun rp -> rp.GetId()))
    for replica in [0; 1] do
        let expected = candidates |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
                                  |> Array.sortBy (fun rp -> rp.GetId().Value) |> Array.take 2
                                  |> Array.map (fun rp -> string %(rp.GetId().Value)) |> Set.ofArray
        let actual = first |> Array.filter (fun rp -> int (rp.GetRepl().Value) = replica)
                           |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray
        Assert.Equal<Set<string>>(expected, actual)
    Assert.Throws<InvalidOperationException>(fun () -> RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> ignore) |> ignore

[<Fact>]
let ``Appending a condensed run retains selected source identities without repeating selection settings`` () =
    let configured, _, _, generate = condenseFixture "rank"
    let selectedIds = generate configured |> Array.map (fun rp -> string %(rp.GetId().Value)) |> Set.ofArray
    let appended = appendFrom configured 2 3 [] [ParamKeys.mutationModKey, ["11"; "12"]]
    let roundTripped = appended |> GeneSort.Project.Mp.V1.RunDto.fromDomain |> GeneSort.Project.Mp.V1.RunDto.toDomain
    let branches = generate roundTripped
    Assert.Equal(32, branches.Length)
    Assert.Equal<Set<string>>(selectedIds, branches |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray)
    Assert.All(branches, fun rp -> Assert.True(rp.GetSelectedSorterPoolCount().IsNone))

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``OnePool chains preserve their family and expand the immediate source`` soss =
    // Deterministic stand-in for ranking: test expansion without requiring saved library data.
    let select (configured: run) _ (candidates: runParameters[]) =
        match configured.ParameterSource |> Option.bind (snd >> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey)) with
        | Some (_, [count]) -> candidates |> Array.sortBy (fun rp -> rp.GetId()) |> Array.take (int count)
        | _ -> candidates
    let generate configured = RunParameterGeneration.generateWithSourceSelection select configured 0<replNumber> 1<replNumber>
    let cases =
        if soss then [
            GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SameFamilyChain.TestSossAppendCondense(), "TestSossAppend", [|32; 32|]
            GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SameFamilyChain.TestSossCondenseAppend(), "TestSossCondense", Array.create 8 32
        ] else [
            GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SameFamilyChain.TestStandardAppendCondense(), "TestStandardAppend", [|32; 32|]
            GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SameFamilyChain.TestStandardCondenseAppend(), "TestStandardCondense", [|32; 32|]
        ]
    for configured, sourceName, sizes in cases do
        let source, _ = configured.ParameterSource.Value
        Assert.Equal(sourceName, %source.RunName)
        Assert.Equal(source.ExecuteFunctionName, configured.ExecuteFunctionName)
        let expectedSources = generate source |> Array.map (fun rp -> string (rp.GetId().Value)) |> Set.ofArray
        let expanded = generate configured
        Assert.Equal<int>(sizes, expanded |> Array.map (fun rp -> int (rp.GetSorterCountPerPool().Value)) |> Array.sort)
        Assert.Equal(expanded.Length, expanded |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
        Assert.All(expanded, fun rp ->
            Assert.Equal(Some 1, rp.GetSourceGenerationInterval())
            Assert.Equal(Some (UMX.tag 2), rp.GetGenerationIntervalLast())
            Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
            Assert.Contains(rp.ParamMap.[ParamKeys.sourceExecutionIdKey], expectedSources))

let private mixedSossSpans = [
    ParamKeys.seedSossKey, ["1234"]
    ParamKeys.partitionCountKey, ["2"]
    ParamKeys.generationsPerSubsetKey, ["10"]
    ParamKeys.generationsPerMergedSetKey, ["20"]
]

[<Fact>]
let ``Standard to Soss requires each explicit transition setting`` () =
    for key, _ in mixedSossSpans do
        for invalid in [None; Some ["invalid"]; Some []] do
            let spans = mixedSossSpans |> List.filter (fst >> (<>) key)
            let spans = match invalid with None -> spans | Some values -> (key, values) :: spans
            let config = continuationConfig (standardSource ()) 1 2 [] spans
            let error = Assert.Throws<ArgumentException>(fun () -> EvoAppend.createSoss config |> ignore)
            Assert.Contains(key, error.Message)
    for key in [ParamKeys.partitionCountKey; ParamKeys.generationsPerSubsetKey; ParamKeys.generationsPerMergedSetKey] do
        let spans = mixedSossSpans |> List.map (fun (k, v) -> k, if k = key then ["0"] else v)
        Assert.Throws<ArgumentException>(fun () ->
            continuationConfig (standardSource ()) 1 2 [] spans |> EvoAppend.createSoss |> ignore) |> ignore

[<Fact>]
let ``MixedFamilyChain presets branch and select within the source family`` () =
    let select (configured: run) _ (candidates: runParameters[]) =
        match configured.ParameterSource |> Option.bind (snd >> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey)) with
        | Some (_, [count]) -> candidates |> Array.sortBy (fun rp -> rp.GetId()) |> Array.take (int count)
        | _ -> candidates
    let cases = [
        GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.MixedFamilyChain.TestStandardAppendSossCondense(), true, [|32; 32|]
        GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.MixedFamilyChain.TestStandardCondenseSossAppend(), true, [|32; 32|]
        GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.MixedFamilyChain.TestSossAppendStandardCondense(), false, [|32; 32|]
        GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.MixedFamilyChain.TestSossCondenseStandardAppend(), false, Array.create 8 32
    ]
    for configured, soss, sizes in cases do
        Assert.Equal("MixedFamilyChain", %configured.DatabaseName)
        Assert.Equal("SorterSgd.RunCfg.p32.Test.OnePool", %configured.ProjectName)
        Assert.NotEqual<string>(configured.ExecuteFunctionName, (fst configured.ParameterSource.Value).ExecuteFunctionName)
        Assert.Equal((if soss then ExecuteFunctionNames.soss else ExecuteFunctionNames.standard), configured.ExecuteFunctionName)
        let generated = RunParameterGeneration.generateWithSourceSelection select configured 0<replNumber> 1<replNumber>
        Assert.Equal<int>(sizes, generated |> Array.map (fun rp -> int (rp.GetSorterCountPerPool().Value)) |> Array.sort)
        for rp in generated do
            Assert.Equal(Some 1, rp.GetSourceGenerationInterval())
            if soss then
                for key, values in mixedSossSpans do Assert.Equal(List.exactlyOne values, rp.ParamMap.[key])
            else Assert.Equal(Some 10<adjustCeLengthFrequency>, rp.GetAdjustCeLengthFrequency())

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``Mixed family Setup converts members and execution needs only destination data`` toSoss =
    let sourceRun = (if toSoss then standardSource () else singleSource ()) |> withSetupSpans [
        ParamKeys.mutationModKey, ["0"]
        ParamKeys.sorterCountPerPoolKey, ["2"]
        ParamKeys.sorterCountPerPoolSetKey, ["2"]
    ]
    let sourceHost = runHost.Create sourceRun 1
    let sourceRp = sourceHost.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    let generation = GenIntervalConfig.getSampleAtIndex sourceHost.GenSaveIntervals.Value 1 |> Option.get
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> (Array.create 150 1)
                |> simpleSorterModel.Msce |> GeneSort.Model.Sorting.V1.sorterModel.Simple
    let originalMembers = [| for _ in 1..2 -> spMember_Standard.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 3) (UMX.tag 0) None None 0<generationNumber> |]
    let originalPool = sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "mixed") (D1 0) originalMembers 150<ceLength> (UMX.tag 0)
    let original = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) (UMX.tag generation) (Dim1 1) (Some [originalPool])
    let fullTests = sortableBinaryTests.create (UMX.tag Guid.Empty) 32<sortingWidth>
                        [|sortableBoolArray.create(Array.create 32 false, 32<sortingWidth>); sortableBoolArray.create(Array.create 32 true, 32<sortingWidth>)|] |> sortableTests.Bools
    let prefix = ceBlock.create (UMX.tag Guid.Empty) 32<sortingWidth> [||]
    let evaluations = PoolRunner_Standard.evaluatePoolSet fullTests prefix sorterEvalType.V2 true (UMX.tag false) original
    let evaluated = original |> SorterPoolSet_Standard.updateSorterEvals evaluations
    let sourcePool = if toSoss then sorterPoolSet.Standard evaluated else SorterPoolSet_Soss.fromStandard evaluated |> sorterPoolSet.Soss
    let sourceMemory = MemoryDb(sourceHost.QueryParamsFromRunParams)
    sourceMemory.SavedParameters <- [|sourceRp|]
    let sourceDb = sourceMemory :> IGeneSortDb
    let persist mode query value = sourceDb.saveAsync mode query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith
    persist geneSortDataMode.Project (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName) (outputData.Run sourceRun)
    let sourceQuery = sourceHost.QueryParamsFromRunParams (sourceRp.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "") |> Option.get
    persist geneSortDataMode.Results sourceQuery (outputData.SorterPoolSet sourcePool)
    persist geneSortDataMode.Setup
        (sourceSetupQuery sourceHost (sourceRp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix ""))
        (outputData.SortableTestsWithPrefix(fullTests, [||]))
    let destination =
        let config = evoContinuationConfig.create {
            ProjectName = UMX.tag "Mixed.Test"; DatabaseName = UMX.tag "Mixed"; RunName = UMX.tag "Transition"
            RunQueryCatalogName = if toSoss then QueryCatalogNamesRun.sorterSgdMsufSossAppend else QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            Source = sourceSelection sourceRun 1 []
            ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["2"]; ParamKeys.adjustCeLengthFrequencyKey, ["10"]] @ (if toSoss then mixedSossSpans else [])
        }
        if toSoss then EvoAppend.createSoss config else EvoAppend.createStandard config
    let initialHost = runHost.Create destination 1
    let memory = MemoryDb(initialHost.QueryParamsFromRunParams)
    let host = initialHost.WithRunDb memory
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.exactlyOne
    use cts = new CancellationTokenSource()
    let mutable sourceOpens = 0
    let sources = workflowSources.create {
        OpenDatabase = fun project database catalog ->
            Assert.Equal<string>(%sourceRun.ProjectName, %project)
            Assert.Equal<string>(%sourceRun.DatabaseName, %database)
            Assert.Equal(sourceRun.RunQueryCatalogName, catalog)
            sourceOpens <- sourceOpens + 1
            sourceDb
        Evolution = {
            LoadMergeTests = fun _ _ _ -> failwith "Continuation must copy its saved tests."
            LoadPrefixTests = fun _ _ _ -> failwith "Continuation must copy its saved tests."
            LoadStandardSorterEvals = fun _ _ _ -> failwith "Continuation must copy its saved pool."
            LoadMergeSorterEvals = fun _ _ _ -> failwith "Continuation must copy its saved pool."
            LoadPrefixSorterEvals = fun _ _ _ -> failwith "Continuation must copy its saved pool."
        }
    }
    let setup = SetupExecutorCatalog.get sources host.Run host.Storage
    Assert.Equal(0, sourceOpens)
    let updated = setup rp (UMX.tag false) cts None |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(1, sourceOpens)
    let imported = Utils.loadSetupOutputData host.RunDb updated (geneSortDataType.SorterPoolSet "") |> Async.RunSynchronously |> Result.defaultWith failwith
    let ids =
        if toSoss then
            let converted = imported |> OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith
            Assert.Equal(original.SorterPoolSetId, converted.SorterPoolSetId)
            let pool = converted.SorterPools.[originalPool.SorterPoolId]
            Assert.All(pool.SorterPoolMembers, fun m -> Assert.Empty(m.SorterEvalMap))
            pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterPoolMemberId) |> Set.ofSeq
        else
            let converted = imported |> OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith
            Assert.Equal(original.SorterPoolSetId, converted.SorterPoolSetId)
            let pool = converted.SorterPools.[originalPool.SorterPoolId]
            Assert.All(pool.SorterPoolMembers, fun m -> Assert.True(m.SorterEval.IsSome))
            pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterPoolMemberId) |> Set.ofSeq
    Assert.True((ids = (originalMembers |> Array.map (fun m -> m.SorterPoolMemberId) |> Set.ofArray)))
    Assert.False(updated.ParamMap.ContainsKey "SourcePoolFileId")
    let unconvertedQuery = queryParams.createForImportedFile host.Run.DatabaseName host.Run.ProjectName (updated.GetRepl()) (geneSortDataType.SorterPoolSet "") sourceQuery.Id
    Assert.NotEqual<string>(string %unconvertedQuery.Id, updated.ParamMap.[ParamKeys.setupSorterPoolSetIdKey])
    sourceMemory.Reads.Clear()
    let shortIntervals = { Name = "mixed-test"; Min = generation; MaxCount = Some 3; Scale = 1.0; Method = Constant 1 }
    let execute = if toSoss then SgdEx_Soss.evaluateEvolutionRunSoss else SgdEx_Standard.evaluateEvolutionRunStandard
    let makeMutator _ = async {
        return msceRandMutate.create rngFactory.LcgFactory (indelRates.create (0.1, 0.0, 0.0)) (UMX.tag true)
                   GeneSort.Model.Sorting.V1.mutatorVariant.V2
               |> GeneSort.Model.Sorting.Simple.V1.simpleSorterModelMutator.SmmMsceRandMutate |> Ok
    }
    let finished = execute makeMutator host.RunDb shortIntervals shortIntervals updated (UMX.tag false) cts None
                   |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(Some true, finished.GetRunFinished())
    Assert.Equal(Some (UMX.tag (GenIntervalConfig.getSampleAtIndex shortIntervals 2 |> Option.get)), finished.GetGenerationCurrent())
    Assert.Empty(sourceMemory.Reads)

[<Fact>]
let ``OnePool Soss condense branches each selected source into four destination mutations`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.Continuation.TestSossCondense ()
    let select _ _ (candidates: runParameters[]) =
        candidates |> Array.filter (fun rp -> List.contains (int (rp.GetMutationMod().Value)) [1; 3])
    let runs = RunParameterGeneration.generateWithSourceSelection select configured 0<replNumber> 1<replNumber>
    Assert.Equal(8, runs.Length)
    Assert.Equal(8, runs |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)
    let groups = runs |> Array.groupBy (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey])
    Assert.Equal(2, groups.Length)
    for _, branches in groups do
        Assert.Equal<Set<int>>(set [0; 1; 2; 3], branches |> Array.map (fun rp -> int (rp.GetMutationMod().Value)) |> Set.ofArray)
        Assert.Single(branches |> Array.map (fun rp -> rp.GetSourceMutationMod()) |> Array.distinct) |> ignore
        for rp in branches do
            Assert.Contains(int (rp.GetSourceMutationMod().Value), [1; 3])
            Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
            Assert.Equal(Some 32<sorterCountPerPool>, rp.GetSorterCountPerPool())

[<Fact>]
let ``Source mutation selection uses the immediate source rather than its ancestry`` () =
    let source = appendFrom (standardSource ()) 1 2 [] [ParamKeys.mutationModKey, ["7"; "8"]]
    let config = continuationConfig source 2 3
                    [ParamKeys.sourceMutationModKey, ["7"]]
                    [ParamKeys.mutationModKey, ["9"; "10"]]
    let run = EvoAppend.createStandard config
    let branches = RunParameterGeneration.generate run 0<replNumber> 1<replNumber>
    Assert.NotEmpty(branches)
    Assert.All(branches, fun rp ->
        Assert.Equal(Some (UMX.tag 7), rp.GetSourceMutationMod())
        Assert.Equal(Some source.RunName, rp.GetSourceRunName())
        Assert.Contains(int (rp.GetMutationMod().Value), [9; 10]))

[<Fact>]
let ``OnePool SimpleR presets inherit bounds and rate while preserving pool capacity`` () =
    let select (configured: run) _ (candidates: runParameters[]) =
        let count = configured.ParameterSource.Value |> snd |> List.find (fst >> (=) ParamKeys.selectedSorterPoolCountKey) |> snd |> List.exactlyOne |> int
        candidates |> Array.sortBy (fun rp -> rp.GetId()) |> Array.take count
    for name, configured in [
        "Standard", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Standard()
        "Soss", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Soss()
        "StandardAppend", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.StandardAppend()
        "SossCondense", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.SossCondense()
    ] do
        Assert.Equal(name, %configured.RunName)
        Assert.Equal("SimpleR", %configured.DatabaseName)
        let parameters = RunParameterGeneration.generateWithSourceSelection select configured 0<replNumber> 1<replNumber>
        Assert.NotEmpty(parameters)
        Assert.All(parameters, fun rp ->
            Assert.Equal(Some GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.aMax, rp.GetAMax())
            Assert.Equal(Some GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.activatorMutationRate, rp.GetActivatorMutationRate())
            Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
            Assert.Equal(Some 32<sorterCountPerPool>, rp.GetSorterCountPerPool()))

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleR parameter values distinguish setup files execution files and report metadata`` soss =
    let configured = if soss then GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Soss() else GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Standard()
    let host = runHost.Create configured 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber> |> Array.head
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let setupQuery parameters = host.QueryParamsFromSetupParams parameters (geneSortDataType.SorterPoolSet "") |> Option.get
    Assert.NotEqual((setupQuery setup).Id, (setupQuery (setup.WithAMax(Some 4))).Id)
    Assert.NotEqual((setupQuery setup).Id, (setupQuery (setup.WithAMax None)).Id)
    let runQuery parameters = host.QueryParamsFromRunParams parameters (geneSortDataType.SorterPoolSet "") |> Option.get
    Assert.NotEqual((runQuery rp).Id, (runQuery (rp.WithAMax(Some 4))).Id)
    Assert.NotEqual((runQuery rp).Id, (runQuery (rp.WithActivatorMutationRate(Some 0.5))).Id)
    let report = host.QueryParamsFromRunParams rp (geneSortDataType.TextReport (UMX.tag "summary")) |> Option.get
    Assert.Equal(string (rp.GetAMax().Value), report.Properties.[ParamKeys.aMaxKey])
    Assert.Equal(rp.ParamMap.[ParamKeys.activatorMutationRateKey], report.Properties.[ParamKeys.activatorMutationRateKey])

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleR seed generation setup and evolution execute with bounded activators`` soss =
    let configured = if soss then GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Soss() else GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Standard()
    let host = runHost.Create configured 1
    // Use small sorting width and complete binary tests to exercise real makers and execution without external data.
    let rp =
        (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head)
            .WithSortingWidth(Some 4<sortingWidth>)
            .WithPrefixLibId(Some (prefixLibId.create 4<sortingWidth> 1<stageLength> prefixLibVariant.PrefixA))
            .WithSimpleSorterModelType(Some GeneSort.Model.Sorting.V1.simpleSorterModelType.Msce)
            .WithMutationRate(Some (UMX.tag 1.0)).WithInsertionRate(Some (UMX.tag 0.0)).WithDeletionRate(Some (UMX.tag 0.0))
            .WithActivatorMutationRate(Some 1.0)
            .WithSorterEvalMeasure(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithSorterEvalMeasureInitial(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithSeedSorterPoolSelectionType(Some (sorterSelectionType.GuidOrder 32<sorterCount>))
            .WithDistinctSorterHashes(Some false).WithSortedFraction(Some 1.0<sortedFraction>)
            .WithGenerationIntervalLast(Some (UMX.tag 1))
            .WithGenerationsPerSubset(Some (UMX.tag 1)).WithGenerationsPerMergedSet(Some (UMX.tag 1))
    let rp = rp.ParamMap |> Map.filter (fun key _ -> not (List.contains key SetupFileReferences.keys)) |> runParameters.create
    let rp = SetupInputReferences.recordFiles (SetupOutputs.generatedFiles configured rp) rp
    let memory = MemoryDb(host.QueryParamsFromRunParams)
    let db = memory :> IGeneSortDb
    let tinyTests =
        Array.init 16 (fun bits -> sortableBoolArray.create(Array.init 4 (fun bit -> bits &&& (1 <<< bit) <> 0), 4<sortingWidth>))
        |> sortableBinaryTests.create (UMX.tag Guid.Empty) 4<sortingWidth> |> sortableTests.Bools
    let evaluations =
        Array.init 32 (fun index ->
            sorterEvalV2.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 4<sortingWidth> 16<sortableCount>
                (UMX.tag index) 2<stageLength> [||] (UMX.tag false) (UMX.tag 1) (UMX.tag 0) |> sorterEval.V2)
        |> sorterSetEval.create (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid()))
    let mutable loads = 0
    let sources: evolutionSetupSources = {
        LoadMergeTests = fun _ _ _ -> async { return Error "Unexpected loader" }
        LoadPrefixTests = fun _ _ _ -> async { return Ok tinyTests }
        LoadStandardSorterEvals = fun _ _ _ -> async { return Error "Unexpected loader" }
        LoadMergeSorterEvals = fun _ _ _ -> async { return Error "Unexpected loader" }
        LoadPrefixSorterEvals = fun _ _ _ -> async { loads <- loads + 1; return Ok evaluations }
    }
    let makePool = SeedSorterPoolSetMakerCatalog.get configured.MakerNames.Value.SeedPoolSet sources
    let shortIntervals = { Name = "simple-r-test"; Min = 0; MaxCount = Some 3; Scale = 1.0; Method = Constant 1 }
    use cts = new CancellationTokenSource()
    let setup = if soss then EvoStartCreation.createSossStart configured.ProjectName configured.DatabaseName else EvoStartCreation.createStandardStart configured.ProjectName configured.DatabaseName
    setup (fun _ -> async { return Ok (tinyTests, [||]) }) makePool db rp cts.Token
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Equal(1, loads)
    let execute = if soss then SgdEx_Soss.evaluateEvolutionRunSoss else SgdEx_Standard.evaluateEvolutionRunStandard
    let finished = execute (hostMutator host) db shortIntervals shortIntervals rp (UMX.tag false) cts None
                   |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(Some true, finished.GetRunFinished())
    Assert.Equal(1, loads) // Execution consumes the prepared seeds and does not regenerate them.
    let query = db.makeQueryParamsFromRunParams finished (geneSortDataType.SorterPoolSet "") |> Option.get
    let saved = db.loadAsync geneSortDataMode.Results query |> Async.RunSynchronously |> Result.defaultWith failwith
    let models, mutated =
        match saved with
        | outputData.SorterPoolSet (sorterPoolSet.Standard value) ->
            let pool = value.SorterPools |> Map.values |> Seq.exactlyOne
            pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel) |> Seq.toArray,
            pool.SorterPoolMembers |> Seq.exists (fun m -> m.SorterMutationSource.IsSome)
        | outputData.SorterPoolSet (sorterPoolSet.Soss value) ->
            let pool = value.SorterPools |> Map.values |> Seq.exactlyOne
            pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel) |> Seq.toArray,
            pool.SorterPoolMembers |> Seq.exists (fun m -> m.SorterMutationSource.IsSome)
        | _ -> failwith "Expected evolved pool"
    Assert.Equal(32, models.Length)
    Assert.True(mutated)
    Assert.All(models, fun model ->
        let paired = GeneSort.Model.Sorting.V1.SorterModel.asSimpleSorterModelR model
        Assert.Equal(rp.GetAMax().Value, paired.AMax)
        Assert.All(paired.Activators, fun value -> Assert.InRange(value, -paired.AMax, paired.AMax)))

[<Fact>]
let ``Dispatch SimpleR settings reject missing invalid and incompatible activator parameters`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleR.Standard()
    let host = runHost.Create configured 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let underlying = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                     |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    for invalid in [rp.WithAMax None; rp.WithAMax(Some -1); rp.WithActivatorMutationRate None;
                    rp.WithActivatorMutationRate(Some Double.NaN); rp.WithActivatorMutationRate(Some 1.1); rp.WithActivatorMutationRate(Some -0.1)] do
        Assert.True(MutatorMakers.applyParameters invalid underlying |> Result.isError)
    let generator = SeedSorterModelGeneration.getSimpleUniformSorterModelGen Lcg 4<sortingWidth>
                        GeneSort.Model.Sorting.V1.simpleSorterModelType.Msce (UMX.tag true)
    Assert.True(SeedSorterModelGeneration.applyParameters (rp.WithAMax(Some -1)) generator |> Result.isError)
    let paired = SeedSorterModelGeneration.applyParameters rp generator |> Result.defaultWith failwith
                 |> GeneSort.Model.Sorting.V1.SorterModelGen.makeSorterModelFromIndex (UMX.tag 0)
    Assert.True(MutatorMakers.validateModels (rp.WithAMax(Some 9)) [paired] |> Result.isError)
    Assert.True(MutatorMakers.validateModels rp [paired] |> Result.isOk)

[<Fact>]
let ``OnePool SimpleS presets inherit probability and rate while preserving pool capacity`` () =
    let select (configured: run) _ (candidates: runParameters[]) =
        let count = configured.ParameterSource.Value |> snd |> List.find (fst >> (=) ParamKeys.selectedSorterPoolCountKey) |> snd |> List.exactlyOne |> int
        candidates |> Array.sortBy (fun rp -> rp.GetId()) |> Array.take count
    for name, configured in [
        "StandardS", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.StandardS()
        "SossS", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.SossS()
        "StandardAppendS", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.StandardAppendS()
        "SossCondenseS", GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.SossCondenseS()
    ] do
        Assert.Equal(name, %configured.RunName)
        Assert.Equal("SimpleS", %configured.DatabaseName)
        let parameters = RunParameterGeneration.generateWithSourceSelection select configured 0<replNumber> 1<replNumber>
        Assert.NotEmpty(parameters)
        Assert.All(parameters, fun rp ->
            Assert.Equal(Some GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.activationProbability, rp.GetActivationProbability())
            Assert.Equal(Some GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.activatorMutationRate, rp.GetActivatorMutationRate())
            Assert.Equal(Some 1<sorterPoolCount>, rp.GetSorterPoolCount())
            Assert.Equal(Some 32<sorterCountPerPool>, rp.GetSorterCountPerPool()))

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleS parameter values distinguish setup files execution files and report metadata`` soss =
    let configured = if soss then GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.SossS() else GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.StandardS()
    let host = runHost.Create configured 1
    let setup = host.MakeSetupParameters 0<replNumber> 1<replNumber> |> Array.head
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let setupQuery parameters = host.QueryParamsFromSetupParams parameters (geneSortDataType.SorterPoolSet "") |> Option.get
    Assert.NotEqual((setupQuery setup).Id, (setupQuery (setup.WithActivationProbability(Some 0.75))).Id)
    Assert.NotEqual((setupQuery setup).Id, (setupQuery (setup.WithActivationProbability None)).Id)
    let runQuery parameters = host.QueryParamsFromRunParams parameters (geneSortDataType.SorterPoolSet "") |> Option.get
    Assert.NotEqual((runQuery rp).Id, (runQuery (rp.WithActivationProbability(Some 0.75))).Id)
    Assert.NotEqual((runQuery rp).Id, (runQuery (rp.WithActivatorMutationRate(Some 0.5))).Id)
    let report = host.QueryParamsFromRunParams rp (geneSortDataType.TextReport (UMX.tag "summary")) |> Option.get
    Assert.Equal(string (rp.GetActivationProbability().Value), report.Properties.[ParamKeys.activationProbabilityKey])
    Assert.Equal(rp.ParamMap.[ParamKeys.activatorMutationRateKey], report.Properties.[ParamKeys.activatorMutationRateKey])

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``SimpleS seed generation setup and evolution execute with boolean activators`` soss =
    let configured = if soss then GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.SossS() else GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.StandardS()
    let host = runHost.Create configured 1
    // Use small sorting width and complete binary tests to exercise real makers and execution without external data.
    let rp =
        (host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head)
            .WithSortingWidth(Some 4<sortingWidth>)
            .WithPrefixLibId(Some (prefixLibId.create 4<sortingWidth> 1<stageLength> prefixLibVariant.PrefixA))
            .WithSimpleSorterModelType(Some GeneSort.Model.Sorting.V1.simpleSorterModelType.Msce)
            .WithMutationRate(Some (UMX.tag 1.0)).WithInsertionRate(Some (UMX.tag 0.0)).WithDeletionRate(Some (UMX.tag 0.0))
            .WithActivatorMutationRate(Some 1.0)
            .WithSorterEvalMeasure(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithSorterEvalMeasureInitial(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithSeedSorterPoolSelectionType(Some (sorterSelectionType.GuidOrder 32<sorterCount>))
            .WithDistinctSorterHashes(Some false).WithSortedFraction(Some 1.0<sortedFraction>)
            .WithGenerationIntervalLast(Some (UMX.tag 1))
            .WithGenerationsPerSubset(Some (UMX.tag 1)).WithGenerationsPerMergedSet(Some (UMX.tag 1))
    let rp = rp.ParamMap |> Map.filter (fun key _ -> not (List.contains key SetupFileReferences.keys)) |> runParameters.create
    let rp = SetupInputReferences.recordFiles (SetupOutputs.generatedFiles configured rp) rp
    let memory = MemoryDb(host.QueryParamsFromRunParams)
    let db = memory :> IGeneSortDb
    let tinyTests =
        Array.init 16 (fun bits -> sortableBoolArray.create(Array.init 4 (fun bit -> bits &&& (1 <<< bit) <> 0), 4<sortingWidth>))
        |> sortableBinaryTests.create (UMX.tag Guid.Empty) 4<sortingWidth> |> sortableTests.Bools
    let evaluations =
        Array.init 32 (fun index ->
            sorterEvalV2.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 4<sortingWidth> 16<sortableCount>
                (UMX.tag index) 2<stageLength> [||] (UMX.tag false) (UMX.tag 1) (UMX.tag 0) |> sorterEval.V2)
        |> sorterSetEval.create (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid()))
    let mutable loads = 0
    let sources: evolutionSetupSources = {
        LoadMergeTests = fun _ _ _ -> async { return Error "Unexpected loader" }
        LoadPrefixTests = fun _ _ _ -> async { return Ok tinyTests }
        LoadStandardSorterEvals = fun _ _ _ -> async { return Error "Unexpected loader" }
        LoadMergeSorterEvals = fun _ _ _ -> async { return Error "Unexpected loader" }
        LoadPrefixSorterEvals = fun _ _ _ -> async { loads <- loads + 1; return Ok evaluations }
    }
    let makePool = SeedSorterPoolSetMakerCatalog.get configured.MakerNames.Value.SeedPoolSet sources
    let shortIntervals = { Name = "simple-s-test"; Min = 0; MaxCount = Some 3; Scale = 1.0; Method = Constant 1 }
    use cts = new CancellationTokenSource()
    let setup = if soss then EvoStartCreation.createSossStart configured.ProjectName configured.DatabaseName else EvoStartCreation.createStandardStart configured.ProjectName configured.DatabaseName
    setup (fun _ -> async { return Ok (tinyTests, [||]) }) makePool db rp cts.Token
    |> Async.RunSynchronously |> Result.defaultWith failwith |> ignore
    Assert.Equal(1, loads)
    let execute = if soss then SgdEx_Soss.evaluateEvolutionRunSoss else SgdEx_Standard.evaluateEvolutionRunStandard
    let finished = execute (hostMutator host) db shortIntervals shortIntervals rp (UMX.tag false) cts None
                   |> Async.RunSynchronously |> Result.defaultWith failwith
    Assert.Equal(Some true, finished.GetRunFinished())
    Assert.Equal(1, loads) // Execution consumes the prepared seeds and does not regenerate them.
    let query = db.makeQueryParamsFromRunParams finished (geneSortDataType.SorterPoolSet "") |> Option.get
    let saved = db.loadAsync geneSortDataMode.Results query |> Async.RunSynchronously |> Result.defaultWith failwith
    let models, mutated =
        match saved with
        | outputData.SorterPoolSet (sorterPoolSet.Standard value) ->
            let pool = value.SorterPools |> Map.values |> Seq.exactlyOne
            pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel) |> Seq.toArray,
            pool.SorterPoolMembers |> Seq.exists (fun m -> m.SorterMutationSource.IsSome)
        | outputData.SorterPoolSet (sorterPoolSet.Soss value) ->
            let pool = value.SorterPools |> Map.values |> Seq.exactlyOne
            pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel) |> Seq.toArray,
            pool.SorterPoolMembers |> Seq.exists (fun m -> m.SorterMutationSource.IsSome)
        | _ -> failwith "Expected evolved pool"
    Assert.Equal(32, models.Length)
    Assert.True(mutated)
    Assert.All(models, fun model ->
        let paired = GeneSort.Model.Sorting.V1.SorterModel.asSimpleSorterModelS model
        Assert.Equal(SimpleSorterModel.getLength paired.Model, paired.Activators.Length))

[<Fact>]
let ``Dispatch SimpleS settings reject missing invalid and incompatible activator parameters`` () =
    let configured = GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test.OnePool.SimpleS.StandardS()
    let host = runHost.Create configured 1
    let rp = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let underlying = (hostMutator host) rp |> Async.RunSynchronously |> Result.defaultWith failwith
                     |> GeneSort.Model.Sorting.V1.sorterModelMutator.Simple
    for invalid in [rp.WithActivationProbability None; rp.WithActivationProbability(Some -0.1);
                    rp.WithActivationProbability(Some Double.NaN); rp.WithActivationProbability(Some 1.1);
                    rp.WithAMax(Some 3); rp.WithActivatorMutationRate None;
                    rp.WithActivatorMutationRate(Some Double.NaN); rp.WithActivatorMutationRate(Some 1.1); rp.WithActivatorMutationRate(Some -0.1)] do
        Assert.True(MutatorMakers.applyParameters invalid underlying |> Result.isError)
    let generator = SeedSorterModelGeneration.getSimpleUniformSorterModelGen Lcg 4<sortingWidth>
                        GeneSort.Model.Sorting.V1.simpleSorterModelType.Msce (UMX.tag true)
    Assert.True(SeedSorterModelGeneration.applyParameters (rp.WithActivationProbability(Some -0.1)) generator |> Result.isError)
    Assert.True(SeedSorterModelGeneration.applyParameters (rp.WithAMax(Some 3)) generator |> Result.isError)
    let paired = SeedSorterModelGeneration.applyParameters rp generator |> Result.defaultWith failwith
                 |> GeneSort.Model.Sorting.V1.SorterModelGen.makeSorterModelFromIndex (UMX.tag 0)
    Assert.True(MutatorMakers.validateModels (rp.WithActivationProbability None) [paired] |> Result.isError)
    Assert.True(MutatorMakers.validateModels rp [paired] |> Result.isOk)
