namespace GeneSort.Project.Test

open System
open FSharp.UMX
open Xunit
open MessagePack.Resolvers
open MessagePack
open MessagePack.FSharp
open System.IO
open GeneSort.Project
open System.Threading
open System.Collections.Generic


type MapStringTransformerTests() =

    [<Fact>]
    let ``ToStringObjMap applies upcast transformation correctly`` () =
        let transformer = MapStringTransformer [
            ("key1", fun s -> [| ("out1", s :> obj) |])
            ("key2", fun s -> [| ("out2", s :> obj) |])
        ]
        let input = Map.ofList [ ("key1", "value1"); ("key2", "value2") ]
        let result = MapStringTransformer.ToStringObjMap input transformer
        Assert.Equal(2, result.Count)
        Assert.Equal("value1" :> obj, result.["out1"])
        Assert.Equal("value2" :> obj, result.["out2"])
        Assert.IsType<string>(result.["out1"]) |> ignore
        Assert.IsType<string>(result.["out2"])

    [<Fact>]
    let ``ToStringObjMap applies mixed transformations with multiple outputs`` () =
        let transformer = MapStringTransformer [
            ("key1", fun s -> [| ("out1", [| s |] :> obj); ("out2", s.Length :> obj) |])
            ("key2", fun s -> [| ("out3", s.ToUpper() :> obj) |])
        ]
        let input = Map.ofList [ ("key1", "value1"); ("key2", "value2") ]
        let result = MapStringTransformer.ToStringObjMap input transformer
        Assert.Equal(3, result.Count)
        Assert.Equal([| "value1" |] :> obj, result.["out1"])
        Assert.Equal(6 :> obj, result.["out2"]) // "value1" has length 6
        Assert.Equal("VALUE2" :> obj, result.["out3"])
        Assert.IsType<string[]>(result.["out1"]) |> ignore
        Assert.IsType<int>(result.["out2"]) |> ignore
        Assert.IsType<string>(result.["out3"])

    [<Fact>]
    let ``ToStringObjMap throws KeyNotFoundException for unmapped key`` () =
        let transformer = MapStringTransformer [
            ("key1", fun s -> [| ("out1", s :> obj) |])
        ]
        let input = Map.ofList [ ("key1", "value1"); ("key2", "value2") ]
        let ex = Assert.Throws<KeyNotFoundException>(fun () -> MapStringTransformer.ToStringObjMap input transformer |> ignore)
        Assert.Equal("No transformation function found for key 'key2'", ex.Message)

    [<Fact>]
    let ``ToStringObjMap handles empty input map correctly`` () =
        let transformer = MapStringTransformer [
            ("key1", fun s -> [| ("out1", s :> obj) |])
        ]
        let input = Map.empty<string, string>
        let result = MapStringTransformer.ToStringObjMap input transformer
        Assert.Empty(result)

    [<Fact>]
    let ``ToStringObjMap handles single key-value pair with multiple outputs`` () =
        let transformer = MapStringTransformer [
            ("key", fun s -> [| ("out1", s :> obj); ("out2", s.ToUpper() :> obj) |])
        ]
        let input = Map.ofList [ ("key", "value") ]
        let result = MapStringTransformer.ToStringObjMap input transformer
        Assert.Equal(2, result.Count)
        Assert.Equal("value" :> obj, result.["out1"])
        Assert.Equal("VALUE" :> obj, result.["out2"])
        Assert.IsType<string>(result.["out1"]) |> ignore
        Assert.IsType<string>(result.["out2"])
