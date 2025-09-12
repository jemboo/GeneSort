﻿namespace GeneSort.Model.Mp.Sorter.Tests

open System
open Xunit
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core
open GeneSort.Model.Sorter
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.Sorter.Uf6
open GeneSort.Model.Sorter.Uf6
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Ce

type SorterModelDtoTests() =


    // Mock floatPicker for deterministic testing
    let mockFloatPicker () = 0.5

    // Helper function to create a TwoOrbitUf4GenRates for a given order
    let createTestGenRates (order: int) =
        Uf4GenRates.makeUniform order

    // Helper function to create a valid TwoOrbitUnfolder4 for a given order, SeedType, and optional TwoOrbitType override
    let createTestTwoOrbitUnfolder4 
            (order: int) 
            (twoOrbitPairType: TwoOrbitPairType) 
            (twoOrbitPairTypeOverride: TwoOrbitPairType option) 
     : TwoOrbitUf4  =
        let baseGenRates = createTestGenRates order
        let orthoRate = if twoOrbitPairType = TwoOrbitPairType.Ortho then 1.0 else 0.0
        let paraRate = if twoOrbitPairType = TwoOrbitPairType.Para then 1.0 else 0.0
        let selfSyymRate = if twoOrbitPairType = TwoOrbitPairType.SelfRefl then 1.0 else 0.0

        let ratesArray = 
                match twoOrbitPairTypeOverride with
                | Some tot -> 
                    let orthoRate = if twoOrbitPairType = TwoOrbitPairType.Ortho then 1.0 else 0.0
                    let paraRate = if twoOrbitPairType = TwoOrbitPairType.Para then 1.0 else 0.0
                    let selfSyymRate = if twoOrbitPairType = TwoOrbitPairType.SelfRefl then 1.0 else 0.0
                    Array.init baseGenRates.OpsGenRatesArray.RatesArray.Length (
                        fun _ -> OpsGenRates.create(orthoRate, paraRate, selfSyymRate))
                | None -> baseGenRates.OpsGenRatesArray.RatesArray

        let genRates : uf4GenRates = 
                    uf4GenRates.create 
                        order
                        (OpsGenRates.create(orthoRate, paraRate, selfSyymRate))
                        (OpsGenRatesArray.create ratesArray)

        RandomUnfolderOps4.makeRandomTwoOrbitUf4 mockFloatPicker genRates



    // Helper function to create a valid TwoOrbitUnfolder4 for a given order, SeedType, and optional TwoOrbitType override
    let createTestTwoOrbitUnfolder6
            (order: int) 
            (twoOrbitPairType: TwoOrbitPairType) 
            (twoOrbitPairTypeOverride: TwoOrbitPairType option) 
     : TwoOrbitUf6  =
        let genRates : uf6GenRates = 
            Uf6GenRates.makeUniform order

        RandomUnfolderOps6.makeRandomTwoOrbitUf6 mockFloatPicker genRates




    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper function to perform round-trip serialization
    let roundTrip (sorterModel: SorterModel) : SorterModel =
        let dto = SorterModelDto.toSorterModelDto sorterModel
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserializedDto = MessagePackSerializer.Deserialize<SorterModelDto>(bytes, options)
        SorterModelDto.fromSorterModelDto deserializedDto

    [<Fact>]
    let ``Msce round-trip serialization and deserialization should succeed`` () =
        let msce = Msce.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> 16) [|1;2;3|]
        let sorterModel = SorterModel.Msce msce
        let result = roundTrip sorterModel
        match result with
        | SorterModel.Msce resultMsce ->
            Assert.Equal(msce.Id, resultMsce.Id)
            Assert.Equal(msce.SortingWidth, resultMsce.SortingWidth)
            Assert.Equal<int array>(msce.CeCodes, resultMsce.CeCodes)
        | _ -> Assert.True(false, "Expected Msce case")

    [<Fact>]
    let ``Mssi round-trip serialization and deserialization should succeed`` () =
        let permSi = Perm_Si.create [|1; 0; 2; 3|] // (0 1)
        let mssi = Mssi.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> 16) [|permSi|]
        let sorterModel = SorterModel.Mssi mssi
        let result = roundTrip sorterModel
        match result with
        | SorterModel.Mssi resultMssi ->
            Assert.Equal(mssi.Id, resultMssi.Id)
            Assert.Equal(mssi.SortingWidth, resultMssi.SortingWidth)
            Assert.Equal<Perm_Si>(mssi.Perm_Sis, resultMssi.Perm_Sis)
        | _ -> Assert.True(false, "Expected Mssi case")

    [<Fact>]
    let ``Msrs round-trip serialization and deserialization should succeed`` () =
        let permRss = [| Perm_Rs.create([| 3; 2; 1; 0 |]); Perm_Rs.create([| 1; 0; 3; 2 |]) |]
        let msrs = Msrs.create (Guid.NewGuid() |> UMX.tag<sorterModelID>) (UMX.tag<sortingWidth> 4) permRss
        let sorterModel = SorterModel.Msrs msrs
        let result = roundTrip sorterModel
        match result with
        | SorterModel.Msrs resultMsrs ->
            Assert.Equal(msrs.Id, resultMsrs.Id)
            Assert.Equal(msrs.SortingWidth, resultMsrs.SortingWidth)
            Assert.Equal<Perm_Rs>(msrs.Perm_Rss, resultMsrs.Perm_Rss)
        | _ -> Assert.True(false, "Expected Msrs case")

    [<Fact>]
    let ``Msuf4 round-trip serialization and deserialization should succeed`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let width = 16<sortingWidth>
        let tou = createTestTwoOrbitUnfolder4 16 TwoOrbitPairType.Ortho (Some TwoOrbitPairType.Para)
        let msuf4 = Msuf4.create id width [|tou|] 
        let sorterModel = SorterModel.Msuf4 msuf4
        let result = roundTrip sorterModel
        match result with
        | SorterModel.Msuf4 resultMsuf4 ->
            Assert.Equal(msuf4.Id, resultMsuf4.Id)
            Assert.Equal(msuf4.SortingWidth, resultMsuf4.SortingWidth)
            Assert.Equal<TwoOrbitUf4>(msuf4.TwoOrbitUnfolder4s, resultMsuf4.TwoOrbitUnfolder4s)
        | _ -> Assert.True(false, "Expected Msuf4 case")


    [<Fact>]
    let ``Msuf6 round-trip serialization and deserialization should succeed`` () =
        let id = Guid.NewGuid() |> UMX.tag<sorterModelID>
        let order = 12
        let width = 12<sortingWidth>
        let tou = createTestTwoOrbitUnfolder6 order TwoOrbitPairType.Ortho (Some TwoOrbitPairType.Para)

        let msuf6 = Msuf6.create id width [|tou|]
        let sorterModel = SorterModel.Msuf6 msuf6
        let result = roundTrip sorterModel
        match result with
        | SorterModel.Msuf6 resultMsuf6 ->
            Assert.Equal(msuf6.Id, resultMsuf6.Id)
            Assert.Equal(msuf6.SortingWidth, resultMsuf6.SortingWidth)
            Assert.Equal<TwoOrbitUf6 array>(msuf6.TwoOrbitUnfolder6s, resultMsuf6.TwoOrbitUnfolder6s)
        | _ -> Assert.True(false, "Expected Msuf6 case")


    [<Fact>]
    let ``Deserialization with invalid Msuf6Dto should throw exception`` () =
        let invalidMsuf6Dto = { Msuf6Dto.Id = Guid.NewGuid(); SortingWidth = 5; TwoOrbitUnfolder6s = [||] } // Invalid: SortingWidth not divisible by 6
        let dto = SorterModelDto.Msuf6 invalidMsuf6Dto
        let bytes = MessagePackSerializer.Serialize(dto, options)
        let deserializedDto = MessagePackSerializer.Deserialize<SorterModelDto>(bytes, options)
        Assert.ThrowsAny<Exception>(fun () -> SorterModelDto.fromSorterModelDto deserializedDto |> ignore)