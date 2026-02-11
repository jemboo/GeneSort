namespace GeneSort.Model.Sorting.SorterPair
open FSharp.UMX

open GeneSort.Sorting.Sorter
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair.SplitPairs


type sorterPairModel =
     | SplitPairs of msSplitPairs
     | SplitPairs2 of msSplitPairs



module SorterPairModel =
    
    let getId (model: sorterPairModel) : Guid<sorterModelID> =
        match model with
        | SplitPairs sp -> sp.Id
        | SplitPairs2 sp -> sp.Id

    let makeSorters (model: sorterPairModel) : sorter =
        match model with
        | SplitPairs sp -> failwith "Not implemented"
        | SplitPairs2 sp -> failwith "Not implemented"


    let getSortingWidth (model: sorterPairModel) : int<sortingWidth> =
        match model with
        | SplitPairs sp -> sp.SortingWidth
        | SplitPairs2 sp -> sp.SortingWidth

    // Add a method to sorterPairModel that generates it's child sorter models.
    let isAChildOf (parentModel: sorterPairModel) (sorterId: Guid<sorterId>) : bool =
        failwith "Not implemented"

