namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.SortingOps

[<MessagePackObject>]
type sorterMutationSourceV1Dto = {
    [<Key(0)>] SorterModelMutatorId: Guid
    [<Key(1)>] SorterModelId: Guid
    [<Key(2)>] MutationIndex: int
}
[<MessagePackObject>]
type sorterMutationSourceDto = 
    | [<Key(0)>] V1 of sorterMutationSourceV1Dto
    | [<Key(1)>] Unknown


module SorterMutationSourceV1Dto =
    
    let toDto (domain: sorterMutationSource) : sorterMutationSourceV1Dto =
        {
            SorterModelMutatorId = UMX.untag domain.SorterModelMutatorId
            SorterModelId = UMX.untag domain.SorterModelId
            MutationIndex = UMX.untag domain.SorterMutationIndex
        }

    let fromDto (dto: sorterMutationSourceV1Dto) : sorterMutationSource =
        sorterMutationSource.create
            (UMX.tag<sorterModelMutatorId> dto.SorterModelMutatorId)
            (UMX.tag<sorterModelId> dto.SorterModelId)
            (UMX.tag<mutationIndex> dto.MutationIndex)



module SorterMutationSourceDto =
    
    let toDto (source: sorterMutationSource option) : sorterMutationSourceDto =
        match source with
        | Some src -> sorterMutationSourceDto.V1 (SorterMutationSourceV1Dto.toDto src)
        | None -> sorterMutationSourceDto.Unknown

    let fromDto (dto: sorterMutationSourceDto) : sorterMutationSource option =
        match dto with
        | sorterMutationSourceDto.V1 v1Dto -> Some (SorterMutationSourceV1Dto.fromDto v1Dto)
        | sorterMutationSourceDto.Unknown -> None