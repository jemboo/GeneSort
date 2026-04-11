namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open System.Collections.Generic
open System
open GeneSort.Sorting.Sorter
open GeneSort.SortingResults.Bins

[<Struct; StructuralEquality; NoComparison>]
type levelSetFilter =
    private {
        sorterCount:     int<sorterCount>
        isSorted:    bool
    }
    static member create (sorterCount: int<sorterCount>) (isSorted: bool) =
        { sorterCount = sorterCount; isSorted = isSorted }
    member this.SorterCount with get() : int<sorterCount> = this.sorterCount
    member this.IsSorted    with get() : bool             = this.isSorted


module SortingSetFilter = 

    // sample the sortingSet based on it's performance based on sorterEvalBins.
    // take only sortings that produce a successfull sorter, and get samplesPerBin
    // from each of sorterEvalBins SorterEvalKeys
    let sampleBinsEvenly 
               (samplesPerBin: int)
               (sorterEvalBins: sorterEvalBins)
               (parentSet: sortingSet) : sorting [] =

        let resultSetMap = SortingResultSetMap.fromSortingSet parentSet
        let getSuccessfullySorted = true
        let sorterIds = SorterEvalBins.getUpToNSorterIdsPerBin
                            SorterEvalKey.noAction
                            getSuccessfullySorted
                            samplesPerBin
                            sorterEvalBins
                        |> Seq.map(snd)

        let sortingIds = sorterIds |> Seq.map(fun id -> resultSetMap.EvalMap[id].SortingId)
                                   |> Seq.distinct

        sortingIds |> Seq.map(parentSet.find) |> Seq.toArray



    let sampleBinsConvexHull
               (samplesPerBin: int)
               (sorterEvalBins: sorterEvalBins)
               (parentSet: sortingSet) : sorting [] =

        let resultSetMap = SortingResultSetMap.fromSortingSet parentSet
        let getSuccessfullySorted = true
        let sorterIds = SorterEvalBins.getUpToNSorterIdsPerConvexHullBin
                            SorterEvalKey.noAction
                            getSuccessfullySorted
                            samplesPerBin
                            sorterEvalBins
                        |> Seq.map(snd)

        let sortingIds = sorterIds |> Seq.map(fun id -> resultSetMap.EvalMap[id].SortingId)
                                   |> Seq.distinct

        sortingIds |> Seq.map(parentSet.find) |> Seq.toArray



    /// Returns up to maxReturned sortings from the center of the
    //  [CeCount, StageLength] distribution.
    /// Uses a balanced weight for CeCount and StageLength to define the center.
    let sampleTheCenterBins
               (maxReturned: int)
               (sorterEvalBins: sorterEvalBins)
               (parentSet: sortingSet) : sorting [] =

        let resultSetMap = SortingResultSetMap.fromSortingSet parentSet
        let getSuccessfullySorted = true
        
        // Define the center calculation metric
        let orderFunc = SorterEvalKey.byWeighted 1.0 1.0

        // Get the list of sorter IDs sorted by proximity to the average
        let sorterIds = 
            SorterEvalBins.getAverageSorterIds
                orderFunc
                getSuccessfullySorted
                sorterEvalBins
            |> Seq.map snd

        let sortingIds = 
            sorterIds 
            // 1. Safely handle potential missing IDs using choose + TryGetValue
            |> Seq.choose (fun id -> 
                match resultSetMap.EvalMap.TryGetValue(id) with
                | true, tag -> Some (ModelSetTag.getSortingParentId tag)
                | false, _  -> None)
            // 2. Filter out duplicates (multiple evals pointing to one sorting)
            |> Seq.distinct

        sortingIds 
        // 3. Retrieve the actual sorting object from the parent set
        |> Seq.map parentSet.find 
        // 4. Enforce the limit on unique sortings
        |> Seq.truncate maxReturned
        |> Seq.toArray



    /// Returns up to maxReturned sortings from the corner of the
    //  [CeCount, StageLength] distribution where CeCount is low and StageLength is low.
    /// Uses a balanced weight for CeCount and StageLength to define the winners.
    let sampleWinningBins
               (maxReturned: int)
               (sorterEvalBins: sorterEvalBins)
               (parentSet: sortingSet) : sorting [] =

        let resultSetMap = SortingResultSetMap.fromSortingSet parentSet
        let getSuccessfullySorted = true
        
        // Define the winning metric (low CeCount and low StageLength)
        let orderFunc = SorterEvalKey.byWeighted 1.0 1.0

        // Get the list of sorter IDs starting from the best performers
        let sorterIds = 
            SorterEvalBins.getWinningSorterIds
                orderFunc
                getSuccessfullySorted
                sorterEvalBins
            |> Seq.map snd

        let sortingIds = 
            sorterIds 
            // 1. Safely handle potential missing IDs using choose + TryGetValue
            |> Seq.choose (fun id -> 
                match resultSetMap.EvalMap.TryGetValue(id) with
                | true, tag -> Some (ModelSetTag.getSortingParentId tag)
                | false, _  -> None)
            // 2. Filter out duplicates (multiple evals pointing to one sorting)
            |> Seq.distinct

        sortingIds 
        // 3. Retrieve the actual sorting object from the parent set
        |> Seq.map parentSet.find 
        // 4. Enforce the limit on unique sortings
        |> Seq.truncate maxReturned
        |> Seq.toArray