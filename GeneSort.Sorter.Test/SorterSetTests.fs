namespace GeneSort.Sorter.Tests

open System
open Xunit
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter

type SorterSetTests() =

    [<Fact>]
    let ``SorterSet.create creates valid SorterSet``() =
        let createSorter ces =
            sorter.createWithNewId (UMX.tag<sortingWidth> 4) ces
        
        let ces1 = [| ce.create 0 1; ce.create 1 1 |]
        let ces2 = [| ce.create 0 2; ce.create 1 2 |]
        let sorters = [| createSorter ces1; createSorter ces2 |]
        
        let sorterSet = sorterSet.createWithNewId 2<ceLength> sorters
        Assert.NotEqual(Guid.Empty, %sorterSet.SorterSetId)
        Assert.Equal(2, (%sorterSet.Sorters.Length * 1<sorterCount>))
        Assert.Equal(ces1 |> Array.toList, sorterSet.Sorters.[0].Ces)
        Assert.Equal(ces2 |> Array.toList, sorterSet.Sorters.[1].Ces)
        Assert.Equal(4, %sorterSet.Sorters.[0].SortingWidth)
        Assert.Equal(4, %sorterSet.Sorters.[1].SortingWidth)
