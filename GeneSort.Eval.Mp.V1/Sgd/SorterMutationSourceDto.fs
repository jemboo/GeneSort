namespace GeneSort.Eval.Mp.V1.Sgd

open System
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd

type sorterMutationSourceDto = {
    sorterModelMutatorId: Guid
    sorterPoolMemberId: Guid
    sorterPoolId: Guid
    mutationIndex: int
}

module SorterMutationSourceDto =
    
    let toDto (domain: sorterMutationSource) : sorterMutationSourceDto =
        {
            sorterModelMutatorId = UMX.untag domain.SorterModelMutatorId
            sorterPoolMemberId = UMX.untag domain.SorterPoolMemberId
            sorterPoolId = UMX.untag domain.SorterPoolId
            mutationIndex = UMX.untag domain.SorterMutationIndex
        }

    let fromDto (dto: sorterMutationSourceDto) : sorterMutationSource =
        sorterMutationSource.create
            (UMX.tag<sorterModelMutatorId> dto.sorterModelMutatorId)
            (UMX.tag<sorterPoolMemberId> dto.sorterPoolMemberId)
            (UMX.tag<sorterPoolId> dto.sorterPoolId)
            (UMX.tag<mutationIndex> dto.mutationIndex)

