namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults


type splitPairSortingEvalBins =
    private {
        sortingEvalBinsId: Guid<sortingEvalBinsId>
        sorterEvalBinsFirstFirst: sorterEvalBins
        sorterEvalBinsFirstSecond: sorterEvalBins
        sorterEvalBinsSecondFirst: sorterEvalBins
        sorterEvalBinsSecondSecond: sorterEvalBins
    }

    static member create (id: Guid<sortingEvalBinsId>) =
        {
            sortingEvalBinsId = id
            sorterEvalBinsFirstFirst = sorterEvalBins.createWithNewId Seq.empty
            sorterEvalBinsFirstSecond = sorterEvalBins.createWithNewId Seq.empty
            sorterEvalBinsSecondFirst = sorterEvalBins.createWithNewId Seq.empty
            sorterEvalBinsSecondSecond = sorterEvalBins.createWithNewId Seq.empty
        }
        
    member this.SorterEvalBinsFirstFirst with get() = this.sorterEvalBinsFirstFirst
    member this.SorterEvalBinsFirstSecond with get() = this.sorterEvalBinsFirstSecond
    member this.SorterEvalBinsSecondFirst with get() = this.sorterEvalBinsSecondFirst
    member this.SorterEvalBinsSecondSecond with get() = this.sorterEvalBinsSecondSecond

    member this.SortingEvalBinsId with get() = this.sortingEvalBinsId

    member this.AddSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        match modelTag with
        | modelTag.Single -> failwith "wrong modelTag"
        | SplitPair sj -> 
            match sj with
             | First_First -> this.SorterEvalBinsFirstFirst.AddSorterEval sorterEval
             | First_Second -> this.SorterEvalBinsFirstSecond.AddSorterEval sorterEval
             | Second_First -> this.SorterEvalBinsSecondFirst.AddSorterEval sorterEval
             | Second_Second -> this.SorterEvalBinsSecondSecond.AddSorterEval sorterEval

module SplitPairSortingEvalBins = ()

