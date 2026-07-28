namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting

[<MessagePackObject>]
type sorterMutationSourceDto = {
    [<Key(0)>] sorterModelMutatorId: Guid
    [<Key(1)>] sorterModelId: Guid
    [<Key(2)>] mutationIndex: int
    [<Key(2)>] ceLength: int
    [<Key(2)>] stageLength: int
}

module SorterMutationSourceDto =
    
    let toDto (domain: sorterMutationSource) : sorterMutationSourceDto =
        {
            sorterModelMutatorId = UMX.untag domain.SorterModelMutatorId
            sorterModelId = UMX.untag domain.SorterModelId
            mutationIndex = UMX.untag domain.SorterMutationIndex
            ceLength = UMX.untag domain.CeLength
            stageLength = UMX.untag domain.StageLength
        }


    let fromDto (dto: sorterMutationSourceDto) : sorterMutationSource =
        sorterMutationSource.create
            (UMX.tag<sorterModelMutatorId> dto.sorterModelMutatorId)
            (UMX.tag<sorterModelId> dto.sorterModelId)
            (UMX.tag<mutationIndex> dto.mutationIndex)
            (UMX.tag<ceLength> dto.ceLength)
            (UMX.tag<stageLength> dto.stageLength)
