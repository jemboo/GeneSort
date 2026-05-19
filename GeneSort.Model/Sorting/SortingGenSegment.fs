namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sortingGenSegment =
    private
        { 
          id : Guid<sortingGenSegmentId>
          sortingGen : sortingGen
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sortingGen: sortingGen) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingGenSegment =
        let id = 
            GuidUtils.guidFromObjs [
                    box "sortingGenSegment"
                    box (sortingGen |> SortingGen.getId |> UMX.untag)
                    box (firstIndex |> UMX.untag)
                    box (count |> UMX.untag)
                ] |> UMX.tag<sortingGenSegmentId>

        { id = id; sortingGen = sortingGen; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SortingGen with get() = this.sortingGen
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MakeSortingSet
                (sortingSetId: Guid<sortingSetId>) : sortingSet =
        if %this.count <= 0 then
            failwith "Count must be greater than 0"
        if %this.firstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sortings = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                SortingGen.makeSortingFromIndex index this.sortingGen |]

        sortingSet.create sortingSetId sortings