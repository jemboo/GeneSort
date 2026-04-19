namespace GeneSort.SortingResults

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type sortingEvalSetMap =
    private { 
        sortingEvalsMap: Dictionary<Guid<sortingId>, sortingEval>
        evalMap: Dictionary<Guid<sorterId>, modelSetTag>
    }

    static member create 
                    (sortingResults: sortingEval seq) 
                    (evalEntries: (Guid<sorterId> * modelSetTag) seq) =
        let dict = Dictionary<Guid<sortingId>, sortingEval>()
        for result in sortingResults do
            dict.[SortingEval.getSortingId result] <- result
        let evalMap = Dictionary<Guid<sorterId>, modelSetTag>()
        for (sorterId, sortingTag) in evalEntries do
            evalMap.[sorterId] <- sortingTag
        { sortingEvalsMap = dict; evalMap = evalMap }

    static member empty () =
        { 
            sortingEvalsMap = Dictionary<Guid<sortingId>, sortingEval>()
            evalMap = Dictionary<Guid<sorterId>, modelSetTag>()
        }

    member this.Count with get() = this.sortingEvalsMap.Count

    member this.TryGetResult (sortingId: Guid<sortingId>) =
        match this.sortingEvalsMap.TryGetValue(sortingId) with
        | true, result -> Some result
        | false, _ -> None

    member this.GetResult (sortingId: Guid<sortingId>) =
        match this.sortingEvalsMap.TryGetValue(sortingId) with
        | true, result -> result
        | false, _ -> failwithf "SortingId %A not found in sortingEvalsMap." sortingId

    member this.ContainsId (sortingId: Guid<sortingId>) =
        this.sortingEvalsMap.ContainsKey(sortingId)

    member this.SortingEvals with get() : sortingEval [] =
        this.sortingEvalsMap.Values |> Seq.map(id) |> Seq.toArray

    member this.SortingEvalsMap with get() =
        this.sortingEvalsMap :> IReadOnlyDictionary<Guid<sortingId>, sortingEval>

    member this.EvalMap with get() =
        this.evalMap :> IReadOnlyDictionary<Guid<sorterId>, modelSetTag>

    member this.UpdateSortingEvals (newEval: sorterEval) : unit =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, sortingTag ->
            let sortingParentId = ModelSetTag.getSortingParentId sortingTag
            let modelTag  = ModelSetTag.getModelTag sortingTag
            match this.sortingEvalsMap.TryGetValue(sortingParentId) with
            | false, _ -> failwithf "SortingId %A not found in sortingEvalsMap." sortingParentId
            | true, result -> SortingEval.addSorterEval modelTag newEval result

    member this.UpdateManySortingEvals (newEvals: sorterEval []) =
        newEvals |> Array.iter(this.UpdateSortingEvals)

    member this.GetAllTaggedSorterEvals () : (sorterEval * modelSetTag) seq =
         this.sortingEvalsMap.Values |> Seq.collect(fun sr -> sr |> SortingEval.getAllTaggedSorterEvals)


module SortingEvalSetMap = 

    let fromSortingSet (sSet:sortingSet) : sortingEvalSetMap =
         let sortingEvals = sSet.Sortings |> Array.map(SortingEval.makeFromSorting)
         sortingEvalSetMap.create sortingEvals sSet.SorterIdsWithSortingTags
         


        


