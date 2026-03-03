namespace GeneSort.SortingOps.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type singleSortingResult=
    private { 
        sorterModelId: Guid<sorterModelId>
        mutable sorterEval: sorterEval option
    }

    static member create 
                (sorterModelId: Guid<sorterModelId>) =
        { 
            sorterModelId = sorterModelId
            sorterEval = None
        }

    member this.SorterEval 
        with get() = this.sorterEval
        and set(value) = this.sorterEval <- value
    member this.SorterModelId with get() : Guid<sorterModelId> = this.sorterModelId


module SingleSortingResult = ()

