namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Sgd

[<MessagePackObject>]
type sorterMutationSourceDto = {
    [<Key(0)>] sorterModelMutatorId: Guid
    [<Key(1)>] SorterModelId: Guid
    [<Key(2)>] MutationIndex: int
}

module SorterMutationSourceDto =
    
    let toDto (domain: sorterMutationSource) : sorterMutationSourceDto =
        {
            sorterModelMutatorId = UMX.untag domain.SorterModelMutatorId
            SorterModelId = UMX.untag domain.SorterModelId
            MutationIndex = UMX.untag domain.SorterMutationIndex
        }

    let fromDto (dto: sorterMutationSourceDto) : sorterMutationSource =
        sorterMutationSource.create
            (UMX.tag<sorterModelMutatorId> dto.sorterModelMutatorId)
            (UMX.tag<sorterModelId> dto.SorterModelId)
            (UMX.tag<mutationIndex> dto.MutationIndex)


