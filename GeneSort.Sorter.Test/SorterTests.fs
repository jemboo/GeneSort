namespace GeneSort.Sorter.Tests

open System
open Xunit
open FSharp.UMX
open FsUnit.Xunit
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable


type SorterTests() =

    [<Fact>]
    let ``Sorter.create allows Low = Hi within width``() =
        let ces = [| Ce.create 0 1; Ce.create 1 1 |]
        let sorter = Sorter.createWithNewId (UMX.tag<sortingWidth> 4) ces
        Assert.Equal(2, sorter.Ces.Length)

    [<Fact>]
    let ``Sorter.create rejects empty SorterId``() =
        let ces = [| Ce.create 0 1 |]
        let ex = Assert.Throws<Exception>(fun () -> 
            Sorter.create (UMX.tag<sorterId> Guid.Empty) (UMX.tag<sortingWidth> 4) ces |> ignore)
        Assert.Equal("Sorter ID must not be empty", ex.Message)

    [<Fact>]
    let ``Sorter.create rejects invalid width``() =
        let ces = [| Ce.create 0 1 |]
        let ex = Assert.Throws<Exception>(fun () -> 
            Sorter.createWithNewId (UMX.tag<sortingWidth> 0) ces |> ignore)
        Assert.Equal("Width must be at least 1", ex.Message)
