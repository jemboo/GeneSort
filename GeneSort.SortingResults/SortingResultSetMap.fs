namespace GeneSort.SortingResult

open System.Collections.Generic
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting

type sortingResultSetMap =
    private { 
        sortingResults: Dictionary<Guid<sortingId>, sortingResult>
        evalMap: Dictionary<Guid<sorterId>, sortingTag>
    }

    static member create (sortingResults: sortingResult seq) (evalEntries: (Guid<sorterId> * sortingTag) seq) =
        let dict = Dictionary<Guid<sortingId>, sortingResult>()
        for result in sortingResults do
            dict.[SortingResult.getSortingId result] <- result
        let evalMap = Dictionary<Guid<sorterId>, sortingTag>()
        for (sorterId, sortingTag) in evalEntries do
            evalMap.[sorterId] <- sortingTag
        { sortingResults = dict; evalMap = evalMap }

    static member empty () =
        { 
            sortingResults = Dictionary<Guid<sortingId>, sortingResult>()
            evalMap = Dictionary<Guid<sorterId>, sortingTag>()
        }

    member this.Count with get() = this.sortingResults.Count

    member this.TryGetResult (sortingId: Guid<sortingId>) =
        match this.sortingResults.TryGetValue(sortingId) with
        | true, result -> Some result
        | false, _ -> None

    member this.GetResult (sortingId: Guid<sortingId>) =
        match this.sortingResults.TryGetValue(sortingId) with
        | true, result -> result
        | false, _ -> failwithf "SortingId %A not found in sortingResultSet." sortingId

    member this.ContainsId (sortingId: Guid<sortingId>) =
        this.sortingResults.ContainsKey(sortingId)

    member this.SortingResults with get() =
        this.sortingResults :> IReadOnlyDictionary<Guid<sortingId>, sortingResult>

    member this.EvalMap with get() =
        this.evalMap :> IReadOnlyDictionary<Guid<sorterId>, sortingTag>

    member this.UpdateSorterEval (newEval: sorterEval) =
        match this.evalMap.TryGetValue(newEval.SorterId) with
        | false, _ -> failwithf "SorterId %A not found in evalMap." newEval.SorterId
        | true, sortingTag ->
            let sortingParentId = SortingTag.getSortingParentId sortingTag
            let modelTag  = SortingTag.getModelTag sortingTag
            match this.sortingResults.TryGetValue(sortingParentId) with
            | false, _ -> failwithf "SortingId %A not found in sortingResultSet." sortingParentId
            | true, result -> SortingResult.UpdateSorterEval modelTag newEval result



module SortingResultSetMap = ()

