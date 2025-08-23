namespace GeneSort.Sorter.Tests

open System
open Xunit
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable

type SorterSetTests() =

    [<Fact>]
    let ``SorterSet.create creates valid SorterSet``() =
        let createSorter ces =
            Sorter.createWithNewId (UMX.tag<sortingWidth> 4) ces
        
        let ces1 = [| Ce.create 0 1; Ce.create 1 1 |]
        let ces2 = [| Ce.create 0 2; Ce.create 1 2 |]
        let sorters = [| createSorter ces1; createSorter ces2 |]
        
        let sorterSet = SorterSet.createWithNewId sorters
        Assert.NotEqual(Guid.Empty, %sorterSet.SorterSetId)
        Assert.Equal(2, (%sorterSet.Sorters.Length * 1<sorterCount>))
        Assert.Equal(ces1 |> Array.toList, sorterSet.Sorters.[0].Ces)
        Assert.Equal(ces2 |> Array.toList, sorterSet.Sorters.[1].Ces)
        Assert.Equal(4, %sorterSet.Sorters.[0].Width)
        Assert.Equal(4, %sorterSet.Sorters.[1].Width)
