namespace GeneSort.Sorter.Tests

open System
open Xunit
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter


type SorterTests() =

    [<Fact>]
    let ``Sorter.create allows Low = Hi within width``() =
        let ces = [| ce.create 0 1; ce.create 1 1 |]
        let sorter = sorter.createWithNewId (UMX.tag<sortingWidth> 4) ces
        Assert.Equal(2, sorter.Ces.Length)

    [<Fact>]
    let ``Sorter.create rejects empty SorterId``() =
        let ces = [| ce.create 0 1 |]
        let ex = Assert.Throws<ArgumentException>(fun () -> 
            sorter.create (UMX.tag<sorterId> Guid.Empty) (UMX.tag<sortingWidth> 4) ces |> ignore)
        Assert.Equal("Sorter ID must not be empty (Parameter 'sorterId')", ex.Message)

    [<Fact>]
    let ``Sorter.create rejects invalid width``() =
        let ces = [| ce.create 0 1 |]
        let ex = Assert.Throws<ArgumentException>(fun () -> 
            sorter.createWithNewId (UMX.tag<sortingWidth> 0) ces |> ignore)
        Assert.Equal("Width must be at least 1 (Parameter 'width')", ex.Message)
