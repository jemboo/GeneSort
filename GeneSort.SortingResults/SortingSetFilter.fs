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

        let sortingIds = sorterIds |> Seq.map(fun id -> fst resultSetMap.EvalMap[id])
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

        let sortingIds = sorterIds |> Seq.map(fun id -> fst resultSetMap.EvalMap[id])
                                   |> Seq.distinct

        sortingIds |> Seq.map(parentSet.find) |> Seq.toArray




    //let getSuccessfulSorts 
    //           (ceCountWeight: float) 
    //           (stageLengthWeight: float) 
    //           (sorterEvalBins: sorterEvalBins)
    //           (sortingSet: sortingSet) : sortingSet =
    
            
