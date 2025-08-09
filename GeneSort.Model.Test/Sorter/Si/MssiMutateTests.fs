﻿namespace GeneSort.Model.Test.Sorter.Si


open System
open Xunit
open FSharp.UMX
open FsUnit.Xunit
open GeneSort.Sorter
open GeneSort.Core
open GeneSort.Core.Perm_Si
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Si

type MssiRandMutateTests() =

    // Helper to create a Model_Si
    let createModelSi (id: Guid<sorterModelID>) (width: int<sortingWidth>) (permSis: Perm_Si array) : Mssi =
        Mssi.create id width permSis

    let randoGen 
            (nextIndexValues: int list) 
            (pickValues: float list) 
            (rt: rngType) 
            (id: Guid) : IRando =
        MockRando(pickValues, nextIndexValues) :> IRando

    [<Fact>]
    let ``mutate with NoAction mode does not change permutations`` () =
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi = Perm_Si.create [|1; 0; 2; 3|] // (0 1)
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi|]
        let siMutationRates = OpActionRates.create (0.0, 0.0)
        let arrayToMutate = OpActionRatesArray.create [|siMutationRates|]
        let modelSiMutate = MssiRandMutate.create rngType.Lcg modelSi arrayToMutate

        let mock = randoGen [0; 1] [0.5] // Ensures NoAction (within NoActionRate)
        let result = (modelSiMutate :> ISorterModelMaker).MakeSorterModel mock 0 :?> Mssi
        Assert.Equal<int array>(modelSi.Perm_Sis.[0].Array, result.Perm_Sis.[0].Array)
        Assert.Equal(modelSi.SortingWidth, result.SortingWidth)

    [<Fact>]
    let ``mutate with Ortho mode applies Ortho mutation`` () = 
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi = Perm_Si.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi|]
        let siMutationRates = OpActionRates.create (0.9, 0.0)
        let arrayToMutate = OpActionRatesArray.create [|siMutationRates|]
        let modelSiMutate = MssiRandMutate.create rngType.Lcg modelSi arrayToMutate

        let mock = randoGen [0; 2] [0.0] // Picks indices 0, 2 and Ortho mode
        let result = (modelSiMutate :> ISorterModelMaker).MakeSorterModel mock 0 :?> Mssi
        let expectedArray = [|2; 3; 0; 1|] // Expected: (0 2)(1 3)
        Assert.Equal<int array>(expectedArray, result.Perm_Sis.[0].Array)
        Assert.True(Permutation.isSelfInverse result.Perm_Sis.[0].Permutation)
        Assert.Equal(modelSi.SortingWidth, result.SortingWidth)

    [<Fact>]
    let ``mutate with Para mode applies Para mutation`` () =
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi = Perm_Si.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi|]
        let siMutationRates = OpActionRates.create (0.0, 1.0)
        let arrayToMutate = OpActionRatesArray.create [|siMutationRates|]
        let modelSiMutate = MssiRandMutate.create rngType.Lcg modelSi arrayToMutate

        let mock = randoGen [0; 2] [0.5] // Picks indices 0, 2 and Para mode
        let result = (modelSiMutate :> ISorterModelMaker).MakeSorterModel mock 0 :?> Mssi
        let expectedArray = [|3; 2; 1; 0|] // Expected: (0 3)(1 2)
        Assert.Equal<int array>(expectedArray, result.Perm_Sis.[0].Array)
        Assert.True(Permutation.isSelfInverse result.Perm_Sis.[0].Permutation)
        Assert.Equal(modelSi.SortingWidth, result.SortingWidth)

    [<Fact>]
    let ``mutate preserves width and number of permutations`` () =
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi1 = Perm_Si.create [|1; 0; 3; 2|]
        let permSi2 = Perm_Si.create [|3; 2; 1; 0|]
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi1; permSi2|]
        let siMutationRates = OpActionRates.create (0.5, 0.5)
        let arrayToMutate = OpActionRatesArray.create [|siMutationRates; siMutationRates|]
        let modelSiMutate = MssiRandMutate.create rngType.Lcg modelSi arrayToMutate

        let mock = randoGen [0; 2; 0; 2] [0.0; 0.0] // Ortho mode
        let result = (modelSiMutate :> ISorterModelMaker).MakeSorterModel mock 0 :?> Mssi
        Assert.Equal(UMX.tag<sortingWidth> 4, result.SortingWidth)
        Assert.Equal(2, result.Perm_Sis.Length)

    [<Fact>]
    let ``mutate generates new unique ID`` () =
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi = Perm_Si.create [|1; 0; 2; 3|]
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi|]
        let siMutationRates = OpActionRates.create (0.0, 0.0)
        let arrayToMutate = OpActionRatesArray.create [|siMutationRates|]
        let modelSiMutate = MssiRandMutate.create rngType.Lcg modelSi arrayToMutate

        let mock = randoGen [0; 1] [0.5]
        let result = (modelSiMutate :> ISorterModelMaker).MakeSorterModel mock 0 :?> Mssi
        Assert.NotEqual(%id, %result.Id)
        Assert.NotEqual(Guid.Empty, %result.Id)

    [<Fact>]
    let ``mutate applies mutation to all permutations in array`` () =
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi1 = Perm_Si.create [|1; 0; 3; 2|] // (0 1)(2 3)
        let permSi2 = Perm_Si.create [|3; 2; 1; 0|] // (0 3)(1 2)
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi1; permSi2|]
        let siMutationRates = OpActionRates.create (1.0, 0.0)
        let arrayToMutate = OpActionRatesArray.create [|siMutationRates; siMutationRates|]
        let modelSiMutate = MssiRandMutate.create rngType.Lcg modelSi arrayToMutate

        let mock = randoGen [0; 2; 0; 2] [0.0; 0.0] // Ortho mode, indices 0, 2
        let result = (modelSiMutate :> ISorterModelMaker).MakeSorterModel mock 0 :?> Mssi
        let expectedArray1 = [|2; 3; 0; 1|] // (0 2)(1 3)
        let expectedArray2 = [|1; 0; 3; 2|] // (0 1)(2 3)
        Assert.Equal<int array>(expectedArray1, result.Perm_Sis.[0].Array)
        Assert.Equal<int array>(expectedArray2, result.Perm_Sis.[1].Array)
        Assert.True(Permutation.isSelfInverse result.Perm_Sis.[0].Permutation)
        Assert.True(Permutation.isSelfInverse result.Perm_Sis.[1].Permutation)

    [<Fact>]
    let ``create fails when Perm_Sis length does not match arrayRates length`` () =
        let id = UMX.tag<sorterModelID> (Guid.NewGuid())
        let permSi = Perm_Si.create [|1; 0; 2; 3|]
        let modelSi = createModelSi id (UMX.tag<sortingWidth> 4) [|permSi|]
        let siMutationRates = OpActionRates.create (0.0, 0.0)
        let array = OpActionRatesArray.create [|siMutationRates; siMutationRates|] // Length 2
        Assert.Throws<exn>(fun () -> MssiRandMutate.create rngType.Lcg modelSi array |> ignore)
