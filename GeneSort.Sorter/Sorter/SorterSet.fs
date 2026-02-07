namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter
open System.Linq

 type sorterSet =
    private { 
        sorterSetId: Guid<sorterSetId>
        sorters: sorter array
        ceLength: int<ceLength> 
    }

    static member create (sorterSetId: Guid<sorterSetId>) 
                         (ceLength: int<ceLength>)
                         (sorters: sorter array) : sorterSet =
        if %sorterSetId = Guid.Empty then
            invalidArg "sorterSetId" "SorterSet ID must not be empty"
        if Array.isEmpty sorters then
            invalidArg "sorters" "Sorter set must contain at least one sorter"
        { sorterSetId = sorterSetId; ceLength = ceLength; sorters = Array.copy sorters; }

    static member createWithNewId (ceLength: int<ceLength>) (sorters: sorter array) : sorterSet =
        sorterSet.create (UMX.tag<sorterSetId> (Guid.NewGuid())) ceLength sorters

    member this.CeLength with get() = this.ceLength
    member this.Id with get() = this.sorterSetId
    member this.Sorters with get() : sorter[] = this.sorters
