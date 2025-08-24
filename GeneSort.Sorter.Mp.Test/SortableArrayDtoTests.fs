

namespace GeneSort.Sorter.Mp.Test
open Xunit
open GeneSort.Core
open FSharp.UMX
open System
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Mp.Sortable
open GeneSort.Sorter.Mp.Sorter

type SortableArrayDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    [<Fact>]
    let ``sortableBoolArrayDto round-trip serialization`` () =
        let originalAsBool = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let originalSorableArray = originalAsBool |> SortableArray.Bools
        let dto = SortableArrayDto.toDto originalSorableArray
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<sortableArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDto deserialized
        let restoredBoolArray =
            match restored with
            | SortableArray.Bools b -> b
            | _ -> failwith "Expected Bool array"
        Assert.Equal<bool>(originalAsBool.Values, restoredBoolArray.Values)
        Assert.Equal(originalAsBool.SortingWidth, restoredBoolArray.SortingWidth)

    [<Fact>]
    let ``sortableIntArrayDto round-trip serialization`` () =
        let originalAsInt = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3 |> UMX.tag<symbolSetSize>))
        let original = originalAsInt |> SortableArray.Ints
        let dto = SortableArrayDto.toDto original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<sortableArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDto deserialized
        let restoredIntArray =
            match restored with
            | SortableArray.Ints i -> i
            | _ -> failwith "Expected Int array"
        Assert.Equal<int>(originalAsInt.Values, restoredIntArray.Values)
        Assert.Equal(originalAsInt.SortingWidth, restoredIntArray.SortingWidth)
        Assert.Equal(originalAsInt.SymbolSetSize, restoredIntArray.SymbolSetSize)

    [<Fact>]
    let ``SortableArrayDto Bools round-trip serialization`` () =
        let original = SortableArray.createBools [| true; false; true |] 3<sortingWidth>
        let dto = SortableArrayDto.toDto original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<sortableArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDto deserialized
        Assert.Equal<obj array>(SortableArray.values original, SortableArray.values restored)

    [<Fact>]
    let ``SortableArrayDto Ints round-trip serialization`` () =
        let original = SortableArray.createInts [| 0; 2; 1 |] 3<sortingWidth> (3 |> UMX.tag<symbolSetSize>)
        let dto = SortableArrayDto.toDto original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<sortableArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDto deserialized
        Assert.Equal<obj array>(SortableArray.values original, SortableArray.values restored)

    [<Fact>]
    let ``SortableArrayDto with invalid Type throws`` () =
        let dto = { Type = "Invalid"; IntArray = None; BoolArray = None }
        let ex = Assert.Throws<ArgumentException>(fun () -> SortableArrayDto.fromDto dto |> ignore)
        Assert.Equal("Invalid Type: Invalid. Expected 'Ints' or 'Bools'. (Parameter 'Type')", ex.Message)

    [<Fact>]
    let ``SortableArrayDto with missing IntArray throws`` () =
        let dto = { Type = "Ints"; IntArray = None; BoolArray = None }
        let ex = Assert.Throws<ArgumentException>(fun () -> SortableArrayDto.fromDto dto |> ignore)
        Assert.Equal("IntArray must be present for Type 'Ints'. (Parameter 'IntArray')", ex.Message)

    [<Fact>]
    let ``SortableArrayDto with missing BoolArray throws`` () =
        let dto = { Type = "Bools"; IntArray = None; BoolArray = None }
        let ex = Assert.Throws<ArgumentException>(fun () -> SortableArrayDto.fromDto dto |> ignore)
        Assert.Equal("BoolArray must be present for Type 'Bools'. (Parameter 'BoolArray')", ex.Message)

