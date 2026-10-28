module GeneSort.Dispatch.Test.V1.ParameterSetTests

open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open Xunit
open GeneSort.Project.V1.Parameters
open GeneSort.Project.Mp.V1

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
let ``Setup parameters round trip through MessagePack including custom keys`` () =
    let options =
        MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(FSharpResolver.Instance, ContractlessStandardResolver.Instance))
    for values in [Map.empty; Map.ofList [ParamKeys.sourceGenerationIntervalKey, "14"; "Custom", "value\nwith Unicode: λ"]] do
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
let ``each setup output case has an independent typed file ID parameter`` () =
    let accessors = [|
        "SortableTests", ParamKeys.setupSortableTestsIdKey, (fun (p: setupParameters) -> p.GetSetupSortableTestsId()), (fun id (p: setupParameters) -> p.WithSetupSortableTestsId(id))
        "SortableTestsWithPrefix", ParamKeys.setupSortableTestsWithPrefixIdKey, (fun (p: setupParameters) -> p.GetSetupSortableTestsWithPrefixId()), (fun id (p: setupParameters) -> p.WithSetupSortableTestsWithPrefixId(id))
        "SorterPoolSet", ParamKeys.setupSorterPoolSetIdKey, (fun (p: setupParameters) -> p.GetSetupSorterPoolSetId()), (fun id (p: setupParameters) -> p.WithSetupSorterPoolSetId(id))
        "SorterPoolSetSummarySet", ParamKeys.setupSorterPoolSetSummarySetIdKey, (fun (p: setupParameters) -> p.GetSetupSorterPoolSetSummarySetId()), (fun id (p: setupParameters) -> p.WithSetupSorterPoolSetSummarySetId(id))
        "SorterSet", ParamKeys.setupSorterSetIdKey, (fun (p: setupParameters) -> p.GetSetupSorterSetId()), (fun id (p: setupParameters) -> p.WithSetupSorterSetId(id))
        "SorterModelSet", ParamKeys.setupSorterModelSetIdKey, (fun (p: setupParameters) -> p.GetSetupSorterModelSetId()), (fun id (p: setupParameters) -> p.WithSetupSorterModelSetId(id))
        "SorterSetEval", ParamKeys.setupSorterSetEvalIdKey, (fun (p: setupParameters) -> p.GetSetupSorterSetEvalId()), (fun id (p: setupParameters) -> p.WithSetupSorterSetEvalId(id))
        "SorterPoolBinsSetSeries", ParamKeys.setupSorterPoolBinsSetSeriesIdKey, (fun (p: setupParameters) -> p.GetSetupSorterPoolBinsSetSeriesId()), (fun id (p: setupParameters) -> p.WithSetupSorterPoolBinsSetSeriesId(id))
        "SorterPoolSetHistory", ParamKeys.setupSorterPoolSetHistoryIdKey, (fun (p: setupParameters) -> p.GetSetupSorterPoolSetHistoryId()), (fun id (p: setupParameters) -> p.WithSetupSorterPoolSetHistoryId(id))
    |]
    let caseNames = Microsoft.FSharp.Reflection.FSharpType.GetUnionCases(typeof<GeneSort.Project.V1.outputData>) |> Array.map (fun c -> c.Name) |> Array.filter (fun name -> not (List.contains name ["Run"; "RunParameters"; "TextReport"])) |> Set.ofArray
    Assert.True((caseNames = (accessors |> Array.map (fun (name, _, _, _) -> name) |> Set.ofArray)))
    let original = setupParameters.create Map.empty
    let ids = accessors |> Array.map (fun _ -> FSharp.UMX.UMX.tag<GeneSort.Project.V1.queryParamsId> (System.Guid.NewGuid()))
    let populated =
        Array.zip accessors ids
        |> Array.fold (fun p ((_, _, _, set), id) -> set (Some id) p) original
    Assert.True(original.ParamMap.IsEmpty)
    Assert.Equal(accessors.Length, populated.ParamMap.Count)
    for index in 0 .. accessors.Length - 1 do
        let name, key, get, set = accessors.[index]
        Assert.Equal("Setup" + name + "Id", key)
        Assert.Equal(Some ids.[index], get populated)
        Assert.True((get (setupParameters.create (Map.ofList [key, "invalid-guid"]))).IsNone)
        let removed = set None populated
        Assert.True((get removed).IsNone)
        Assert.Equal(accessors.Length - 1, removed.ParamMap.Count)
    let restored = populated |> SetupParametersDto.fromDomain |> SetupParametersDto.toDomain
    Assert.Equal<setupParameters>(populated, restored)
    let run = runParameters.create populated.ParamMap
    Assert.Equal(Some ids.[0], run.GetSetupSortableTestsId())
    Assert.Equal(Some ids.[accessors.Length - 1], run.GetSetupSorterPoolSetHistoryId())
