namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Model.Sorting


type sortingEvalBinsSet =
    private {
        sortingEvalBinsMap: Dictionary<Guid<sortingId>, sortingEvalBins>
    }

    static member create (sortingEvalBins: sortingEvalBins seq) =
           let sortingEvalBinsMap = new Dictionary<Guid<sortingId>, sortingEvalBins>()
           for seb in sortingEvalBins do
                sortingEvalBinsMap[seb.SortingId] <- seb
           {
                sortingEvalBinsMap = sortingEvalBinsMap
           }

    member this.SortingEvalBinsMap with get() = this.sortingEvalBinsMap :> IReadOnlyDictionary<Guid<sortingId>, sortingEvalBins>


    //member this.AddSorterEvals (tag: modelTag) (sorterEvals: sorterEval seq) =
    //    match this.sorterEvalBinsMap.TryGetValue(tag) with
    //    | true, bins -> bins.AddSorterEvals sorterEvals
    //    | false, _   -> this.sorterEvalBinsMap.[tag] <- sorterEvalBins.createWithNewId sorterEvals




module SortingEvalBinsSet =

    let createFromSortings (sortings: sorting seq) : sortingEvalBinsSet =
        let sortingEvalBinsSeq = sortings |> Seq.map(SortingEvalBins.createFromSorting)
        sortingEvalBinsSet.create sortingEvalBinsSeq