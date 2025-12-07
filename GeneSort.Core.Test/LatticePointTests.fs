namespace GeneSort.Core.Test
open System
open Xunit
open FsUnit.Xunit
open GeneSort.Core.Permutation
open GeneSort.Core
open GeneSort.Core.OrbitSet
open GeneSort.Core.LatticePoint

type LatticePointTests() =

    // Utility to create points more concisely
    let lp xs = latticePoint.create(xs)

    [<Fact>]
    let ``getOverIndex returns correct index when exactly one coordinate increases by 1`` () =
        let a = lp [|0; 2; 5|]
        let b = lp [|0; 3; 5|]

        let idx = getOverIndex a b

        Assert.Equal(1, idx)

    [<Fact>]
    let ``getOverIndex throws when more than one coordinate increases`` () =
        let a = lp [|1; 1; 1|]
        let b = lp [|2; 2; 1|]   // two +1 changes → invalid

        let ex =
            Assert.Throws<System.ArgumentException>(fun () ->
                getOverIndex a b |> ignore)

        Assert.Contains("More than one coordinate", ex.Message)

    [<Fact>]
    let ``getOverIndex throws when coordinates differ in invalid way`` () =
        let a = lp [|1; 1; 1|]
        let b = lp [|1; 4; 1|]   // difference is +3 → invalid

        let ex =
            Assert.Throws<System.ArgumentException>(fun () ->
                getOverIndex a b |> ignore)

        Assert.Contains("not equal to +1", ex.Message)

    [<Fact>]
    let ``getOverIndex throws when no coordinate increases`` () =
        let a = lp [|1; 2; 3|]
        let b = lp [|1; 2; 3|]  // identical → invalid

        let ex =
            Assert.Throws<System.ArgumentException>(fun () ->
                getOverIndex a b |> ignore)

        Assert.Contains("No coordinate", ex.Message)

    [<Fact>]
    let ``getOverIndex throws when dimensions differ`` () =
        let a = lp [|1; 2|]
        let b = lp [|1; 2; 3|]

        let ex =
            Assert.Throws<System.ArgumentException>(fun () ->
                getOverIndex a b |> ignore)

        Assert.Contains("same dimension", ex.Message)

    //----------------------------------------------------------------------
    // getPermutationIndex tests
    //----------------------------------------------------------------------

    //[<Fact>]
    //let ``getPermutationIndex equals sum(lowPoint) when single +1 change`` () =
    //    let a = lp [|1; 0; 2|]
    //    let b = lp [|1; 1; 2|]   // +1 at index 1

    //    let result = getPermutationIndex a b

    //    // sum(a) = 1 + 0 + 2 = 3
    //    Assert.Equal(3, result)


    //[<Fact>]
    //let ``getPermutationIndex propagates getOverIndex exceptions`` () =
    //    let a = lp [|1; 1; 1|]
    //    let b = lp [|3; 1; 1|]    // illegal change

    //    let ex =
    //        Assert.Throws<System.ArgumentException>(fun () ->
    //            getPermutationIndex a b |> ignore)

    //    Assert.Contains("not equal to +1", ex.Message)