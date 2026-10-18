module GeneSort.Dispatch.Test.V2.ParameterSetTests

open Xunit
open GeneSort.Project.V1.Parameters
open GeneSort.Project.Mp.V1
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

[<Fact>]
let ``Setup updates preserve their type and leave the original unchanged`` () =
    let original = setupParameters.create Map.empty
    let updated: setupParameters =
        original.WithSourceGenerationInterval(Some 14).WithRunFinished(Some false)
    Assert.Equal(Some 14, updated.GetSourceGenerationInterval())
    Assert.Equal(Some false, updated.GetRunFinished())
    Assert.True(original.ParamMap.IsEmpty)
    Assert.True(updated.WithSourceGenerationInterval(None).GetSourceGenerationInterval().IsNone)

[<Fact>]
let ``Setup and run parameters are distinct types with the same keys and parsing`` () =
    Assert.NotEqual(typeof<runParameters>, typeof<setupParameters>)
    Assert.Equal(runParameters.sourceGenerationIntervalKey, setupParameters.sourceGenerationIntervalKey)
    let values = Map.ofList [setupParameters.sourceGenerationIntervalKey, "14"; setupParameters.runFinishedKey, "invalid"]
    let setup = setupParameters.create values
    let run: runParameters = (runParameters.create values).WithRunFinished(None)
    Assert.Equal(run.GetSourceGenerationInterval(), setup.GetSourceGenerationInterval())
    Assert.True(setup.GetRunFinished().IsNone)

[<Fact>]
let ``Setup parameters round trip through MessagePack including custom keys`` () =
    let options =
        MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(FSharpResolver.Instance, ContractlessStandardResolver.Instance))
    for values in [Map.empty; Map.ofList [setupParameters.sourceGenerationIntervalKey, "14"; "Custom", "value\nwith Unicode: λ"]] do
        let original = setupParameters.create values
        let bytes = MessagePackSerializer.Serialize(SetupParametersDto.fromDomain original, options)
        let restored = MessagePackSerializer.Deserialize<setupParametersDto>(System.ReadOnlyMemory<byte>(bytes), options) |> SetupParametersDto.toDomain
        Assert.Equal<setupParameters>(original, restored)

[<Fact>]
let ``Setup parameter selection handles unique missing and ambiguous matches`` () =
    let first = setupParameters.create (Map.ofList ["Group", "A"; "Item", "1"])
    let second = setupParameters.create (Map.ofList ["Group", "A"; "Item", "2"])
    let parameters = [|first; second|]
    Assert.Equal(Some first, SetupParameters.pickByParameters parameters [|"Item", "1"|])
    Assert.True((SetupParameters.pickByParameters parameters [|"Group", "A"|]).IsNone)
    Assert.True((SetupParameters.pickByParameters parameters [|"Item", "3"|]).IsNone)
    Assert.Equal(2, (SetupParameters.filterByParameters parameters [|"Group", "A"|]).Length)

[<Fact>]
let ``Setup tables include all keys and placeholders for missing values`` () =
    let parameters = [|setupParameters.create (Map.ofList ["A", "1"]); setupParameters.create (Map.ofList ["B", "2"])|]
    let table = SetupParameters.makeIndexAndReplTable parameters
    Assert.Equal<string>([|"Setup_Index"; "A"; "B"|], table.[0])
    Assert.Equal<string>([|"0"; "1"; "N/A"|], table.[1])
    Assert.Equal<string>([|"1"; "N/A"; "2"|], table.[2])
    Assert.Contains("Setup_Index\tA\tB", SetupParameters.toStringTable parameters)
    Assert.Equal("\tA: 1\n", SetupParameters.reportKvps parameters.[0])

