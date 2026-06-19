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

[<MessagePackObject>]
type spmDescriptionDto = {
    [<Key(0)>] SorterPoolMemberId: Guid
    [<Key(1)>] SorterModelId: Guid
    [<Key(2)>] SorterMutationIndex: int
    [<Key(3)>] SorterMutationSource: sorterMutationSourceDto
    // Note: Assuming a sorterEvalDto exists or using MessagePack fallback serialization
    [<Key(4)>] SorterEval: sorterEval option 
}

[<MessagePackObject>]
type sorterPoolDescriptionDto = {
    [<Key(0)>] SorterPoolId: Guid
    [<Key(1)>] SorterPoolMembers: spmDescriptionDto array
}

[<MessagePackObject>]
type sorterPoolSetDescriptionDto = {
    [<Key(0)>] SorterPoolSetId: Guid
    [<Key(1)>] GenerationNumber: int
    [<Key(2)>] Pools: sorterPoolDescriptionDto array
}




// --- Fully Realized Final State DTOs ---

[<MessagePackObject>]
type sorterPoolMemberDto = {
    [<Key(0)>] SorterPoolMemberId: Guid
    // Assuming sorterModel possesses its own DTO implementation, otherwise stored via primitive map or MessagePack union
    [<Key(1)>] SorterModel: sorterModel 
    [<Key(2)>] SorterMutationIndex: int
    [<Key(3)>] SorterMutationSource: sorterMutationSourceDto
    [<Key(4)>] SorterEval: sorterEval option
}

[<MessagePackObject>]
type sorterPoolDto = {
    [<Key(0)>] SorterPoolId: Guid
    [<Key(1)>] SorterPoolMembers: sorterPoolMemberDto array
}

[<MessagePackObject>]
type sorterPoolSetDto = {
    [<Key(0)>] SorterPoolSetId: Guid
    [<Key(1)>] GenerationNumber: int
    [<Key(2)>] SorterPools: sorterPoolDto array
}


// --- Evolution Run Result Aggregator DTO ---

[<MessagePackObject>]
type evolutionRunResultV1Dto = {
    [<Key(0)>] IntermediateHistory: sorterPoolSetDescriptionDto array
    [<Key(1)>] FinalPoolSet: sorterPoolSetDto
}

[<MessagePackObject>]
type evolutionRunResultDto =
    | [<Key(0)>] V1 of evolutionRunResultV1Dto
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
            (UMX.tag<sorterMutationIndex> dto.MutationIndex)



module SorterMutationSourceDto =
    
    let toDto (source: sorterMutationSource option) : sorterMutationSourceDto =
        match source with
        | Some src -> sorterMutationSourceDto.V1 (SorterMutationSourceV1Dto.toDto src)
        | None -> sorterMutationSourceDto.Unknown

    let fromDto (dto: sorterMutationSourceDto) : sorterMutationSource option =
        match dto with
        | sorterMutationSourceDto.V1 v1Dto -> Some (SorterMutationSourceV1Dto.fromDto v1Dto)
        | sorterMutationSourceDto.Unknown -> None