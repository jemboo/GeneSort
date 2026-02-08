namespace GeneSort.Model.Mp.Sorter.Tests

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Sorting

type SorterModelSetDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper function to perform round-trip serialization
    let roundTrip (sorterModelSet: sortingModelSet) : sortingModelSet =
        let dto = SortingModelSetDto.fromDomain sorterModelSet
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserializedDto = MessagePackSerializer.Deserialize<sortingModelSetDto>(bytes, options)
        SortingModelSetDto.toDomain deserializedDto

    //[<Fact>]
    //let ``SorterModelSetDto with Msce round-trip serialization and deserialization should succeed`` () =
    //    let msce = Msce.create (Guid.NewGuid() |> UMX.tag<sortingModelID>) (UMX.tag<sortingWidth> 16) [|1;2;3|]
    //    let sorterModel = sorterModel.Msce msce
    //    let sorterModelSet = sortingModelSet.create (Guid.NewGuid() |> UMX.tag<sortingModelSetID>) (msce.CeLength) [| sorterModel |]
    //    let result = roundTrip sorterModelSet
    //    Assert.Equal(sorterModelSet.Id, result.Id)
    //    Assert.Equal(sorterModelSet.SorterModels.Length, result.SorterModels.Length)
    //    match result.SorterModels.[0] with
    //    | sorterModel.Msce resultMsce ->
    //        Assert.Equal(msce.Id, resultMsce.Id)
    //        Assert.Equal(msce.SortingWidth, resultMsce.SortingWidth)
    //        Assert.Equal<int>(msce.CeCodes, resultMsce.CeCodes)
    //    | _ -> Assert.True(false, "Expected Msce case")
