namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sortingModelSetMutator =
    private
        { 
          id : Guid<sortingModelSetMutatorId>
          sortingModelMutator : sortingModelMutator
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sortingModelMutator: sortingModelMutator) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingModelSetMutator =
        let id = 
            // Generate a unique ID based on the SorterModelMaker and indices
            GuidUtils.guidFromObjs [
                    sortingModelMutator :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sortingModelSetMutatorId>

        { id = id; sortingModelMutator = sortingModelMutator; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SortingModelMutator with get() = this.sortingModelMutator
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MutateSortingModels
                : (Guid<sortingModelMutatorId> * sortingModel) [] =
        let mutantSortingModels = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                ( this.SortingModelMutator |> SortingModelMutator.getId,
                  SortingModelMutator.makeSortingModel index this.SortingModelMutator )
            |]
        mutantSortingModels

