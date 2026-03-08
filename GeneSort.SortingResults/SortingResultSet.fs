namespace GeneSort.SortingResult

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open System.Collections.Generic

type sortingResultSet =

    private { sortingResults: Dictionary<Guid<sortingId>, sortingResult> }

    static member create (sortingResults: sortingResult seq) =
        let dict = Dictionary<Guid<sortingId>, sortingResult>()
        for result in sortingResults do
            dict.[SortingResult.getSortingId result] <- result
        { sortingResults = dict }

    static member empty () =
        { sortingResults = Dictionary<Guid<sortingId>, sortingResult>() }

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

    member internal this.UpdateSorterEval (sortingId: Guid<sortingId>) 
                                          (modelTag: modelTag) 
                                          (newEval: sorterEval) =
        match this.sortingResults.TryGetValue(sortingId) with
        | false, _ -> failwithf "SortingId %A not found in sortingResultSet." sortingId
        | true, result -> SortingResult.UpdateSorterEval modelTag newEval result
        

module SortingResultSet = ()

            