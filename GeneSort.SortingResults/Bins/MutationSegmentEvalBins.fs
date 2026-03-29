namespace GeneSort.SortingResults.Bins

open System.Collections.Generic
open FSharp.UMX

open GeneSort.SortingOps
open GeneSort.Model.Sorting


type mutationSegmentEvalBins =
    private {
        sortingEvalParentMap: Dictionary<Guid<sortingId>, sortingEvalKeys>
        sortingEvalBinsMap: Dictionary<Guid<sortingId>, sortingEvalBins>
    }


module MutationSegmentEvalBins = ()