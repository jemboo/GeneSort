namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1
open GeneSort.Eval.V1.SorterRunResult

// ---------------------------------------------------------------------
// 1. Lightweight History Description DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type spmDescriptionDto = {
    [<Key(0)>] SorterPoolMemberId: Guid
    [<Key(1)>] SorterModelId: Guid
    [<Key(2)>] SorterMutationIndex: int
    [<Key(3)>] SorterMutationSource: sorterMutationSourceDto
    [<Key(4)>] SorterEval: sorterEvalDto option 
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


// ---------------------------------------------------------------------
// 2. Fully Realized Final State DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterPoolMemberDto = {
    [<Key(0)>] SorterPoolMemberId: Guid
    [<Key(1)>] SorterModel: sorterModel 
    [<Key(2)>] SorterMutationIndex: int
    [<Key(3)>] SorterMutationSource: sorterMutationSourceDto
    [<Key(4)>] SorterEval: sorterEvalDto option
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


// ---------------------------------------------------------------------
// 3. Main Result Target Wrapper
// ---------------------------------------------------------------------

[<MessagePackObject>]
type evolutionRunResultV1Dto = {
    [<Key(0)>] IntermediateHistory: sorterPoolSetDescriptionDto array
    [<Key(1)>] FinalPoolSet: sorterPoolSetDto
}

[<MessagePackObject>]
type evolutionRunResultDto =
    | [<Key(0)>] V1 of evolutionRunResultV1Dto
    | [<Key(1)>] Unknown


// ---------------------------------------------------------------------
// 4. Translation Logic Modules
// ---------------------------------------------------------------------

module SorterPoolSetDescriptionDto =

    let toDto (domain: sorterPoolSetDescription) : sorterPoolSetDescriptionDto =
        let poolDtos =
            domain.Pools
            |> Array.map (fun p ->
                let memberDtos =
                    p.SorterPoolMembers
                    |> Array.map (fun m ->
                        {
                            spmDescriptionDto.SorterPoolMemberId = UMX.untag m.SorterPoolMemberId
                            SorterModelId = UMX.untag m.SorterModelId
                            SorterMutationIndex = UMX.untag m.MutationIndex
                            SorterMutationSource = SorterMutationSourceDto.toDto m.SorterMutationSource
                            SorterEval = m.SorterEval |> Option.map SorterEvalDto.fromDomain
                        }
                    )
                { sorterPoolDescriptionDto.SorterPoolId = UMX.untag p.SorterPoolId; SorterPoolMembers = memberDtos }
            )
        {
            SorterPoolSetId = UMX.untag domain.SorterPoolSetId
            GenerationNumber = UMX.untag domain.GenerationNumber
            Pools = poolDtos
        }

    let fromDto (dto: sorterPoolSetDescriptionDto) : sorterPoolSetDescription =
        let poolDomains =
            dto.Pools
            |> Array.map (fun p ->
                let memberDomains =
                    p.SorterPoolMembers
                    |> Array.map (fun m ->
                        let evalOpt = m.SorterEval |> Option.map SorterEvalDto.toDomain
                        spmDescription.Create
                            (UMX.tag m.SorterPoolMemberId)
                            (UMX.tag m.SorterModelId)
                            (UMX.tag m.SorterMutationIndex)
                            (SorterMutationSourceDto.fromDto m.SorterMutationSource)
                            evalOpt
                    )
                sorterPoolDescription.Create(UMX.tag p.SorterPoolId, memberDomains)
            )
        sorterPoolSetDescription.Create
            (UMX.tag dto.SorterPoolSetId, UMX.tag dto.GenerationNumber, poolDomains)


module SorterPoolSetDto =

    let toDto (domain: sorterPoolSet) : sorterPoolSetDto =
        let poolDtos =
            domain.SorterPools
            |> Map.values
            |> Seq.map (fun p ->
                let memberDtos =
                    p.SorterPoolMembers
                    |> Seq.map (fun m ->
                        {
                            SorterPoolMemberId = UMX.untag m.SorterPoolMemberId
                            SorterModel = m.SorterModel
                            SorterMutationIndex = UMX.untag m.MutationIndex
                            SorterMutationSource = SorterMutationSourceDto.toDto m.SorterMutationSource
                            SorterEval = m.SorterEval |> Option.map SorterEvalDto.fromDomain
                        }
                    )
                    |> Seq.toArray
                { SorterPoolId = UMX.untag p.SorterPoolId; SorterPoolMembers = memberDtos }
            )
            |> Seq.toArray
        {
            SorterPoolSetId = UMX.untag domain.SorterPoolSetId
            GenerationNumber = UMX.untag domain.GenerationNumber
            SorterPools = poolDtos
        }

    let fromDto (dto: sorterPoolSetDto) : sorterPoolSet =
        let pools =
            dto.SorterPools
            |> Array.map (fun p ->
                let members =
                    p.SorterPoolMembers
                    |> Array.map (fun m ->
                        let evalOpt = m.SorterEval |> Option.map SorterEvalDto.toDomain
                        sorterPoolMember.Create
                            (UMX.tag m.SorterPoolMemberId)
                            m.SorterModel
                            (UMX.tag m.SorterMutationIndex)
                            (SorterMutationSourceDto.fromDto m.SorterMutationSource)
                            evalOpt
                    )
                sorterPool.Create(UMX.tag p.SorterPoolId, members)
            )
        sorterPoolSet.Create(UMX.tag dto.SorterPoolSetId, UMX.tag dto.GenerationNumber, pools)


module EvolutionRunResultDto =

    let toDto (domain: sorterRunResult) : evolutionRunResultDto =
        let v1 = {
            IntermediateHistory = domain.IntermediateHistory |> Array.map SorterPoolSetDescriptionDto.toDto
            FinalPoolSet = SorterPoolSetDto.toDto domain.FinalPoolSet
        }
        evolutionRunResultDto.V1 v1

    let fromDto (dto: evolutionRunResultDto) : sorterRunResult =
        match dto with
        | evolutionRunResultDto.V1 v1Dto ->
            {
                IntermediateHistory = v1Dto.IntermediateHistory |> Array.map SorterPoolSetDescriptionDto.fromDto
                FinalPoolSet = SorterPoolSetDto.fromDto v1Dto.FinalPoolSet
            }
        | evolutionRunResultDto.Unknown ->
            failwith "Cannot reconstruct evaluation run result from an Unknown DTO variant container state."