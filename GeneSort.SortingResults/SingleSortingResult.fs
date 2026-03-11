namespace GeneSort.SortingResult

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type singleSortingResult=
    private { 
        sortingId: Guid<sortingId>
        mutable sorterEval: sorterEval option
    }

    static member create 
                (sorterModelId: Guid<sortingId>) =
        { 
            sortingId = sorterModelId
            sorterEval = None
        }

    static member empty () : singleSortingResult =
        { 
            sortingId = Guid.Empty |> UMX.tag<sortingId>
            sorterEval = None
        }

    member this.SorterEval 
        with get() = this.sorterEval
        and set(value) = this.sorterEval <- value

    member this.SortingId with get() : Guid<sortingId> = this.sortingId

    member this.UpdateSorterEval (modelTag: modelTag) (newEval: sorterEval) : unit =
        match modelTag with 
        | modelTag.Single -> this.SorterEval <- Some newEval
        | modelTag.SplitPair splitJoin -> failwith "invalid modeltag"
            


module SingleSortingResult = ()

