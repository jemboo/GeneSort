namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sortingSetMutator =
    private
        { 
          id : Guid<sortingSetMutatorId>
          sortingMutator : sortingMutator
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sortingMutator: sortingMutator) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingSetMutator =
        let id = 
            // Generate a unique ID based on the SorterModelMaker and indices
            GuidUtils.guidFromObjs [
                    sortingMutator :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sortingSetMutatorId>

        { id = id; sortingMutator = sortingMutator; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SortingMutator with get() = this.sortingMutator
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MutateSortings
                : (Guid<sortingMutatorId> * sorting) [] =
        let mutantSortings = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                ( this.SortingMutator |> SortingMutator.getId,
                  SortingMutator.makeSorting index this.SortingMutator )
            |]
        mutantSortings

