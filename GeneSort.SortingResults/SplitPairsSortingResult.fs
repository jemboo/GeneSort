namespace GeneSort.SortingResults

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


type splitPairsSortingResult=
    private { 
        sortingId: Guid<sortingId>
        mutable sorterEvalFirstFirst: sorterEval option
        mutable sorterEvalFirstSecond: sorterEval option
        mutable sorterEvalSecondFirst: sorterEval option
        mutable sorterEvalSecondSecond: sorterEval option
    }

    static member create (sortingId: Guid<sortingId>) =
        { 
            sortingId = sortingId
            sorterEvalFirstFirst = None
            sorterEvalFirstSecond = None
            sorterEvalSecondFirst = None
            sorterEvalSecondSecond = None
        }

    static member empty () : splitPairsSortingResult =
        { 
            sortingId = Guid.Empty |> UMX.tag<sortingId>
            sorterEvalFirstFirst = None
            sorterEvalFirstSecond = None
            sorterEvalSecondFirst = None
            sorterEvalSecondSecond = None
        }


    member this.SortingId with get() : Guid<sortingId> = this.sortingId

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

    member this.AddSorterEval (modelTag: modelTag) (newEval: sorterEval) : unit =
        match modelTag with 
        | modelTag.Single -> failwith "Invalid model tag for split pairs sorting result."
        | modelTag.SplitPair splitJoin ->
            match splitJoin with
            | splitJoin.First_First -> this.SorterEvalFirstFirst <- Some newEval
            | splitJoin.First_Second -> this.SorterEvalFirstSecond <- Some newEval
            | splitJoin.Second_First -> this.SorterEvalSecondFirst <- Some newEval
            | splitJoin.Second_Second -> this.SorterEvalSecondSecond <- Some newEval


    member this.GetSorterEval (modelTag: modelTag) : sorterEval =
        match modelTag with
        | modelTag.Single -> failwith "Invalid model tag for split pairs sorting result."
        | modelTag.SplitPair splitJoin ->
            match splitJoin with
            | splitJoin.First_First -> this.SorterEvalFirstFirst.Value
            | splitJoin.First_Second -> this.SorterEvalFirstSecond.Value
            | splitJoin.Second_First -> this.SorterEvalSecondFirst.Value
            | splitJoin.Second_Second -> this.SorterEvalSecondSecond.Value

    member this.GetAllSorterEvals () : (sorterEval * modelSetTag) seq =
        seq { 
            yield (this.SorterEvalFirstFirst.Value, ModelSetTag.create this.sortingId (splitJoin.First_First |> modelTag.SplitPair) )
            yield (this.SorterEvalFirstSecond.Value, ModelSetTag.create this.sortingId (splitJoin.First_Second |> modelTag.SplitPair) )
            yield (this.SorterEvalSecondFirst.Value, ModelSetTag.create this.sortingId (splitJoin.Second_First |> modelTag.SplitPair) )
            yield (this.SorterEvalSecondSecond.Value, ModelSetTag.create this.sortingId (splitJoin.Second_Second |> modelTag.SplitPair) )
        }


module SplitPairsSortingResult = ()

