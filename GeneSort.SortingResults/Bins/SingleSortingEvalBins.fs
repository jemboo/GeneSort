namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Model.Sorting
open GeneSort.SortingResults


type singleSortingEvalBins =
    private {
        sortingEvalBinsId: Guid<sortingEvalBinsId>
        sorterEvalBins: sorterEvalBins
    }

    static member create (id: Guid<sortingEvalBinsId>) =
        {
            sortingEvalBinsId = id
            sorterEvalBins = sorterEvalBins.createWithNewId Seq.empty
        }
        
    member this.SortingEvalBinsId with get() = this.sortingEvalBinsId

    member this.SorterEvalBins with get() = this.sorterEvalBins

    member this.AddSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        match modelTag with
        | modelTag.Single -> this.sorterEvalBins.AddSorterEval sorterEval
        | _ -> failwith "wrong modelTag"



module SingleSortingEvalBins = 

    let fromStorage 
            (sortingEvalBinsId: Guid<sortingEvalBinsId>) 
            (sorterEvalBins: sorterEvalBins) : singleSortingEvalBins =
        {
            sortingEvalBinsId = sortingEvalBinsId
            sorterEvalBins = sorterEvalBins
        }


