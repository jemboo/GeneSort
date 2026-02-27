namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sortingSetMaker =
    private
        { 
          id : Guid<sortingSetMakerId>
          sortingMaker : sortingMaker
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sortingMaker: sortingMaker) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingSetMaker =
        let id = 
            // Generate a unique ID based on the SorterModelMaker and indices
            GuidUtils.guidFromObjs [
                    sortingMaker :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sortingSetMakerId>

        { id = id; sortingMaker = sortingMaker; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SortingMaker with get() = this.sortingMaker
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MakeSortingSet : sortingSet =
        if %this.count <= 0 then
            failwith "Count must be greater than 0"
        if %this.firstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sortings = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                SortingMaker.makeSorting index this.sortingMaker |]

        let id = (%this.id) |> UMX.tag<sortingSetId>
        sortingSet.create id sortings