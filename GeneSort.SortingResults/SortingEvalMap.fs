namespace GeneSort.SortingResults

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type sortingEvalMap =
    private { 
        sortingEval: sortingEval
        evalMap: Dictionary<Guid<sorterId>, modelTag>
    }

    static member create (sortingResult: sortingEval) (evalEntries: (Guid<sorterId> * modelTag) seq) =
        let evalMap = Dictionary<Guid<sorterId>, modelTag>()
        for (sorterId, sortingTag) in evalEntries do
            evalMap.[sorterId] <- sortingTag
        { sortingEval = sortingResult; evalMap = evalMap }

    static member empty (sortingResult: sortingEval) =
        { 
            sortingEval = sortingResult
            evalMap = Dictionary<Guid<sorterId>, modelTag>()
        }

    member this.SortingEval with get() = this.sortingEval

    member this.SortingId with get() =
        SortingResult.getSortingId this.sortingEval

    member this.EvalMap with get() =
        this.evalMap :> IReadOnlyDictionary<Guid<sorterId>, modelTag>

    member this.EvalCount with get() = this.evalMap.Count

    member this.TryGetSortingTag (sorterId: Guid<sorterId>) =
        match this.evalMap.TryGetValue(sorterId) with
        | true, tag -> Some tag
        | false, _ -> None

    member this.ContainsSorter (sorterId: Guid<sorterId>) =
        this.evalMap.ContainsKey(sorterId)

    member this.UpdateSortingResult (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, modelTag ->
            SortingResult.addSorterEval modelTag newEval this.sortingEval

    member this.GetAllTaggedSorterEvals () : (sorterEval * modelSetTag) seq =
         this.sortingEval |> SortingResult.getAllTaggedSorterEvals