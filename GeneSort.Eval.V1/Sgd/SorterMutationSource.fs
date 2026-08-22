namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Eval.V1

type sorterMutationSource =
    private {
        _sorterModelMutatorId: Guid<sorterModelMutatorId>
        _sorterPoolMemberId:   Guid<sorterPoolMemberId>
        _mutationIndex: int<mutationIndex>
    }
    member this.SorterModelMutatorId = this._sorterModelMutatorId
    member this.SorterPoolMemberId = this._sorterPoolMemberId
    member this.SorterMutationIndex = this._mutationIndex

    static member create
                    sorterModelMutatorId 
                    parentSorterPoolMemberId
                    parentMutationIndex =
        { 
            _sorterModelMutatorId = sorterModelMutatorId
            _sorterPoolMemberId = parentSorterPoolMemberId
            _mutationIndex = parentMutationIndex 
        }


module SorterMutationSource = 
    
    let toDataTableRecordWithPrefix (prefix: string) (source: sorterMutationSource) : dataTableRecord =
        (dataTableRecord.createEmpty())
        |> dataTableRecord.addData (sprintf "%sSorterModelMutatorId" prefix) (string (%source.SorterModelMutatorId))
        |> dataTableRecord.addData (sprintf "%sSorterPoolMemberId" prefix) (string (%source.SorterPoolMemberId))
        |> dataTableRecord.addData (sprintf "%sMutationIndex" prefix) (string (%source.SorterMutationIndex))