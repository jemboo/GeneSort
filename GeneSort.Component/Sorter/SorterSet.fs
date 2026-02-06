namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Sorting

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
        { sorterSetId = sorterSetId; ceLength = ceLength; sorters = Array.copy sorters; }

    static member createWithNewId (ceLength: int<ceLength>) (sorters: sorter array) : sorterSet =
        sorterSet.create (UMX.tag<sorterSetId> (Guid.NewGuid())) ceLength sorters

    member this.CeLength with get() = this.ceLength
    member this.Id with get() = this.sorterSetId
    member this.Sorters with get() : sorter[] = this.sorters
