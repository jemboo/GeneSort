namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open Xunit
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Bins.Soss
open GeneSort.Eval.Mp.V1.Sgd
open GeneSort.Eval.Mp.V1.Bins
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1

module private Fixtures =
    let poolId = (UMX.tag<sorterPoolId> (Guid.NewGuid()))
    let memberId = (UMX.tag<sorterPoolMemberId> (Guid.NewGuid()))
    let modelId = (UMX.tag<sorterModelId> (Guid.NewGuid()))
    let subsetId = "partition-A" |> UMX.tag<sortableTestsSubsetId>
    let evaluation =
        sorterEvalV2.create ((UMX.tag<sorterId> (Guid.NewGuid()))) subsetId 32<sortingWidth> 3<sortableCount>
            7<sequenceHash> 2<stageLength> [||] (UMX.tag<isReflectionSymmetric> false)
            (UMX.tag 1) 0<reflectiveCount> |> sorterEval.V2
    let evaluations =
        Map.ofList
            [ subsetId, evaluation
              SortableTestsSubsetId.Default, SorterEval.withSortableTestsSubsetId SortableTestsSubsetId.Default evaluation
              UMX.tag<sortableTestsSubsetId> "partition-B", SorterEval.withSortableTestsSubsetId (UMX.tag<sortableTestsSubsetId> "partition-B") evaluation ]
    let memberStandard =
        spMember_Standard.create memberId sorterModel.Unknown 4<mutationIndex> 5<mutationMod>
            None evaluations 2<generationNumber>
    let poolStandard =
        sorterPool_Standard.create poolId (Some ((UMX.tag<sorterPoolId> (Guid.NewGuid()))))
            (UMX.tag<sorterPoolName> "pool") (D1 0) [|memberStandard|] 100<ceLength> 5<mutationMod>
    let standard =
        sorterPoolSet_Standard.create ((UMX.tag<sorterPoolSetId> (Guid.NewGuid()))) 9<generationNumber> (Dim1 1) (Some [poolStandard])
    let soss = SorterPoolSet_Soss.fromStandard standard
    let summaryStandard =
        spSummary_Standard.create poolId (UMX.tag<sorterPoolName> "pool") subsetId
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
        [ outputDataType.SorterPoolSet "", outputData.SorterPoolSet (sorterPoolSet.Standard standard)
          outputDataType.SorterPoolSet "", outputData.SorterPoolSet (sorterPoolSet.Soss soss)
          outputDataType.SorterPoolSetSummarySet "", outputData.SorterPoolSetSummarySet (spSummarySet.Standard summarySetStandard)
          outputDataType.SorterPoolSetSummarySet "", outputData.SorterPoolSetSummarySet (spSummarySet.Soss summarySetSoss)
          outputDataType.SorterPoolBinsSetSeries "", outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Standard binsStandard)
          outputDataType.SorterPoolBinsSetSeries "", outputData.SorterPoolBinsSetSeries (sorterPoolBinsSetSeries.Soss binsSoss)
          outputDataType.SorterPoolSetHistory "", outputData.SorterPoolSetHistory (spsh.Standard historyStandard)
          outputDataType.SorterPoolSetHistory "", outputData.SorterPoolSetHistory (spsh.Soss historySoss) ]
    let serialized = function
        | outputData.SorterPoolSet value -> MessagePackSerializer.Serialize(SorterPoolSetUnionDto.fromDomain value)
        | outputData.SorterPoolSetSummarySet value -> MessagePackSerializer.Serialize(SpSummarySetUnionDto.fromDomain value)
        | outputData.SorterPoolBinsSetSeries value -> MessagePackSerializer.Serialize(SorterPoolBinsSetSeriesUnionDto.fromDomain value)
        | outputData.SorterPoolSetHistory value -> MessagePackSerializer.Serialize(SpshUnionDto.fromDomain value)
        | _ -> failwith "Unexpected output type."
    let unwrap = function Ok value -> value | Error error -> failwith error
    let query dataType = queryParams.create (UMX.tag "db") (UMX.tag "project") (Some 0<replNumber>) (Some 9<generationNumber>) dataType [||]
    let withFolder action =
        let folder = Path.Combine(Path.GetTempPath(), "GeneSort-output-unions-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory folder |> ignore
        try action (UMX.tag<pathToRootFolder> folder)
        finally Directory.Delete(folder, true)

module OutputUnionTests =
    [<Fact>]
    let ``all output variants round trip through files with nested state intact`` () =
        MessagePackSetup.configure()
        for dataType, original in Fixtures.outputs do
            Fixtures.withFolder (fun folder ->
                let qp = Fixtures.query dataType
                OutputDataFile.saveToFileAsync folder qp original (UMX.tag<allowOverwrite> false)
                |> Async.RunSynchronously |> Fixtures.unwrap
                let loaded = OutputDataFile.getOutputDataAsync folder qp None |> Async.RunSynchronously |> Fixtures.unwrap
                Assert.True(Fixtures.serialized original = Fixtures.serialized loaded, "Workflow tag or nested payload changed."))

    [<Fact>]
    let ``legacy Standard files load into Standard union cases`` () =
        MessagePackSetup.configure()
        let legacyFiles =
            [ Fixtures.outputs.[0], MessagePackSerializer.Serialize(SorterPoolSetDto.toDto Fixtures.standard)
              Fixtures.outputs.[2], MessagePackSerializer.Serialize(SorterPoolSetSummarySetDto.toDto Fixtures.summarySetStandard)
              Fixtures.outputs.[4], MessagePackSerializer.Serialize(SorterPoolBinsSetSeriesDto.fromDomain Fixtures.binsStandard)
              Fixtures.outputs.[6], MessagePackSerializer.Serialize(SorterPoolSetHistoryDto.fromDomain Fixtures.historyStandard) ]
        for (dataType, expected), bytes in legacyFiles do
            Fixtures.withFolder (fun folder ->
                let qp = Fixtures.query dataType
                let path = OutputDataFile.getFullOutputDataFilePath folder qp |> UMX.untag
                Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
                File.WriteAllBytes(path, bytes)
                let loaded = OutputDataFile.getOutputDataAsync folder qp None |> Async.RunSynchronously |> Fixtures.unwrap
                Assert.True(Fixtures.serialized expected = Fixtures.serialized loaded, "Legacy Standard payload changed."))

    [<Fact>]
    let ``workflow extractors reject the opposite pool variant`` () =
        Assert.True(OutputData.asSorterPoolSetStandard (outputData.SorterPoolSet (sorterPoolSet.Soss Fixtures.soss)) |> Result.isError)
        Assert.True(OutputData.asSorterPoolSetSoss (outputData.SorterPoolSet (sorterPoolSet.Standard Fixtures.standard)) |> Result.isError)
