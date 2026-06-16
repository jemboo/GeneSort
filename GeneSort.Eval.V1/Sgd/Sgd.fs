namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1



type sorterModelSgd = 
    private {
        sorterModel: sorterModel
        mutationIndex: int
        parentModelId: Guid<sorterModelId>
        parentMutationIndex: int
    }

module Sgd = ()

