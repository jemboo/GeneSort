namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Sorting

 type sorterSet =
    private { 
        sorterSetId: Guid<sorterSetId>
        sorters: sorter array
    }

    static member create (sorterSetId: Guid<sorterSetId>)
                         (sorters: sorter array) : sorterSet =
        if %sorterSetId = Guid.Empty then
            invalidArg "sorterSetId" "SorterSet ID must not be empty"
        { sorterSetId = sorterSetId; sorters = Array.copy sorters; }

    static member createWithNewId (sorters: sorter array) : sorterSet =
        sorterSet.create (UMX.tag<sorterSetId> (Guid.NewGuid())) sorters

    member this.Id with get() = this.sorterSetId
    member this.Sorters with get() : sorter[] = this.sorters
