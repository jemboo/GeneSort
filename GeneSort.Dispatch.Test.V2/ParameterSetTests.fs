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
