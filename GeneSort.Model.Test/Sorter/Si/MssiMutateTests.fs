namespace GeneSort.Model.Test.Sorter.Si

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Si

type MssiRandMutateTests() =
    [<Theory>]
    [<InlineData(0.0, 0.0)>]
    [<InlineData(1.0, 0.0)>]
    [<InlineData(0.0, 1.0)>]
    [<InlineData(0.5, 0.5)>]
    member _.``Mutation preserves self inversion and replays deterministically`` ortho para =
        let parent = mssi.create (UMX.tag (Guid.Parse "00000000-0000-0000-0000-000000000001")) 4<sortingWidth>
                        [|permSi.create [|1; 0; 3; 2|]; permSi.create [|3; 2; 1; 0|]|]
        let mutator = mssiRandMutate.create rngFactory.LcgFactory (opActionRates.create (ortho, para)) mutatorVariant.V1
        let result = mutator.MakeSorterModelFromIndexAndMod parent (UMX.tag 0) (UMX.tag 0)
        Assert.Equal(parent.SortingWidth, result.SortingWidth)
        Assert.Equal(parent.Perm_Sis.Length, result.Perm_Sis.Length)
        Assert.All(result.Perm_Sis, fun perm -> Assert.True(Permutation.isSelfInverse perm.Permutation))
        Assert.Equal(result, mutator.MakeSorterModelFromIndexAndMod parent (UMX.tag 0) (UMX.tag 0))
        Assert.NotEqual(parent.Id, result.Id)
        Assert.NotEqual(result.Id, (mutator.MakeSorterModelFromIndexAndMod parent (UMX.tag 1) (UMX.tag 0)).Id)
        if ortho + para = 0.0 then Assert.Equal<permSi[]>(parent.Perm_Sis, result.Perm_Sis)
        else Assert.NotEqual<permSi[]>(parent.Perm_Sis, result.Perm_Sis)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(3)>]
    member _.``Uniform mutator accepts parents with different stage counts`` count =
        let mutator = mssiRandMutate.create rngFactory.LcgFactory (opActionRates.create (0.0, 0.0)) mutatorVariant.V1
        let parent = mssi.create (UMX.tag Guid.Empty) 4<sortingWidth> (Array.create count (permSi.create [|1; 0; 3; 2|]))
        let result = mutator.MakeSorterModelFromIndexAndMod parent (UMX.tag 0) (UMX.tag 0)
        Assert.Equal<permSi[]>(parent.Perm_Sis, result.Perm_Sis)
