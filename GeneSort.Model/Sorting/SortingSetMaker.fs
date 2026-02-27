namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sortingModelSetMaker =
    private
        { 
          id : Guid<sortingModelSetMakerId>
          sortingModelMaker : sortingModelMaker
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sortingModelMaker: sortingModelMaker) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingModelSetMaker =
        let id = 
            // Generate a unique ID based on the SorterModelMaker and indices
            GuidUtils.guidFromObjs [
                    sortingModelMaker :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sortingModelSetMakerId>

        { id = id; sortingModelMaker = sortingModelMaker; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SortingModelMaker with get() = this.sortingModelMaker
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MakeSortingModelSet : sortingModelSet =
        if %this.count <= 0 then
            failwith "Count must be greater than 0"
        if %this.firstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sortingModels = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                SortingModelMaker.makeSortingModel index this.sortingModelMaker |]

        let id = (%this.id) |> UMX.tag<sortingModelSetId>
        sortingModelSet.create id sortingModels