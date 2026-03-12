namespace GeneSort.SortingResult

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type sortingResultMap =
    private { 
        sortingResult: sortingResult
        evalMap: Dictionary<Guid<sorterId>, modelTag>
    }

    static member create (sortingResult: sortingResult) (evalEntries: (Guid<sorterId> * modelTag) seq) =
        let evalMap = Dictionary<Guid<sorterId>, modelTag>()
        for (sorterId, sortingTag) in evalEntries do
            evalMap.[sorterId] <- sortingTag
        { sortingResult = sortingResult; evalMap = evalMap }

    static member empty (sortingResult: sortingResult) =
        { 
            sortingResult = sortingResult
            evalMap = Dictionary<Guid<sorterId>, modelTag>()
        }

    member this.SortingResult with get() = this.sortingResult

    member this.SortingId with get() =
        SortingResult.getSortingId this.sortingResult

    member this.EvalMap with get() =
        this.evalMap :> IReadOnlyDictionary<Guid<sorterId>, modelTag>

    member this.EvalCount with get() = this.evalMap.Count

    member this.TryGetSortingTag (sorterId: Guid<sorterId>) =
        match this.evalMap.TryGetValue(sorterId) with
        | true, tag -> Some tag
        | false, _ -> None

    member this.ContainsSorter (sorterId: Guid<sorterId>) =
        this.evalMap.ContainsKey(sorterId)

    member this.UpdateSorterEval (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, modelTag ->
            SortingResult.UpdateSorterEval modelTag newEval this.sortingResult