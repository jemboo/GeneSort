namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Sorting

type sorterMutationSource =
    private {
        _sorterModelMutatorId: Guid<sorterModelMutatorId>
        _sorterModelId: Guid<sorterModelId>
        _mutationIndex: int<mutationIndex>
        _ceLength: int<ceLength> 
        _stageLength: int<stageLength>
    }
    member this.CeLength = this._ceLength
    member this.StageLength = this._stageLength
    member this.SorterModelMutatorId = this._sorterModelMutatorId
    member this.SorterModelId = this._sorterModelId
    member this.SorterMutationIndex = this._mutationIndex

    static member create 
                    sorterModelMutatorId 
                    parentSorterModelId 
                    parentMutationIndex 
                    ceLength
                    stageLength =
        { 
            _ceLength = ceLength
            _stageLength = stageLength
            _sorterModelMutatorId = sorterModelMutatorId
            _sorterModelId = parentSorterModelId
            _mutationIndex = parentMutationIndex 
        }


module SorterMutationSource = 
    
    let toDataTableRecordWithPrefix (prefix: string) (source: sorterMutationSource) : dataTableRecord =
        (dataTableRecord.createEmpty())
        |> dataTableRecord.addData (sprintf "%sSorterModelMutatorId" prefix) (string (%source.SorterModelMutatorId))
        |> dataTableRecord.addData (sprintf "%sParentSorterModelId" prefix) (string (%source.SorterModelId))
        |> dataTableRecord.addData (sprintf "%sParentMutationIndex" prefix) (string (%source.SorterMutationIndex))
        |> dataTableRecord.addData (sprintf "%sParentCeLength" prefix) (string (%source.CeLength))
        |> dataTableRecord.addData (sprintf "%sParentStageLength" prefix) (string (%source.StageLength))