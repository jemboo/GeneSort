namespace GeneSort.Model.Mp.Sorter.Tests

open System
open Xunit
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorter
open GeneSort.Model.Mp.Sorter
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Model.Sorter.Ce

type SorterModelSetDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper function to perform round-trip serialization
    let roundTrip (sorterModelSet: sorterModelSet) : sorterModelSet =
        let dto = SorterModelSetDto.fromDomain sorterModelSet
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserializedDto = MessagePackSerializer.Deserialize<SorterModelSetDto>(bytes, options)
        SorterModelSetDto.toDomain deserializedDto

    [<Fact>]
    let ``SorterModelSetDto with Msce round-trip serialization and deserialization should succeed`` () =
        let msce = Msce.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> 16) [|1;2;3|]
        let sorterModel = SorterModel.Msce msce
        let sorterModelSet = sorterModelSet.create (Guid.NewGuid() |> UMX.tag<sorterModelSetID>) (msce.CeLength) [| sorterModel |]
        let result = roundTrip sorterModelSet
        Assert.Equal(sorterModelSet.Id, result.Id)
        Assert.Equal(sorterModelSet.SorterModels.Length, result.SorterModels.Length)
        match result.SorterModels.[0] with
        | SorterModel.Msce resultMsce ->
            Assert.Equal(msce.Id, resultMsce.Id)
            Assert.Equal(msce.SortingWidth, resultMsce.SortingWidth)
            Assert.Equal<int>(msce.CeCodes, resultMsce.CeCodes)
        | _ -> Assert.True(false, "Expected Msce case")
