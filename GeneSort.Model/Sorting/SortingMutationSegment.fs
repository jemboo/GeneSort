namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingMutationSegment =
    private
        { 
          id : Guid<sortingMutationSegmentId>
          sortingMutator : sortingMutator
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sortingMutator: sortingMutator) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingMutationSegment =
        let id = 
            // Generate a unique ID based on the SorterModelGen and indices
            GuidUtils.guidFromObjs [
                    sortingMutator :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sortingMutationSegmentId>

        { id = id; sortingMutator = sortingMutator; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SortingMutator with get() = this.sortingMutator
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MakeMutantSortings : sorting [] =
        let mutantSortings = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                SortingMutator.makeMutantSorting index this.SortingMutator
            |]
        mutantSortings


    member this.MakeMutantSorters : sorter [] =
        this.MakeMutantSortings |> Array.collect(Sorting.makeSorters)


    member this.MakeMutantSorterSet : sorterSet =
        sorterSet.create  (%(this.id) |> UMX.tag<sorterSetId>)
                          this.MakeMutantSorters


    member this.GetMutantSortingIds : Guid<sortingId> [] =
        [| for i in 0 .. %this.count - 1 do
            let index = %this.firstIndex + i
            SortingMutator.getMutantSortingId index this.SortingMutator
        |]


    member this.MakeParentSorterIdsWithModelTags : (Guid<sorterId> * modelTag) [] =
        SortingMutator.getParentSorterIdsWithTags this.SortingMutator 


    member this.MakeMutantSorterIdsWithSortingTags : (Guid<sorterId> * modelSetTag) [] =
        [| for i in 0 .. %this.count - 1 do
            let index = %this.firstIndex + i
            let tupes = SortingMutator.makeMutantSorterIdsWithTags index this.SortingMutator
            let mutantSortingId = SortingMutator.getMutantSortingId index this.SortingMutator
            tupes |> Array.map (
                    fun (sorterId, modelTag) -> 
                            (sorterId, ModelSetTag.create mutantSortingId modelTag))
        |] |> Array.collect id
