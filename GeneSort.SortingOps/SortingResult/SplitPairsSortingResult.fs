namespace GeneSort.SortingOps.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type splitPairsSortingResult=
    private { 
        sorterModelId: Guid<sorterModelId>
        mutable sorterEvalFirstFirst: sorterEval option
        mutable sorterEvalFirstSecond: sorterEval option
        mutable sorterEvalSecondFirst: sorterEval option
        mutable sorterEvalSecondSecond: sorterEval option
    }

    static member create (sorterModelId: Guid<sorterModelId>) =
        { 
            sorterModelId = sorterModelId
            sorterEvalFirstFirst = None
            sorterEvalFirstSecond = None
            sorterEvalSecondFirst = None
            sorterEvalSecondSecond = None
        }

    member this.SorterModelId with get() : Guid<sorterModelId> = this.sorterModelId
    member this.SorterEvalFirstFirst
        with get() = this.sorterEvalFirstFirst
        and set(value) = this.sorterEvalFirstFirst <- value
    member this.SorterEvalFirstSecond
        with get() = this.sorterEvalFirstSecond
        and set(value) = this.sorterEvalFirstSecond <- value
    member this.SorterEvalSecondFirst
        with get() = this.sorterEvalSecondFirst
        and set(value) = this.sorterEvalSecondFirst <- value
    member this.SorterEvalSecondSecond
        with get() = this.sorterEvalSecondSecond
        and set(value) = this.sorterEvalSecondSecond <- value


module SplitPairsSortingResult = ()

