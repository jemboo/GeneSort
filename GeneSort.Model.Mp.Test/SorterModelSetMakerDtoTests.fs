namespace GeneSort.Model.Mp.Sorter.Tests

open Xunit
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Mp.Sorter



type SorterModelMakerDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    //// Helper function to perform round-trip serialization
    //let roundTrip (sorterModelSetMaker: sortingModelSetMaker) : sortingModelSetMaker =
    //    let dto = SorterModelSetMakerDto.fromDomain sorterModelSetMaker
    //    let bytes = MessagePackSerializer.Serialize(dto, options)
    //    let deserializedDto = MessagePackSerializer.Deserialize<sorterModelSetMakerDto>(bytes, options)
    //    SorterModelSetMakerDto.toDomain deserializedDto

    //[<Fact>]
    //let ``SorterModelSetMakerDto with MsceRandGen round-trip serialization and deserialization should succeed`` () =
    //    let excludeSelfCe = true 
    //    let msceRandGen = msceRandGen.create rngType.Lcg (UMX.tag<sortingWidth> 16) excludeSelfCe (UMX.tag<ceLength> 10)
    //    let sorterModelMaker = sorterModelMaker.SmmMsceRandGen msceRandGen
    //    let sorterModelSetMaker = sortingModelSetMaker.create sorterModelMaker 0<sorterCount> 5<sorterCount>
    //    let result = roundTrip sorterModelSetMaker
    //    Assert.Equal(sorterModelSetMaker.Id, result.Id)
    //    Assert.Equal(sorterModelSetMaker.FirstIndex, result.FirstIndex)
    //    Assert.Equal(sorterModelSetMaker.Count, result.Count)
    //    match result.SortingModelMaker with
    //    | sorterModelMaker.SmmMsceRandGen resultMsceRandGen ->
    //        Assert.Equal(msceRandGen.Id, resultMsceRandGen.Id)
    //        Assert.Equal(msceRandGen.RngType, resultMsceRandGen.RngType)
    //        Assert.Equal(msceRandGen.SortingWidth, resultMsceRandGen.SortingWidth)
    //        Assert.Equal(msceRandGen.CeLength, resultMsceRandGen.CeLength)
    //    | _ -> Assert.True(false, "Expected MsceRandGen case")

