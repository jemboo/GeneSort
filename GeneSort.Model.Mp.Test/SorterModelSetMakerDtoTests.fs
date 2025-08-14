namespace GeneSort.Model.Mp.Sorter.Tests


open System
open Xunit
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Ce

open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.Sorter.Ce
open GeneSort.Model.Mp.Sorter.Si
open GeneSort.Model.Mp.Sorter.Rs
open GeneSort.Model.Mp.Sorter.Uf4
open GeneSort.Model.Mp.Sorter.Uf6

open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.Sorter.Uf6
open GeneSort.Model.Sorter.Uf6



type SorterModelMakerDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper function to perform round-trip serialization
    let roundTrip (sorterModelSetMaker: SorterModelSetMaker) : SorterModelSetMaker =
        let dto = SorterModelSetMakerDto.fromDomain sorterModelSetMaker
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserializedDto = MessagePackSerializer.Deserialize<SorterModelSetMakerDto>(bytes, options)
        SorterModelSetMakerDto.toDomain deserializedDto

    [<Fact>]
    let ``SorterModelSetMakerDto with MsceRandGen round-trip serialization and deserialization should succeed`` () =
        let excludeSelfCe = true
        let msceRandGen = MsceRandGen.create rngType.Lcg (UMX.tag<sortingWidth> 16) excludeSelfCe (UMX.tag<ceCount> 10)
        let sorterModelMaker = SorterModelMaker.MsceRandGen msceRandGen
        let sorterModelSetMaker = SorterModelSetMaker.create sorterModelMaker 0 5
        let result = roundTrip sorterModelSetMaker
        Assert.Equal(sorterModelSetMaker.Id, result.Id)
        Assert.Equal(sorterModelSetMaker.FirstIndex, result.FirstIndex)
        Assert.Equal(sorterModelSetMaker.Count, result.Count)
        match result.SorterModelMaker with
        | SorterModelMaker.MsceRandGen resultMsceRandGen ->
            Assert.Equal(msceRandGen.Id, resultMsceRandGen.Id)
            Assert.Equal(msceRandGen.RngType, resultMsceRandGen.RngType)
            Assert.Equal(msceRandGen.SortingWidth, resultMsceRandGen.SortingWidth)
            Assert.Equal(msceRandGen.CeCount, resultMsceRandGen.CeCount)
        | _ -> Assert.True(false, "Expected MsceRandGen case")

