namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Core

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



  module SorterSet = 
    
    let mergeSorterSets (sorterSets: sorterSet []) : sorterSet =
        let idSet = sorterSets |> Array.map(fun ss -> ss.Id :> obj)
        let mergedId = GuidUtils.guidFromObjs idSet |> UMX.tag<sorterSetId>
        let mergedSorters = sorterSets |> Array.collect(fun ss -> ss.Sorters)
        sorterSet.create mergedId mergedSorters