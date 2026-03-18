namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Sorting.Sorter

[<Struct; StructuralEquality; NoComparison>]
type levelSetFilter =
    private {
        sorterCount:     int<sorterCount>
        isSorted:    bool
    }
    static member create (sorterCount: int<sorterCount>) (isSorted: bool) =
        { sorterCount = sorterCount; isSorted = isSorted }
    member this.SorterCount with get() : int<sorterCount> = this.sorterCount
    member this.IsSorted    with get() : bool             = this.isSorted


module SortingSetFilter = ()


