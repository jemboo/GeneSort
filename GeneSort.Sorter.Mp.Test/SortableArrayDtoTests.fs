

namespace GeneSort.Sorter.Mp.Test
open Xunit
open GeneSort.Core
open FSharp.UMX
open System
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Sorter
open GeneSort.Sorter.Mp

type SortableArrayDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    [<Fact>]
    let ``sortableBoolArrayDto round-trip serialization`` () =
        let original = sortableBoolArray.Create([| true; false; true |], 3<sortingWidth>)
        let dto = SortableArrayDto.toDtoBoolArray original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<sortableBoolArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDtoBoolArray deserialized
        Assert.Equal<bool>(original.Values, restored.Values)
        Assert.Equal(original.SortingWidth, restored.SortingWidth)

    [<Fact>]
    let ``sortableIntArrayDto round-trip serialization`` () =
        let original = sortableIntArray.Create([| 0; 2; 1 |], 3<sortingWidth>, (3UL |> UMX.tag<symbolSetSize>))
        let dto = SortableArrayDto.toDtoIntArray original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<sortableIntArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDtoIntArray deserialized
        Assert.Equal<int>(original.Values, restored.Values)
        Assert.Equal(original.SortingWidth, restored.SortingWidth)
        Assert.Equal(original.SymbolSetSize, restored.SymbolSetSize)

    [<Fact>]
    let ``SortableArrayDto Bools round-trip serialization`` () =
        let original = SortableArray.createBools [| true; false; true |] 3<sortingWidth>
        let dto = SortableArrayDto.toDto original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<SortableArrayDto>(bytes, options)
        let restored = SortableArrayDto.fromDto deserialized
        Assert.Equal<obj array>(SortableArray.values original, SortableArray.values restored)

    [<Fact>]
    let ``SortableArrayDto Ints round-trip serialization`` () =
        let original = SortableArray.createInts [| 0; 2; 1 |] 3<sortingWidth> (3UL |> UMX.tag<symbolSetSize>)
        let dto = SortableArrayDto.toDto original
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserialized = MessagePackSerializer.Deserialize<SortableArrayDto>(bytes, options)
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

    [<Fact>]
    let ``sortableIntArrayDto with invalid SymbolSetSize throws`` () =
        let dto = { Values = [| 0; 1 |]; SortingWidth = 2; SymbolSetSize = 0UL }
        let ex = Assert.Throws<ArgumentException>(fun () -> SortableArrayDto.fromDtoIntArray dto |> ignore)
        Assert.Contains("Symbol set size must be positive", ex.Message)
