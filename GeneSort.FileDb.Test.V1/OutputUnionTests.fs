namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open FSharp.UMX
open MessagePack
open Xunit
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Soss
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.Mp.V1.Bins
open GeneSort.Eval.Mp.V1.Sgd
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.FileDb.V1

module private Fixtures =
    let poolId = (UMX.tag<sorterPoolId> (Guid.NewGuid()))
    let memberId = (UMX.tag<sorterPoolMemberId> (Guid.NewGuid()))
    let modelId = (UMX.tag<sorterModelId> (Guid.NewGuid()))
    let subsetId = "partition-A" |> UMX.tag<sortableTestsSubsetId>
    let evaluation =
        sorterEvalV2.create ((UMX.tag<sorterId> (Guid.NewGuid()))) subsetId 32<channelCount> 3<sortableCount>
            7<sequenceHash> 2<stageLength> [||] (UMX.tag<isReflectionSymmetric> false)
            (UMX.tag 1) 0<reflectiveCount> |> sorterEval.V2
    let evaluations =
        Map.ofList
            [ subsetId, evaluation
              SortableTestsSubsetId.Default, SorterEval.withSortableTestsSubsetId SortableTestsSubsetId.Default evaluation
              UMX.tag<sortableTestsSubsetId> "partition-B", SorterEval.withSortableTestsSubsetId (UMX.tag<sortableTestsSubsetId> "partition-B") evaluation ]
    let memberStandard =
        spMember_Standard.create memberId sorterModel.Unknown 4<mutationIndex> 5<mutationMod>
            None (Some evaluation) 2<generationNumber>
    let poolStandard =
        sorterPool_Standard.create poolId (Some ((UMX.tag<sorterPoolId> (Guid.NewGuid()))))
            (UMX.tag<sorterPoolName> "pool") (D1 0) [|memberStandard|] 100<ceLength> 5<mutationMod>
    let standard =
        sorterPoolSet_Standard.create ((UMX.tag<sorterPoolSetId> (Guid.NewGuid()))) 9<generationNumber> (Dim1 1) (Some [poolStandard])
    let soss =
        let memberSoss =
            spMember_Soss.create memberId sorterModel.Unknown 4<mutationIndex> 5<mutationMod>
                None evaluations 2<generationNumber>
        let poolSoss =
            sorterPool_Soss.create poolId poolStandard.ParentSorterPoolId poolStandard.Name (D1 0)
                [|memberSoss|] 100<ceLength> 5<mutationMod>
        sorterPoolSet_Soss.create standard.SorterPoolSetId 9<generationNumber> (Dim1 1) (Some [poolSoss])
    let summaryStandard =
        spSummary_Standard.create poolId (UMX.tag<sorterPoolName> "pool")
            100<ceLength> 10<ceLength> 11.0<ceLength> 1.0<ceLength> 2<stageLength>
            3.0<stageLength> 1.0<stageLength> (UMX.tag 1.0) 0.0<reflectiveCount> 3.0
    let summarySoss =
        spSummary_Soss.create poolId (UMX.tag<sorterPoolName> "pool") subsetId
            100<ceLength> 10<ceLength> 11.0<ceLength> 1.0<ceLength> 2<stageLength>
            3.0<stageLength> 1.0<stageLength> (UMX.tag 1.0) 0.0<reflectiveCount> 3.0
    let summarySetStandard =
        spSummarySet_Standard.create ((UMX.tag<sorterPoolSetSummarySetId> (Guid.NewGuid()))) 9<generationNumber>
            [|sorterPoolSetSummary_Standard.Create(standard.SorterPoolSetId, 9<generationNumber>, 0.0, [|summaryStandard|])|]
    let summarySetSoss =
        spSummarySet_Soss.create ((UMX.tag<sorterPoolSetSummarySetId> (Guid.NewGuid()))) 9<generationNumber>
            [|sorterPoolSetSummary_Soss.Create(soss.SorterPoolSetId, 9<generationNumber>, 0.0, [|summarySoss|])|]
    let binsStandard =
        sorterPoolBinsSetSeries_Standard.create ((UMX.tag<sorterPoolBinsSetSeriesId> (Guid.NewGuid())))
            [sorterPoolBinsSet_Standard.create ((UMX.tag<sorterPoolBinsSetId> (Guid.NewGuid()))) standard]
    let binsSoss =
        sorterPoolBinsSetSeries_Soss.create ((UMX.tag<sorterPoolBinsSetSeriesId> (Guid.NewGuid())))
            [sorterPoolBinsSet_Soss.create ((UMX.tag<sorterPoolBinsSetId> (Guid.NewGuid()))) soss]
    let evalV2 = match evaluation with sorterEval.V2 value -> value | _ -> failwith "Fixture must use V2."
    let memberHistoryStandard =
        spMemberHistory_Standard.create poolId memberId modelId 2<generationNumber> 9<generationNumber>
            4<mutationIndex> 5<mutationMod> (Some ((UMX.tag<sorterPoolMemberId> (Guid.NewGuid()))))
            (Some ((UMX.tag<sorterPoolId> (Guid.NewGuid())))) (Some ((UMX.tag<sorterModelMutatorId> (Guid.NewGuid())))) (Some 3<mutationIndex>) (Some evalV2)
    let memberHistorySoss =
        spMemberHistory_Soss.create poolId memberId modelId 2<generationNumber> 9<generationNumber>
            4<mutationIndex> 5<mutationMod> (Some ((UMX.tag<sorterPoolMemberId> (Guid.NewGuid()))))
            (Some ((UMX.tag<sorterPoolId> (Guid.NewGuid())))) (Some ((UMX.tag<sorterModelMutatorId> (Guid.NewGuid())))) (Some 3<mutationIndex>) (Some evalV2)
    let historyStandard =
        spsh_Standard.create(standard.SorterPoolSetId, 9<generationNumber>,
            [spHistory_Standard.create(poolId, 9<generationNumber>, [memberHistoryStandard])])
    let historySoss =
        spsh_Soss.create(soss.SorterPoolSetId, 9<generationNumber>,
            [spHistory_Soss.create(poolId, 9<generationNumber>, [memberHistorySoss])])
    let outputs =
        [ geneSortDataType.SorterPoolSet "", outputData.SorterPoolSet (sorterPoolSet.Standard standard)
          geneSortDataType.SorterPoolSet "", outputData.SorterPoolSet (sorterPoolSet.Soss soss)
          geneSortDataType.SorterPoolSetSummarySet "", outputData.SorterPoolSetSummarySet (spSummarySet.Standard summarySetStandard)
          geneSortDataType.SorterPoolSetSummarySet "", outputData.SorterPoolSetSummarySet (spSummarySet.Soss summarySetSoss)
          geneSortDataType.SorterPoolBinsSetSeries "", outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Standard binsStandard)
          geneSortDataType.SorterPoolBinsSetSeries "", outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Soss binsSoss)
          geneSortDataType.SorterPoolSetHistory "", outputData.SorterPoolSetHistory (spsh.Standard historyStandard)
          geneSortDataType.SorterPoolSetHistory "", outputData.SorterPoolSetHistory (spsh.Soss historySoss) ]
    let serialized = function
        | outputData.SorterPoolSet value -> MessagePackSerializer.Serialize(SorterPoolSetDto.fromDomain value)
        | outputData.SorterPoolSetSummarySet value -> MessagePackSerializer.Serialize(SpSummarySetDto.fromDomain value)
        | outputData.SorterPoolBinsSetSeries value -> MessagePackSerializer.Serialize(SorterPoolBinsSetSeriesDto.fromDomain value)
        | outputData.SorterPoolSetHistory value -> MessagePackSerializer.Serialize(SpshDto.fromDomain value)
        | _ -> failwith "Unexpected output type."
    let unwrap = function Ok value -> value | Error error -> failwith error
    let query dataType = queryParams.create (UMX.tag "db") (UMX.tag "project") (Some 0<replNumber>) (Some 9<generationNumber>) dataType [||]
    let withFolder action =
        let folder = Path.Combine(Path.GetTempPath(), "GeneSort-output-unions-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory folder |> ignore
        try action (UMX.tag<pathToRootFolder> folder)
        finally Directory.Delete(folder, true)

module OutputUnionTests =
    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(2)>]
    let ``sorter model sets round trip as setup data`` (count: int) =
        MessagePackSetup.configure()
        Fixtures.withFolder (fun folder ->
            let models =
                Array.init count (fun index ->
                    msce.create (UMX.tag (Guid.NewGuid())) 4<channelCount> [| 1; 2 + index |]
                    |> simpleSorterModel.Msce |> sorterModel.Simple)
            let original = sorterModelSet.create (UMX.tag (Guid.NewGuid())) models 17<ceLength>
            let dataType = geneSortDataType.SorterModelSet "evaluation_setup"
            let query = Fixtures.query dataType
            OutputDataFile.saveToFileAsync folder geneSortDataMode.Setup query (outputData.SorterModelSet original) (UMX.tag false)
            |> Async.RunSynchronously |> Fixtures.unwrap
            let loaded =
                OutputDataFile.getOutputDataAsync folder geneSortDataMode.Setup query None
                |> Async.RunSynchronously |> Fixtures.unwrap
                |> OutputData.asSorterModelSet |> Fixtures.unwrap
            Assert.Equal(original.Id, loaded.Id)
            Assert.Equal(original.RawCeLength, loaded.RawCeLength)
            Assert.Equal(count, loaded.Count)
            let serialize value =
                value |> GeneSort.Model.Sorting.Mp.V1.SorterModelSetDto.fromDomain |> MessagePackSerializer.Serialize
            Assert.True(serialize original = serialize loaded)
            Assert.Equal(Some dataType, GeneSortDataType.fromFolderName (GeneSortDataType.toFolderName dataType))
            Assert.Equal(dataType, dataType |> GeneSort.Project.Mp.V1.OutputDataTypeDto.fromDomain |> GeneSort.Project.Mp.V1.OutputDataTypeDto.toDomain)
            Assert.True(OutputData.asSorterModelSet (outputData.SorterPoolSet (sorterPoolSet.Soss Fixtures.soss)) |> Result.isError)
            let path = OutputDataFile.getFullOutputDataFilePath folder geneSortDataMode.Setup query |> UMX.untag
            Assert.True(File.Exists path)
            Assert.Contains(Path.Combine("Setup", "SorterModelSet_evaluation_setup"), path)
            let resultPath = OutputDataFile.getFullOutputDataFilePath folder geneSortDataMode.Results query |> UMX.untag
            Assert.False(File.Exists resultPath))

    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``sortable tests with prefix round trip as setup data`` (hasPrefix: bool) =
        MessagePackSetup.configure()
        Fixtures.withFolder (fun folder ->
            let tests =
                GeneSort.Sorting.Sortable.sortableBinaryTests.create (UMX.tag (Guid.NewGuid())) 4<channelCount>
                    [| GeneSort.Sorting.Sortable.sortableBoolArray.create([| true; false; true; false |], 4<channelCount>) |]
                |> GeneSort.Sorting.Sortable.sortableTests.Bools
            let prefix = if hasPrefix then [| ce.create 0 3; ce.create 1 2; ce.create 0 1 |] else [||]
            let dataType = geneSortDataType.SortableTestsWithPrefix "evaluation_setup"
            let query = Fixtures.query dataType
            let original = outputData.SortableTestsWithPrefix (tests, prefix)
            OutputDataFile.saveToFileAsync folder geneSortDataMode.Setup query original (UMX.tag false)
            |> Async.RunSynchronously |> Fixtures.unwrap
            let loadedTests, loadedPrefix =
                OutputDataFile.getOutputDataAsync folder geneSortDataMode.Setup query None
                |> Async.RunSynchronously |> Fixtures.unwrap
                |> OutputData.asSortableTestsWithPrefix |> Fixtures.unwrap
            Assert.Equal(GeneSort.Sorting.Sortable.SortableTests.getId tests, GeneSort.Sorting.Sortable.SortableTests.getId loadedTests)
            let serializeTests value =
                value |> GeneSort.Sorting.Mp.Sortable.SortableTestsDto.fromDomain |> MessagePackSerializer.Serialize
            Assert.True(serializeTests tests = serializeTests loadedTests)
            Assert.True((prefix = loadedPrefix))
            Assert.Equal(Some dataType, GeneSortDataType.fromFolderName (GeneSortDataType.toFolderName dataType))
            Assert.Equal(dataType, dataType |> GeneSort.Project.Mp.V1.OutputDataTypeDto.fromDomain |> GeneSort.Project.Mp.V1.OutputDataTypeDto.toDomain)
            Assert.True(OutputData.asSortableTestsWithPrefix (outputData.SortableTests tests) |> Result.isError)
            let path = OutputDataFile.getFullOutputDataFilePath folder geneSortDataMode.Setup query |> UMX.untag
            Assert.True(File.Exists path)
            Assert.Contains(Path.Combine("Setup", "SortableTestsWithPrefix_evaluation_setup"), path)
            let resultPath = OutputDataFile.getFullOutputDataFilePath folder geneSortDataMode.Results query |> UMX.untag
            Assert.False(File.Exists resultPath))

    [<Theory>]
    [<InlineData(25, 399, "3")>]
    [<InlineData(25, 400, "Merge")>]
    [<InlineData(25, 424, "Merge")>]
    [<InlineData(25, 425, "0")>]
    [<InlineData(25, 825, "Merge")>]
    [<InlineData(250, 400, "Merge")>]
    [<InlineData(250, 649, "Merge")>]
    [<InlineData(250, 650, "0")>]
    let ``Soss merged duration is independent of partition duration`` (mergedDuration: int) (generation: int) expected =
        let parameters = (runParameters.create Map.empty).WithGenerationsPerMergedSet(Some (UMX.tag<generationsPerSubset> mergedDuration))
        let duration = parameters.GetGenerationsPerMergedSet().Value
        let partitions = [| for index in 0 .. 3 -> UMX.tag<sortableTestsPartitionId> (string index) |]
        let actual = CommonSgdSoss.subsetIdForGeneration (100<generationsPerSubset>)  duration (UMX.tag<generationNumber> generation) partitions
        Assert.Equal(expected, UMX.untag actual)
        Assert.True(parameters.WithGenerationsPerMergedSet(None).GetGenerationsPerMergedSet().IsNone)

    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(-1)>]
    let ``Soss rejects nonpositive merged durations`` (duration: int) =
        Assert.Throws<ArgumentException>(fun () ->
            CommonSgdSoss.subsetIdForGeneration 100<generationsPerSubset> (UMX.tag<generationsPerSubset> duration)
                0<generationNumber> [| UMX.tag<sortableTestsPartitionId> "0" |] |> ignore) |> ignore

    [<Fact>]
    let ``Soss query identity includes merged duration`` () =
        let parameters =
            (runParameters.create Map.empty)
                .WithRepl(Some 0<replNumber>)
                .WithCodeModKey(Some (UMX.tag "NoMods"))
                .WithGenerationCurrent(Some 0<generationNumber>)
                .WithSorterCountPerPool(Some 32<sorterCountPerPool>)
                .WithSorterPoolCount(Some 4<sorterPoolCount>)
                .WithParaRate(Some (UMX.tag 1.15))
                .WithSelfSymRate(Some (UMX.tag 1.75))
                .WithSeedModificationRate(Some (UMX.tag 0.02))
                .WithModificationRate(Some (UMX.tag 0.03))
                .WithMutationMod(Some 0<mutationMod>)
                .WithMutatorVariant(Some mutatorVariant.V2)
                .WithSeedSoss(Some (UMX.tag 1234UL))
                .WithPartitionCount(Some 4<GeneSort.Sorting.partitionCount>)
                .WithGenerationsPerSubset(Some 100<generationsPerSubset>)
        QueryParamsBuildersRun.registerAll ()
        let builder = QueryParamsCatalogRun.get QueryCatalogNamesRun.sorterSgdMsufSoss (UMX.tag "Test") (UMX.tag "Soss")
        let query duration =
            builder (parameters.WithGenerationsPerMergedSet(Some duration)) (geneSortDataType.SorterPoolSet "")
            |> Option.get
        let shortQuery = query 25<generationsPerSubset>
        let longQuery = query 250<generationsPerSubset>
        Assert.Equal("25", shortQuery.Properties.[ParamKeys.generationsPerMergedSetKey])
        Assert.NotEqual(shortQuery.Id, longQuery.Id)
        Assert.True((builder parameters (geneSortDataType.SorterPoolSet "")).IsNone)

    [<Theory>]
    [<InlineData(100, 0, true)>]
    [<InlineData(100, 99, false)>]
    [<InlineData(100, 100, true)>]
    [<InlineData(100, 101, false)>]
    [<InlineData(100, 200, true)>]
    [<InlineData(7, 100, false)>]
    [<InlineData(7, 105, true)>]
    [<InlineData(1, 3, true)>]
    let ``Standard and Soss share the configured CE adjustment schedule`` (frequency: int) (generation: int) (expected: bool) =
        let parameters =
            (runParameters.create Map.empty)
                .WithAdjustCeLengthFrequency(Some (UMX.tag<adjustCeLengthFrequency> frequency))
        let actual = CommonSgdSoss.shouldAdjustCeLengths (parameters.GetAdjustCeLengthFrequency().Value) (UMX.tag<generationNumber> generation)
        Assert.Equal(expected, actual)
        Assert.Equal(expected, GenerationSchedule.shouldAdjustCeLengths (parameters.GetAdjustCeLengthFrequency().Value) (UMX.tag<generationNumber> generation))
        Assert.True(parameters.WithAdjustCeLengthFrequency(None).GetAdjustCeLengthFrequency().IsNone)

    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(-1)>]
    let ``Standard and Soss reject nonpositive CE adjustment frequencies`` (frequency: int) =
        Assert.Throws<ArgumentException>(fun () ->
            CommonSgdSoss.shouldAdjustCeLengths (UMX.tag<adjustCeLengthFrequency> frequency) 0<generationNumber>
            |> ignore) |> ignore
        Assert.Throws<ArgumentException>(fun () ->
            GenerationSchedule.shouldAdjustCeLengths (UMX.tag<adjustCeLengthFrequency> frequency) 0<generationNumber>
            |> ignore) |> ignore

    [<Theory>]
    [<InlineData(1, 1)>]
    [<InlineData(2, 30)>]
    [<InlineData(6, 100)>]
    [<InlineData(4, 7)>]
    let ``Soss cycles through the configured partitions then Merge`` (count: int) (duration: int) =
        let parameters =
            (runParameters.create Map.empty)
                .WithPartitionCount(Some (UMX.tag<GeneSort.Sorting.partitionCount> count))
                .WithGenerationsPerSubset(Some (UMX.tag<generationsPerSubset> duration))
        let configuredCount = parameters.GetPartitionCount().Value |> UMX.untag
        let configuredDuration = parameters.GetGenerationsPerSubset().Value
        let partitionIds = Array.init configuredCount (fun index -> UMX.tag<sortableTestsPartitionId> (string index))
        for cycle in 0 .. 1 do
            for index in 0 .. configuredCount do
                let expected = if configuredCount = 1 then "0" elif index = configuredCount then "Merge" else string index
                for offset in [0; duration - 1] do
                    let generation = cycle * ((configuredCount + 1) * duration) + index * duration + offset
                    let actual = CommonSgdSoss.subsetIdForGeneration configuredDuration configuredDuration (UMX.tag<generationNumber> generation) partitionIds
                    Assert.Equal(expected, UMX.untag actual)

    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(-1)>]
    let ``Soss rejects nonpositive subset cycle durations`` (duration: int) =
        Assert.Throws<ArgumentException>(fun () ->
            CommonSgdSoss.subsetIdForGeneration (UMX.tag<generationsPerSubset> duration) 100<generationsPerSubset> 0<generationNumber>
                [|UMX.tag<sortableTestsPartitionId> "0"|] |> ignore) |> ignore

    [<Theory>]
    [<InlineData(false, false)>]
    [<InlineData(true, false)>]
    [<InlineData(false, true)>]
    [<InlineData(true, true)>]
    let ``reports pool score statistics once per member within each pool`` (useSoss: bool) (summaryReport: bool) =
        MessagePackSetup.configure()
        let measure = stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength
        let makeMember (stageCount: int) =
            let modelId = UMX.tag<sorterModelId> (Guid.NewGuid())
            let model = msce.create modelId 32<channelCount> [|1|] |> simpleSorterModel.Msce |> sorterModel.Simple
            let evaluation =
                sorterEvalV2.create (UMX.cast<sorterModelId, sorterId> modelId) Fixtures.subsetId 32<channelCount> 3<sortableCount>
                    7<sequenceHash> (UMX.tag<stageLength> stageCount) [||] (UMX.tag false) (UMX.tag 1) 0<reflectiveCount>
                |> sorterEval.V2
            spMember_Standard.create (UMX.tag<sorterPoolMemberId> (Guid.NewGuid())) model 0<mutationIndex> 5<mutationMod>
                None (Some evaluation) 0<generationNumber>
            |> SpMember_Standard.withSelectionMeasure measure
        let makePool (name: string) members =
            sorterPool_Standard.create (UMX.tag<sorterPoolId> (Guid.NewGuid())) None (UMX.tag<sorterPoolName> name)
                (D1 0) members 100<ceLength> 5<mutationMod>
        let standard =
            sorterPoolSet_Standard.create Fixtures.standard.SorterPoolSetId 0<generationNumber> (Dim1 2)
                (Some [makePool "first" [|makeMember 2; makeMember 8|]; makePool "second" [|makeMember 11|]])
        let rows =
            if useSoss then
                let soss = standard |> SorterPoolSet_Soss.fromStandard |> SorterPoolSet_Soss.withSubsetIdForSelection Fixtures.subsetId
                let firstPool = soss.SorterPools |> Map.values |> Seq.find (fun pool -> %pool.Name = "first")
                let lowMember = firstPool.SorterPoolMembers |> Seq.find (fun memberValue -> %memberValue.Score = 2.0)
                // Extra evaluation rows must not give this member extra weight in the pool average.
                let expandedMember =
                    ["extra-A"; "extra-B"]
                    |> List.fold (fun memberValue subset ->
                        let evaluation = lowMember.SorterEvalForSelection.Value |> SorterEval.withSortableTestsSubsetId (UMX.tag subset)
                        memberValue |> SpMember_Soss.withEval (Some evaluation)) lowMember
                let expandedPool = firstPool |> SorterPool_Soss.upsertMember expandedMember
                let poolSet = soss |> SorterPoolSet_Soss.upsertPool expandedPool
                if summaryReport then
                    let dto =
                        poolSet |> SorterPoolSetSummary_Soss.fromPoolSet
                        |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.toDto
                    let bytes = MessagePackSerializer.Serialize(dto)
                    MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetSummaryDto>(bytes)
                    |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.fromDto
                    |> SorterPoolSetSummary_Soss.toDataTableRecords ""
                else
                    poolSet
                    |> GeneSort.Eval.V1.Sgd.Soss.SorterPoolSetDescription.toDataTableRecordsSnapshot ""
                    |> Seq.toArray
            else
                if summaryReport then
                    let dto = standard |> SorterPoolSetSummary_Standard.fromPoolSet |> SorterPoolSetSummaryDto.toDto
                    let bytes = MessagePackSerializer.Serialize(dto)
                    MessagePackSerializer.Deserialize<sorterPoolSetSummaryDto>(bytes)
                    |> SorterPoolSetSummaryDto.fromDto
                    |> SorterPoolSetSummary_Standard.toDataTableRecords ""
                else
                    standard
                    |> GeneSort.Eval.V1.Sgd.Standard.SorterPoolSetDescription.toDataTableRecordsSnapshot ""
                    |> Seq.toArray
        let expectedRows = if summaryReport then (if useSoss then 4 else 2) else (if useSoss then 5 else 3)
        Assert.Equal(expectedRows, rows.Length)
        for row in rows do
            let expectedAverage, expectedMinimum =
                if useSoss && summaryReport && row.Data.["SortableTestsSubsetId"].StartsWith("extra-") then "2", "2"
                elif row.Data.["SorterPoolName"] = "first" then "5", "2"
                else "11", "11"
            Assert.Equal(expectedAverage, row.Data.["AverageSorterPoolScore"])
            Assert.Equal(expectedMinimum, row.Data.["MinimumSorterPoolScore"])
            if not summaryReport then Assert.True(row.Data.ContainsKey "Score")

    [<Fact>]
    let ``Soss summary scores use each subset evaluation independently of selection`` () =
        MessagePackSetup.configure()
        let measure = stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength
        let makeMember (stageCounts: int list) =
            let memberId = UMX.tag<sorterPoolMemberId> (Guid.NewGuid())
            let evaluations =
                List.zip ["0"; "1"; "Merge"] stageCounts
                |> List.map (fun (subset, stageCount) ->
                    let subsetId = UMX.tag<sortableTestsSubsetId> subset
                    let evaluation =
                        sorterEvalV2.create (UMX.cast<sorterPoolMemberId, sorterId> memberId) subsetId 32<channelCount> 3<sortableCount>
                            7<sequenceHash> (UMX.tag<stageLength> stageCount) [||] (UMX.tag false) (UMX.tag 1) 0<reflectiveCount>
                        |> sorterEval.V2
                    subsetId, evaluation)
                |> Map.ofList
            spMember_Soss.create memberId sorterModel.Unknown 0<mutationIndex> 5<mutationMod> None evaluations 0<generationNumber>
            |> SpMember_Soss.withSelectionMeasure measure
        let pool =
            sorterPool_Soss.create Fixtures.poolId None Fixtures.poolStandard.Name (D1 0)
                [| makeMember [2; 10; 20]; makeMember [8; 14; 30] |] 100<ceLength> 5<mutationMod>
        let poolSet = Fixtures.soss |> SorterPoolSet_Soss.upsertPool pool
        for selection in ["0"; "1"; "Merge"] do
            let dto =
                poolSet |> SorterPoolSet_Soss.withSubsetIdForSelection (UMX.tag selection)
                |> SorterPoolSetSummary_Soss.fromPoolSet
                |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.toDto
            let bytes = MessagePackSerializer.Serialize(dto)
            let rows =
                MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetSummaryDto>(bytes)
                |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.fromDto
                |> SorterPoolSetSummary_Soss.toDataTableRecords ""
            Assert.Equal(3, rows.Length)
            for row in rows do
                let average, minimum =
                    match row.Data.["SortableTestsSubsetId"] with
                    | "0" -> "5", "2"
                    | "1" -> "12", "10"
                    | "Merge" -> "25", "20"
                    | other -> failwithf "Unexpected subset %s" other
                Assert.Equal(average, row.Data.["AverageSorterPoolScore"])
                Assert.Equal(minimum, row.Data.["MinimumSorterPoolScore"])

    [<Fact>]
    let ``summaries without recorded score statistics leave score columns blank`` () =
        let standard = Fixtures.summarySetStandard.SorterPoolSetSummaries.[0] |> SorterPoolSetSummaryDto.toDto
        let soss = Fixtures.summarySetSoss.SorterPoolSetSummaries.[0] |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.toDto
        Assert.True(standard.sorterPoolSummaryDtos.[0].averageSorterPoolScore.IsNone)
        Assert.True(soss.sorterPoolSummaryDtos.[0].minimumSorterPoolScore.IsNone)
        let rows =
            Array.append
                (standard |> SorterPoolSetSummaryDto.fromDto |> SorterPoolSetSummary_Standard.toDataTableRecords "")
                (soss |> GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetSummaryDto.fromDto |> SorterPoolSetSummary_Soss.toDataTableRecords "")
        for row in rows do
            Assert.Equal("", row.Data.["AverageSorterPoolScore"])
            Assert.Equal("", row.Data.["MinimumSorterPoolScore"])

    [<Theory>]
    [<InlineData(4, false)>]
    [<InlineData(1, false)>]
    [<InlineData(1, true)>]
    let ``Soss skips unneeded partitions and avoids a redundant single partition merge`` (count: int) (forReport: bool) =
        let selectedId = UMX.tag<sortableTestsSubsetId> "0"
        let tests =
            GeneSort.Sorting.Sortable.sortableBinaryTests.create (UMX.tag<sortableTestsId> (Guid.NewGuid()))
                32<channelCount> [| GeneSort.Sorting.Sortable.sortableBoolArray.create(Array.zeroCreate<bool> 32, 32<channelCount>) |]
            |> GeneSort.Sorting.Sortable.sortableTests.Bools
        let partitions =
            [0 .. count - 1]
            |> List.map (fun index -> UMX.tag<sortableTestsPartitionId> (string index), tests)
            |> Map.ofList
            |> GeneSort.Sorting.Sortable.partitionOfSortableTests.create
        let cachedEval = SorterEval.withSortableTestsSubsetId selectedId Fixtures.evaluation
        let memberValue =
            Fixtures.soss.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
            |> SpMember_Soss.withEval None
            |> SpMember_Soss.withEval (Some cachedEval)
        let pool = Fixtures.soss.SorterPools.[Fixtures.poolId] |> SorterPool_Soss.upsertMember memberValue
        let poolSet = Fixtures.soss |> SorterPoolSet_Soss.upsertPool pool
        // Unknown models would fail if any of the other, uncached partitions were evaluated.
        let evaluated =
            Pipeline_Soss.evaluateRequiredPartitions partitions Unchecked.defaultof<_> sorterEvalType.V2 false
                (UMX.tag<collectNewSortableTests> false) selectedId forReport poolSet
        let result = evaluated.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(1, result.SorterEvalMap.Count)
        Assert.True(result.SorterEvalMap.[selectedId] = cachedEval)
        Assert.False(Map.containsKey CommonSgdSoss.mergedSubsetId result.SorterEvalMap)

    [<Fact>]
    let ``A single partition retains the original tests without shuffling`` () =
        let tests =
            GeneSort.Sorting.Sortable.sortableBinaryTests.create (UMX.tag<sortableTestsId> (Guid.NewGuid()))
                32<channelCount> [| GeneSort.Sorting.Sortable.sortableBoolArray.create(Array.zeroCreate<bool> 32, 32<channelCount>) |]
            |> GeneSort.Sorting.Sortable.sortableTests.Bools
        let partitions =
            GeneSort.Sorting.Sortable.PartitionOfSortableTests.partition
                (fun _ -> failwith "Single partition must not shuffle tests") 1<sortableTestsCount> tests
        Assert.Equal(1, partitions.Count)
        Assert.Same(tests, partitions.SortableTests.[SortableTestsPartitionId.Default])

    [<Theory>]
    [<InlineData("Merge", false)>]
    [<InlineData("0", true)>]
    let ``Soss completes all partition evaluations for Merge and reports`` (selection: string) (forReport: bool) =
        let selectedId = UMX.tag<sortableTestsSubsetId> selection
        let tests =
            GeneSort.Sorting.Sortable.sortableBinaryTests.create (UMX.tag<sortableTestsId> (Guid.NewGuid()))
                32<channelCount> [| GeneSort.Sorting.Sortable.sortableBoolArray.create(Array.zeroCreate<bool> 32, 32<channelCount>) |]
            |> GeneSort.Sorting.Sortable.sortableTests.Bools
        let partitions =
            [0 .. 3]
            |> List.map (fun index -> UMX.tag<sortableTestsPartitionId> (string index), tests)
            |> Map.ofList
            |> GeneSort.Sorting.Sortable.partitionOfSortableTests.create
        let memberValue =
            Fixtures.soss.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
            |> SpMember_Soss.withEval None
            |> SpMember_Soss.withSubsetIdForSelection selectedId
        let cachedMember =
            [0 .. 3] |> List.fold (fun memberValue index ->
                let evaluation = SorterEval.withSortableTestsSubsetId (UMX.tag<sortableTestsSubsetId> (string index)) Fixtures.evaluation
                memberValue |> SpMember_Soss.withEval (Some evaluation)) memberValue
        let pool = Fixtures.soss.SorterPools.[Fixtures.poolId] |> SorterPool_Soss.upsertMember cachedMember
        let poolSet = Fixtures.soss |> SorterPoolSet_Soss.upsertPool pool
        let evaluated =
            Pipeline_Soss.evaluateRequiredPartitions partitions Unchecked.defaultof<_> sorterEvalType.V2 false
                (UMX.tag<collectNewSortableTests> false) selectedId forReport poolSet
        let result = evaluated.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(5, result.SorterEvalMap.Count)
        Assert.True(result.SubsetIdForSelection = selectedId)
        Assert.True(SorterEval.getSortableTestsSubsetId result.SorterEvalMap.[CommonSgdSoss.mergedSubsetId] = CommonSgdSoss.mergedSubsetId)

    [<Fact>]
    let ``member scores follow evaluation updates and Soss subset changes`` () =
        let measure = stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength
        let standard = Fixtures.memberStandard |> SpMember_Standard.withSelectionMeasure measure
        Assert.Equal(2.0, %standard.Score)
        Assert.True(Double.IsPositiveInfinity %(standard |> SpMember_Standard.withEval None).Score)
        let secondSubset = UMX.tag<sortableTestsSubsetId> "second"
        let secondEval =
            sorterEvalV2.create (SorterEval.getSorterId Fixtures.evaluation) secondSubset 32<channelCount> 3<sortableCount>
                7<sequenceHash> 9<stageLength> [||] (UMX.tag<isReflectionSymmetric> false)
                (UMX.tag 1) 0<reflectiveCount> |> sorterEval.V2
        let updatedStandard = standard |> SpMember_Standard.withEval (Some secondEval)
        Assert.Equal(9.0, %updatedStandard.Score)
        let soss =
            Fixtures.soss.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
            |> SpMember_Soss.withSelectionMeasure measure
            |> SpMember_Soss.withEval (Some secondEval)
            |> SpMember_Soss.withSubsetIdForSelection Fixtures.subsetId
        Assert.Equal(2.0, %soss.Score)
        let selected = soss |> SpMember_Soss.withSubsetIdForSelection secondSubset
        Assert.Equal(9.0, %selected.Score)
        Assert.True(Double.IsPositiveInfinity %(selected |> SpMember_Soss.withEval None).Score)

    [<Fact>]
    let ``selection persists its measure and reports the same score for both member types`` () =
        MessagePackSetup.configure()
        let measure = stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength
        let standardPool =
            SorterPool_Standard.pruneSorterPool Fixtures.poolStandard measure (UMX.tag false) (UMX.tag false) 1<sorterCountPerPool>
        let standardMember = standardPool.SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(2.0, %standardMember.Score)
        let standardSet = Fixtures.standard |> SorterPoolSet_Standard.upsertPool standardPool
        let standardBytes = MessagePackSerializer.Serialize(SorterPoolSetDto_Standard.toDto standardSet)
        let standardDto = MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.sorterPoolSetDto_Standard>(standardBytes)
        let restoredStandard = SorterPoolSetDto_Standard.fromDto standardDto
        let restoredMember = restoredStandard.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(2.0, %restoredMember.Score)
        let standardDescription =
            spDescription_Standard.create restoredMember.SorterPoolMemberId Fixtures.modelId restoredMember.MutationIndex
                restoredMember.SorterMutationSource restoredMember.SorterEval restoredMember.Birthday restoredMember.Score
        let standardRow = SpDescription_Standard.toDataTableRecordWithPrefix "" standardDescription
        Assert.Equal("2", standardRow.Data.["Score"])
        let sossSet = Fixtures.soss |> SorterPoolSet_Soss.withSubsetIdForSelection Fixtures.subsetId
        let sossPool =
            SorterPool_Soss.pruneSorterPool sossSet.SorterPools.[Fixtures.poolId] measure (UMX.tag false) (UMX.tag false) 1<sorterCountPerPool>
        let selectedSet = sossSet |> SorterPoolSet_Soss.upsertPool sossPool
        let sossBytes = MessagePackSerializer.Serialize(GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto selectedSet)
        let sossDto = MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(sossBytes)
        let restoredSoss = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto sossDto
        let restoredSossMember = restoredSoss.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(2.0, %restoredSossMember.Score)
        let sossDescription =
            spDescription_Soss.create restoredSossMember.SorterPoolMemberId Fixtures.modelId restoredSossMember.MutationIndex
                restoredSossMember.SorterMutationSource restoredSossMember.SorterEvalMap restoredSossMember.Birthday restoredSossMember.Score
        let sossRows = SpDescription_Soss.toDataTableRecordsWithPrefix "Test_" sossDescription
        Assert.Equal(3, sossRows.Length)
        sossRows |> Array.iter (fun row -> Assert.Equal("2", row.Data.["Test_Score"]))

    [<Theory>]
    [<InlineData(0, "0")>]
    [<InlineData(99, "0")>]
    [<InlineData(100, "1")>]
    [<InlineData(199, "1")>]
    [<InlineData(200, "2")>]
    [<InlineData(299, "2")>]
    [<InlineData(300, "3")>]
    [<InlineData(399, "3")>]
    [<InlineData(400, "Merge")>]
    [<InlineData(499, "Merge")>]
    [<InlineData(500, "0")>]
    [<InlineData(600, "1")>]
    [<InlineData(700, "2")>]
    [<InlineData(800, "3")>]
    [<InlineData(900, "Merge")>]
    let ``Soss selection rotates every 100 generations and repeats after 500`` (generation: int) (expected: string) =
        let partitions = [| for index in 0 .. 3 -> string index |> UMX.tag<sortableTestsPartitionId> |]
        let selected = CommonSgdSoss.subsetIdForGeneration 100<generationsPerSubset> 100<generationsPerSubset> (UMX.tag<generationNumber> generation) partitions
        Assert.Equal(expected, UMX.untag selected)

    [<Fact>]
    let ``Soss stores both cast partition evaluations and a separate Merge evaluation`` () =
        let firstPartitionId = UMX.tag<sortableTestsPartitionId> "0"
        let secondPartitionId = UMX.tag<sortableTestsPartitionId> "1"
        let firstSubsetId = UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId> firstPartitionId
        let secondSubsetId = UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId> secondPartitionId
        let first = SorterEval.withSortableTestsSubsetId firstSubsetId Fixtures.evaluation
        let second = SorterEval.withSortableTestsSubsetId secondSubsetId Fixtures.evaluation
        let partitionEvals =
            Map.ofList
                [ firstPartitionId, Map.ofList [Fixtures.memberId, first]
                  secondPartitionId, Map.ofList [Fixtures.memberId, second] ]
        let mergedEvals = CommonSgdSoss.mergePartitionEvals partitionEvals
        let merged = mergedEvals.[Fixtures.memberId]
        Assert.True(SorterEval.getSortableTestsSubsetId merged = (UMX.tag<sortableTestsSubsetId> "Merge"))
        Assert.True((merged = (SorterEval.merge first second |> SorterEval.withSortableTestsSubsetId CommonSgdSoss.mergedSubsetId)))
        let memberValue =
            spMember_Soss.create Fixtures.memberId sorterModel.Unknown 0<mutationIndex> 5<mutationMod>
                None Map.empty 0<generationNumber>
        let stored =
            memberValue
            |> SpMember_Soss.withEval (Some first)
            |> SpMember_Soss.withEval (Some second)
            |> SpMember_Soss.withEval (Some merged)
        Assert.Equal(3, stored.SorterEvalMap.Count)
        Assert.True(stored.SorterEvalMap.[firstSubsetId] = first)
        Assert.True(stored.SorterEvalMap.[secondSubsetId] = second)
        let selected = stored |> SpMember_Soss.withSubsetIdForSelection CommonSgdSoss.mergedSubsetId
        Assert.True(selected.SorterEvalForSelection = Some merged)

    [<Fact>]
    let ``Soss selection switches partitions and reuses the matching cached evaluation`` () =
        let selected = Fixtures.soss |> SorterPoolSet_Soss.withSubsetIdForSelection Fixtures.subsetId
        let memberValue = selected.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.True(memberValue.SorterEvalForSelection = Some Fixtures.evaluation)
        Assert.True(memberValue.SorterEvalMap = Fixtures.evaluations)
        let cached =
            PoolRunner_Soss.evaluatePoolSet Unchecked.defaultof<_> Unchecked.defaultof<_>
                sorterEvalType.V2 false (UMX.tag<collectNewSortableTests> false) selected
        Assert.True(cached.[Fixtures.memberId] = Fixtures.evaluation)
        let missing = memberValue |> SpMember_Soss.withSubsetIdForSelection (UMX.tag<sortableTestsSubsetId> "missing")
        Assert.True(missing.SorterEvalForSelection.IsNone)

    [<Fact>]
    let ``Soss selection subset survives serialization and legacy DTOs default it`` () =
        MessagePackSetup.configure()
        let selected = Fixtures.soss |> SorterPoolSet_Soss.withSubsetIdForSelection Fixtures.subsetId
        let dto = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.toDto selected
        let bytes = MessagePackSerializer.Serialize(dto)
        let restoredDto = MessagePackSerializer.Deserialize<GeneSort.Eval.Mp.V1.Sgd.Soss.sorterPoolSetDto_Soss>(bytes)
        let restored = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto restoredDto
        let memberValue = restored.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.True(memberValue.SubsetIdForSelection = Fixtures.subsetId)
        Assert.True(memberValue.SorterEvalForSelection = Some Fixtures.evaluation)
        let legacyPools =
            dto.sorterPools
            |> Array.map (fun pool ->
                let legacyMembers =
                    pool.sorterPoolMemberDtos
                    |> Array.map (fun memberDto -> { memberDto with subsetIdForSelection = null })
                { pool with sorterPoolMemberDtos = legacyMembers })
        let legacyDto = { dto with sorterPools = legacyPools }
        let legacy = GeneSort.Eval.Mp.V1.Sgd.Soss.SorterPoolSetDto_Soss.fromDto legacyDto
        let legacyMember = legacy.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.True(legacyMember.SubsetIdForSelection = SortableTestsSubsetId.Default)

    [<Fact>]
    let ``Standard reuses its single cached evaluation regardless of subset id`` () =
        // Unknown models and absent test inputs would fail if this tried to evaluate again.
        let cached =
            PoolRunner_Standard.evaluatePoolSet Unchecked.defaultof<_> Unchecked.defaultof<_>
                sorterEvalType.V2 false (UMX.tag<collectNewSortableTests> false) Fixtures.standard
        Assert.True(cached.[Fixtures.memberId] = Fixtures.evaluation)
        let replacement = SorterEval.withSortableTestsSubsetId SortableTestsSubsetId.Default Fixtures.evaluation
        let updated = SorterPoolSet_Standard.updateSorterEvals (Map.ofList [Fixtures.memberId, replacement]) Fixtures.standard
        let memberValue = updated.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.True(memberValue.SorterEval = Some replacement)
        Assert.True((SpMember_Standard.withEval None memberValue).SorterEval.IsNone)

    [<Fact>]
    let ``Standard summaries and snapshots retain unevaluated pools and members`` () =
        let model =
            msce.create Fixtures.modelId 32<channelCount> [|1|]
            |> simpleSorterModel.Msce
            |> sorterModel.Simple
        let memberWithModel =
            spMember_Standard.create Fixtures.memberId model 4<mutationIndex> 5<mutationMod>
                None (Some Fixtures.evaluation) 2<generationNumber>
        let unevaluated = SpMember_Standard.withEval None memberWithModel
        let pool =
            sorterPool_Standard.create Fixtures.poolId None Fixtures.poolStandard.Name (D1 0)
                [|unevaluated|] 100<ceLength> 5<mutationMod>
        let poolSet =
            sorterPoolSet_Standard.create Fixtures.standard.SorterPoolSetId 9<generationNumber> (Dim1 1) (Some [pool])
        let summary = SorterPoolSetSummary_Standard.fromPoolSet poolSet
        Assert.Single(summary.SorterPoolSummaries) |> ignore
        Assert.Equal(0.0, summary.SorterPoolSummaries.[0].AverageUnsortedCount)
        Assert.Equal(0<ceLength>, summary.SorterPoolSummaries.[0].MinCeLength)
        let rows =
            global.GeneSort.Eval.V1.Sgd.Standard.SorterPoolSetDescription.fromPoolSet poolSet
            |> global.GeneSort.Eval.V1.Sgd.Standard.SorterPoolSetDescription.toDataTableRecords ""
        Assert.Single(rows) |> ignore
        let evaluatedSummary = SorterPoolSetSummary_Standard.fromPoolSet Fixtures.standard
        Assert.Single(evaluatedSummary.SorterPoolSummaries) |> ignore
        Assert.Equal(3.0, evaluatedSummary.SorterPoolSummaries.[0].AverageUnsortedCount)
        let bins = sorterPoolBins_Standard.create (UMX.tag<sorterPoolBinsId> (Guid.NewGuid())) Fixtures.poolStandard
        Assert.Equal(1, bins.Bins |> Map.values |> Seq.sumBy (fun bin -> bin.SorterEvals.Count))
        let history = SpMemberHistory_Standard.fromPoolMember Fixtures.poolId None None 9<generationNumber> memberWithModel
        Assert.True(history.EvalV2 = Some Fixtures.evalV2)

    [<Fact>]
    let ``Standard seed conversion preserves the evaluation in the Soss subset map`` () =
        let converted = SorterPoolSet_Soss.fromStandard Fixtures.standard
        let memberValue = converted.SorterPools.[Fixtures.poolId].SorterPoolMembers |> Seq.exactlyOne
        Assert.Equal(1, memberValue.SorterEvalMap.Count)
        Assert.True(memberValue.SorterEvalMap.[Fixtures.subsetId] = Fixtures.evaluation)

    [<Fact>]
    let ``Standard seed conversion discards Default evaluations while preserving seed identities`` () =
        let evaluation = SorterEval.withSortableTestsSubsetId SortableTestsSubsetId.Default Fixtures.evaluation
        let memberValue = Fixtures.memberStandard |> SpMember_Standard.withEval (Some evaluation)
        let pool =
            sorterPool_Standard.create Fixtures.poolId Fixtures.poolStandard.ParentSorterPoolId
                Fixtures.poolStandard.Name Fixtures.poolStandard.SorterPoolTag [|memberValue|]
                Fixtures.poolStandard.RawCeLength Fixtures.poolStandard.MutationMod
        let source = Fixtures.standard |> SorterPoolSet_Standard.upsertPool pool
        let converted = SorterPoolSet_Soss.fromStandard source
        let convertedPool = converted.SorterPools.[Fixtures.poolId]
        let convertedMember = convertedPool.SorterPoolMembers |> Seq.exactlyOne
        Assert.Empty(convertedMember.SorterEvalMap)
        Assert.Equal(source.SorterPoolSetId, converted.SorterPoolSetId)
        Assert.Equal(source.GenerationNumber, converted.GenerationNumber)
        Assert.Equal(pool.ParentSorterPoolId, convertedPool.ParentSorterPoolId)
        Assert.Equal(memberValue.SorterPoolMemberId, convertedMember.SorterPoolMemberId)
        Assert.Equal(memberValue.MutationMod, convertedMember.MutationMod)
        Assert.Equal(memberValue.Birthday, convertedMember.Birthday)

    [<Fact>]
    let ``all output variants round trip through files with nested state intact`` () =
        MessagePackSetup.configure()
        for dataType, original in Fixtures.outputs do
            Fixtures.withFolder (fun folder ->
                let qp = Fixtures.query dataType
                OutputDataFile.saveToFileAsync folder geneSortDataMode.Results qp original (UMX.tag<allowOverwrite> false)
                |> Async.RunSynchronously |> Fixtures.unwrap
                let loaded = OutputDataFile.getOutputDataAsync folder geneSortDataMode.Results qp None |> Async.RunSynchronously |> Fixtures.unwrap
                Assert.True(Fixtures.serialized original = Fixtures.serialized loaded, "Workflow tag or nested payload changed."))

    [<Fact>]
    let ``data modes isolate result files and database lookups`` () =
        MessagePackSetup.configure()
        Fixtures.withFolder (fun folder ->
            let db = GeneSortDbMp(folder, QueryCatalogNamesRun.sorterSgdMsufSoss) :> IGeneSortDb
            let dataType, original = Fixtures.outputs.[0]
            let qp = Fixtures.query dataType
            for mode in [geneSortDataMode.Project; geneSortDataMode.Setup; geneSortDataMode.Results] do
                Assert.False(db.doesOutPutDataExist mode qp |> Async.RunSynchronously)
                Assert.True((db.loadIfFoundAsync mode qp |> Async.RunSynchronously).IsNone)
                db.saveAsync mode qp original (UMX.tag false) |> Async.RunSynchronously |> Fixtures.unwrap
                let path = OutputDataFile.getFullOutputDataFilePath folder mode qp |> UMX.untag
                let replicaFolder = if mode = geneSortDataMode.Setup then "" else "0"
                Assert.Equal(Path.Combine(%folder, GeneSortDataMode.toFolderName mode, "SorterPoolSet", replicaFolder, string %qp.Id + ".msgpack"), path)
                Assert.True(File.Exists path)
                Assert.True(db.doesOutPutDataExist mode qp |> Async.RunSynchronously)
                let loaded = db.loadAsync mode qp |> Async.RunSynchronously |> Fixtures.unwrap
                Assert.True(Fixtures.serialized original = Fixtures.serialized loaded)
                Assert.True((db.loadIfFoundAsync mode qp |> Async.RunSynchronously).IsSome))

    [<Fact>]
    let ``runs and run parameters always use Project and bulk loading finds parameters`` () =
        MessagePackSetup.configure()
        Fixtures.withFolder (fun folder ->
            let runName = UMX.tag<runName> "TestRun"
            let runQuery = Fixtures.query (geneSortDataType.Run runName)
            let paramQuery = Fixtures.query (geneSortDataType.RunParameters runName)
            for mode in [geneSortDataMode.Project; geneSortDataMode.Setup; geneSortDataMode.Results] do
                let runPath = OutputDataFile.getFullOutputDataFilePath folder mode runQuery |> UMX.untag
                Assert.Equal(Path.Combine(%folder, "Project", "Run", "TestRun", "Run_TestRun.msgpack"), runPath)
                let paramPath = OutputDataFile.getFullOutputDataFilePath folder mode paramQuery |> UMX.untag
                Assert.Equal(Path.Combine(%folder, "Project", "Run", "RunParameters_TestRun", "0", string %paramQuery.Id + ".msgpack"), paramPath)
            let parameters = runParameters.create (Map.ofList [ParamKeys.replKey, "0"])
            OutputDataFile.saveToFileAsync folder geneSortDataMode.Results paramQuery (outputData.RunParameters parameters) (UMX.tag false)
            |> Async.RunSynchronously |> Fixtures.unwrap
            let loaded = OutputDataFile.getRunParameters folder runName (Some 0<replNumber>) (Some 1<replNumber>) None None
                         |> Async.RunSynchronously |> Fixtures.unwrap
            Assert.Single(loaded) |> ignore
            Assert.True(parameters.ParamMap = loaded.[0].ParamMap))

    [<Fact>]
    let ``legacy Standard files load into Standard union cases`` () =
        MessagePackSetup.configure()
        let legacyFiles =
            [ Fixtures.outputs.[0], MessagePackSerializer.Serialize(SorterPoolSetDto_Standard.toDto Fixtures.standard)
              Fixtures.outputs.[2], MessagePackSerializer.Serialize(SorterPoolSetSummarySetDto_Standard.toDto Fixtures.summarySetStandard)
              Fixtures.outputs.[4], MessagePackSerializer.Serialize(SorterPoolBinsSetSeriesDto_Standard.fromDomain Fixtures.binsStandard)
              Fixtures.outputs.[6], MessagePackSerializer.Serialize(SorterPoolSetHistoryDto_Standard.fromDomain Fixtures.historyStandard) ]
        for (dataType, expected), bytes in legacyFiles do
            Fixtures.withFolder (fun folder ->
                let qp = Fixtures.query dataType
                let path = OutputDataFile.getFullOutputDataFilePath folder geneSortDataMode.Results qp |> UMX.untag
                Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
                File.WriteAllBytes(path, bytes)
                let loaded = OutputDataFile.getOutputDataAsync folder geneSortDataMode.Results qp None |> Async.RunSynchronously |> Fixtures.unwrap
                Assert.True(Fixtures.serialized expected = Fixtures.serialized loaded, "Legacy Standard payload changed."))

    [<Fact>]
    let ``workflow extractors reject the opposite pool variant`` () =
        Assert.True(OutputData.asSorterPoolSetStandard (outputData.SorterPoolSet (sorterPoolSet.Soss Fixtures.soss)) |> Result.isError)
        Assert.True(OutputData.asSorterPoolSetSoss (outputData.SorterPoolSet (sorterPoolSet.Standard Fixtures.standard)) |> Result.isError)
