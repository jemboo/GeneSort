namespace GeneSort.Eval.Mp.V1.Sgd

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd

[<MessagePackObject>]
type sorterMutationSourceDto = {
    [<Key(0)>] sorterModelMutatorId: Guid
    [<Key(1)>] sorterPoolMemberId: Guid
    [<Key(2)>] mutationIndex: int
}

module SorterMutationSourceDto =
    
    let toDto (domain: sorterMutationSource) : sorterMutationSourceDto =
        {
            sorterModelMutatorId = UMX.untag domain.SorterModelMutatorId
            sorterPoolMemberId = UMX.untag domain.SorterPoolMemberId
            mutationIndex = UMX.untag domain.SorterMutationIndex
        }

    let fromDto (dto: sorterMutationSourceDto) : sorterMutationSource =
        sorterMutationSource.create
            (UMX.tag<sorterModelMutatorId> dto.sorterModelMutatorId)
            (UMX.tag<sorterPoolMemberId> dto.sorterPoolMemberId)
            (UMX.tag<mutationIndex> dto.mutationIndex)