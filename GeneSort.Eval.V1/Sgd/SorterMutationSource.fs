namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1

// --- Types ---

type sorterMutationSource =
    private {
        _sorterModelMutatorId: Guid<sorterModelMutatorId>
        _sorterModelId: Guid<sorterModelId>
        _mutationIndex: int<sorterMutationIndex>
    }
    member this.SorterModelMutatorId = this._sorterModelMutatorId
    member this.SorterModelId = this._sorterModelId
    member this.SorterMutationIndex = this._mutationIndex

    static member create 
                    sorterModelMutatorId 
                    parentSorterModelId 
                    parentMutationIndex =
        { 
            _sorterModelMutatorId = sorterModelMutatorId
            _sorterModelId = parentSorterModelId
            _mutationIndex = parentMutationIndex 
        }