[<Fact>]
let ``each output case has an independent typed file ID parameter`` () =
    let accessors = [|
        "Run", setupParameters.runIdKey, (fun (p: setupParameters) -> p.GetRunId()), (fun id (p: setupParameters) -> p.WithRunId(id))
        "RunParameters", setupParameters.runParametersIdKey, (fun (p: setupParameters) -> p.GetRunParametersId()), (fun id (p: setupParameters) -> p.WithRunParametersId(id))
        "SortableTests", setupParameters.sortableTestsIdKey, (fun (p: setupParameters) -> p.GetSortableTestsId()), (fun id (p: setupParameters) -> p.WithSortableTestsId(id))
        "SortableTestsWithPrefix", setupParameters.sortableTestsWithPrefixIdKey, (fun (p: setupParameters) -> p.GetSortableTestsWithPrefixId()), (fun id (p: setupParameters) -> p.WithSortableTestsWithPrefixId(id))
        "SorterPoolSet", setupParameters.sorterPoolSetIdKey, (fun (p: setupParameters) -> p.GetSorterPoolSetId()), (fun id (p: setupParameters) -> p.WithSorterPoolSetId(id))
        "SorterPoolSetSummarySet", setupParameters.sorterPoolSetSummarySetIdKey, (fun (p: setupParameters) -> p.GetSorterPoolSetSummarySetId()), (fun id (p: setupParameters) -> p.WithSorterPoolSetSummarySetId(id))
        "SorterSet", setupParameters.sorterSetIdKey, (fun (p: setupParameters) -> p.GetSorterSetId()), (fun id (p: setupParameters) -> p.WithSorterSetId(id))
        "SorterModelSet", setupParameters.sorterModelSetIdKey, (fun (p: setupParameters) -> p.GetSorterModelSetId()), (fun id (p: setupParameters) -> p.WithSorterModelSetId(id))
        "SorterSetEval", setupParameters.sorterSetEvalIdKey, (fun (p: setupParameters) -> p.GetSorterSetEvalId()), (fun id (p: setupParameters) -> p.WithSorterSetEvalId(id))
        "SorterPoolBinsSetSeries", setupParameters.sorterPoolBinsSetSeriesIdKey, (fun (p: setupParameters) -> p.GetSorterPoolBinsSetSeriesId()), (fun id (p: setupParameters) -> p.WithSorterPoolBinsSetSeriesId(id))
        "SorterPoolSetHistory", setupParameters.sorterPoolSetHistoryIdKey, (fun (p: setupParameters) -> p.GetSorterPoolSetHistoryId()), (fun id (p: setupParameters) -> p.WithSorterPoolSetHistoryId(id))
        "TextReport", setupParameters.textReportIdKey, (fun (p: setupParameters) -> p.GetTextReportId()), (fun id (p: setupParameters) -> p.WithTextReportId(id))
    |]
    let caseNames = Microsoft.FSharp.Reflection.FSharpType.GetUnionCases(typeof<GeneSort.Project.V1.outputData>) |> Array.map (fun c -> c.Name) |> Set.ofArray
    Assert.True((caseNames = (accessors |> Array.map (fun (name, _, _, _) -> name) |> Set.ofArray)))
    let original = setupParameters.create Map.empty
    let ids = accessors |> Array.map (fun _ -> FSharp.UMX.UMX.tag<GeneSort.Project.V1.queryParamsId> (System.Guid.NewGuid()))
    let populated =
        Array.zip accessors ids
        |> Array.fold (fun p ((_, _, _, set), id) -> set (Some id) p) original
    Assert.True(original.ParamMap.IsEmpty)
    Assert.Equal(accessors.Length, populated.ParamMap.Count)
    for index in 0 .. accessors.Length - 1 do
        let _, key, get, set = accessors.[index]
        Assert.Equal(Some ids.[index], get populated)
        Assert.True((get (setupParameters.create (Map.ofList [key, "invalid-guid"]))).IsNone)
        let removed = set None populated
        Assert.True((get removed).IsNone)
        Assert.Equal(accessors.Length - 1, removed.ParamMap.Count)
    let restored = populated |> SetupParametersDto.fromDomain |> SetupParametersDto.toDomain
    Assert.Equal<setupParameters>(populated, restored)
    let run = runParameters.create populated.ParamMap
    Assert.Equal(Some ids.[0], run.GetRunId())
    Assert.Equal(Some ids.[accessors.Length - 1], run.GetTextReportId())
