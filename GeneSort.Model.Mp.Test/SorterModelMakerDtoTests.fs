namespace GeneSort.Model.Mp.Sorter.Tests

open System
open Xunit
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Model.Sorting



type SorterModelSetMakerDtoTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper function to perform round-trip serialization
    let roundTrip (sorterModelMaker: sorterModelMaker) : sorterModelMaker =
        let dto = SorterModelMakerDto.toSorterModelMakerDto sorterModelMaker
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserializedDto = MessagePackSerializer.Deserialize<SorterModelMakerDto>(bytes, options)
        SorterModelMakerDto.fromSorterModelMakerDto deserializedDto

    [<Fact>]
    let ``MsceRandGen round-trip serialization and deserialization should succeed`` () =
        let excludeSelfCe = true
        let msceRandGen = msceRandGen.create rngType.Lcg (UMX.tag<sortingWidth> 16) excludeSelfCe (UMX.tag<ceLength> 10)
        let sorterModelMaker = sorterModelMaker.SmmMsceRandGen msceRandGen
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsceRandGen resultMsceRandGen ->
            Assert.Equal(msceRandGen.Id, resultMsceRandGen.Id)
            Assert.Equal(msceRandGen.RngType, resultMsceRandGen.RngType)
            Assert.Equal(msceRandGen.SortingWidth, resultMsceRandGen.SortingWidth)
            Assert.Equal(msceRandGen.CeLength, resultMsceRandGen.CeLength)
        | _ -> Assert.True(false, "Expected MsceRandGen case")

    [<Fact>]
    let ``MsceRandMutate round-trip serialization and deserialization should succeed`` () =
        let excludeSelfCe = true
        let msce = msce.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> 16) [|16|]
        let arrayRates = indelRatesArray.create [| indelRates.create (1.0, 0.0, 0.0) |] // Always Mutation
        let msceRandMutate = msceRandMutate.create rngType.Lcg arrayRates excludeSelfCe msce
        let sorterModelMaker = sorterModelMaker.SmmMsceRandMutate msceRandMutate
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsceRandMutate resultMsceRandMutate ->
            Assert.Equal(msceRandMutate.Id, resultMsceRandMutate.Id)
            Assert.Equal(msceRandMutate.RngType, resultMsceRandMutate.RngType)
            Assert.Equal(msceRandMutate.Msce, resultMsceRandMutate.Msce)
        | _ -> Assert.True(false, "Expected MsceRandMutate case")

    [<Fact>]
    let ``MssiRandGen round-trip serialization and deserialization should succeed`` () =
        let mssiRandGen = mssiRandGen.create rngType.Lcg (UMX.tag<sortingWidth> 2) (UMX.tag<stageLength> 5)
        let sorterModelMaker = sorterModelMaker.SmmMssiRandGen mssiRandGen
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMssiRandGen resultMssiRandGen ->
            Assert.Equal(mssiRandGen.Id, resultMssiRandGen.Id)
            Assert.Equal(mssiRandGen.RngType, resultMssiRandGen.RngType)
            Assert.Equal(mssiRandGen.SortingWidth, resultMssiRandGen.SortingWidth)
        | _ -> Assert.True(false, "Expected MssiRandGen case")

    [<Fact>]
    let ``MssiRandMutate round-trip serialization and deserialization should succeed`` () =
        let mssi = mssi.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> 2) [| Perm_Si.create [|0; 1|]|]
        let siMutationRates = opActionRates.create (0.0, 0.0)
        let opActionRatesArray = opActionRatesArray.create [|siMutationRates|]
        let mssiRandMutate = mssiRandMutate.create rngType.Lcg mssi opActionRatesArray
        let sorterModelMaker = sorterModelMaker.SmmMssiRandMutate mssiRandMutate 
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMssiRandMutate resultMssiRandMutate ->
            Assert.Equal(mssiRandMutate.Id, resultMssiRandMutate.Id)
            Assert.Equal(mssiRandMutate.RngType, resultMssiRandMutate.RngType)
            Assert.Equal(mssiRandMutate.Mssi, resultMssiRandMutate.Mssi)
        | _ -> Assert.True(false, "Expected MssiRandMutate case")

    [<Fact>]
    let ``MsrsRandGen round-trip serialization and deserialization should succeed`` () =
        let opsGenRates = opsGenRates.create (0.0, 1.0, 0.0)
        let opsGenRatesArray = opsGenRatesArray.create [|opsGenRates|]
        let msrsRandGen = msrsRandGen.create rngType.Lcg (UMX.tag<sortingWidth> 2) opsGenRatesArray
        let sorterModelMaker = sorterModelMaker.SmmMsrsRandGen msrsRandGen
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsrsRandGen resultMsrsRandGen ->
            Assert.Equal(msrsRandGen.Id, resultMsrsRandGen.Id)
            Assert.Equal(msrsRandGen.RngType, resultMsrsRandGen.RngType)
            Assert.Equal(msrsRandGen.SortingWidth, resultMsrsRandGen.SortingWidth)
        | _ -> Assert.True(false, "Expected MsrsRandGen case")


    [<Fact>]
    let ``MsrsRandMutate round-trip serialization and deserialization should succeed`` () =
        let order = 8
        let msrs = msrs.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> order) [| Perm_Rs.create [|0; 1; 2; 3; 4; 5; 6; 7|] |]
        let opsActionRates = opsActionRates.create (1.0, 0.0, 0.0)
        let opsActionRatesArray = opsActionRatesArray.create [|opsActionRates|]
        let msrsRandMutate = msrsRandMutate.create rngType.Lcg msrs opsActionRatesArray
        let sorterModelMaker = sorterModelMaker.SmmMsrsRandMutate msrsRandMutate
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsrsRandMutate resultMsrsRandMutate ->
            Assert.Equal(msrsRandMutate.Id, resultMsrsRandMutate.Id)
            Assert.Equal(msrsRandMutate.RngType, resultMsrsRandMutate.RngType)
            Assert.Equal(msrsRandMutate.Msrs, resultMsrsRandMutate.Msrs)
        | _ -> Assert.True(false, "Expected MsrsRandMutate case")



    [<Fact>]
    let ``Msuf4RandGen round-trip serialization and deserialization should succeed`` () =
        let order = 8
        let genRates = uf4GenRatesArray.create [|Uf4GenRates.makeUniform order|]
        let msuf4RandGen = msuf4RandGen.create rngType.Lcg (UMX.tag<sortingWidth> order) (UMX.tag<stageLength> 1) genRates
        let sorterModelMaker = sorterModelMaker.SmmMsuf4RandGen msuf4RandGen
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsuf4RandGen resultMsuf4RandGen ->
            Assert.Equal(msuf4RandGen.Id, resultMsuf4RandGen.Id)
            Assert.Equal(msuf4RandGen.RngType, resultMsuf4RandGen.RngType)
            Assert.Equal(msuf4RandGen.SortingWidth, resultMsuf4RandGen.SortingWidth)
            Assert.Equal(msuf4RandGen.StageLength, resultMsuf4RandGen.StageLength)
            Assert.Equal(msuf4RandGen.GenRates, resultMsuf4RandGen.GenRates)
        | _ -> Assert.True(false, "Expected Msuf4RandGen case")

    [<Fact>]
    let ``Msuf4RandMutate round-trip serialization and deserialization should succeed`` () =
        let order = 8
        let twoOrbitUfStep = TwoOrbitUfStep.create [|TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho|] order
        let twoOrbitUf4s = [| TwoOrbitUf4.create (TwoOrbitPairType.Ortho) [| twoOrbitUfStep |] |]
        let msuf4 = msuf4.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> order) twoOrbitUf4s
        let mutationRates = uf4MutationRatesArray.create [| Uf4MutationRates.makeUniform order 0.1 0.05 |]
        let msuf4RandMutate = msuf4RandMutate.create rngType.Lcg msuf4 mutationRates
        let sorterModelMaker = sorterModelMaker.SmmMsuf4RandMutate msuf4RandMutate
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsuf4RandMutate resultMsuf4RandMutate ->
            Assert.Equal(msuf4RandMutate.Id, resultMsuf4RandMutate.Id)
            Assert.Equal(msuf4RandMutate.RngType, resultMsuf4RandMutate.RngType)
            Assert.Equal(msuf4RandMutate.Msuf4, resultMsuf4RandMutate.Msuf4)
            Assert.Equal(msuf4RandMutate.Uf4MutationRatesArray, resultMsuf4RandMutate.Uf4MutationRatesArray)
        | _ -> Assert.True(false, "Expected Msuf4RandMutate case")

    [<Fact>]
    let ``Msuf6RandGen round-trip serialization and deserialization should succeed`` () =
        let order = 12
        let genRates = uf6GenRatesArray.create  [|Uf6GenRates.makeUniform order|]
        let msuf6RandGen = msuf6RandGen.create rngType.Lcg (UMX.tag<sortingWidth> order) (UMX.tag<stageLength> 1) genRates
        let sorterModelMaker = sorterModelMaker.SmmMsuf6RandGen msuf6RandGen
        let result = roundTrip sorterModelMaker
        match result with
        | sorterModelMaker.SmmMsuf6RandGen resultMsuf6RandGen ->
            Assert.Equal(msuf6RandGen.Id, resultMsuf6RandGen.Id)
            Assert.Equal(msuf6RandGen.RngType, resultMsuf6RandGen.RngType)
            Assert.Equal(msuf6RandGen.SortingWidth, resultMsuf6RandGen.SortingWidth)
            Assert.Equal(msuf6RandGen.StageLength, resultMsuf6RandGen.StageLength)
            Assert.Equal(msuf6RandGen.GenRates, resultMsuf6RandGen.GenRates)
        | _ -> Assert.True(false, "Expected Msuf6RandGen case")

    [<Fact>]
    let ``Msuf6RandMutate round-trip serialization and deserialization should succeed`` () =
        let order = 12
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let twoOrbitUfSteps = TwoOrbitUfStep.create [|TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; TwoOrbitPairType.Ortho; |] order
        let twoOrbitUf6s = [| TwoOrbitUf6.create TwoOrbitTripleType.Ortho1 [|twoOrbitUfSteps|] |]
        let msuf6 = msuf6.create id (UMX.tag<sortingWidth> order) twoOrbitUf6s
        let mutationRates = uf6MutationRatesArray.create [| Uf6MutationRates.makeUniform order 0.1 0.05 |]
        let msuf6RandMutate = msuf6RandMutate.create rngType.Lcg msuf6 mutationRates
        let sorterModelMaker = sorterModelMaker.SmmMsuf6RandMutate msuf6RandMutate
        let result = roundTrip sorterModelMaker 
        match result with
        | sorterModelMaker.SmmMsuf6RandMutate resultMsuf6RandMutate ->
            Assert.Equal(msuf6RandMutate.Id, resultMsuf6RandMutate.Id)
            Assert.Equal(msuf6RandMutate.RngType, resultMsuf6RandMutate.RngType)
            Assert.Equal(msuf6RandMutate.Msuf6, resultMsuf6RandMutate.Msuf6)
            Assert.Equal(msuf6RandMutate.Uf6MutationRatesArray, resultMsuf6RandMutate.Uf6MutationRatesArray)
        | _ -> Assert.True(false, "Expected Msuf6RandMutate case")



