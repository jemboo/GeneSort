namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1
open GeneSort.Model.Mp.Sorting.Mp.V1

// ---------------------------------------------------------------------
// 1. Lightweight History Description DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type spmDescriptionDto = {
    [<Key(0)>] sorterPoolMemberId: Guid
    [<Key(1)>] sorterModelId: Guid
    [<Key(2)>] sorterMutationIndex: int
    [<Key(3)>] sorterMutationSource: sorterMutationSourceDto option
    [<Key(4)>] sorterEval: sorterEvalDto option 
}

[<MessagePackObject>]
type sorterPoolDescriptionDto = {
    [<Key(0)>] sorterPoolId: Guid
    [<Key(1)>] sorterPoolName: string
    [<Key(2)>] spmDescriptionDtos: spmDescriptionDto array
}

[<MessagePackObject>]
type sorterPoolSetDescriptionDto = {
    [<Key(0)>] sorterPoolSetId: Guid
    [<Key(1)>] generationNumber: int
    [<Key(2)>] sorterPoolDescriptionDtos: sorterPoolDescriptionDto array
}


// ---------------------------------------------------------------------
// 2. Fully Realized Final State DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterPoolMemberDto = {
    [<Key(0)>] sorterPoolMemberId: Guid
    [<Key(1)>] sorterModelDto: sorterModelDto 
    [<Key(2)>] sorterMutationIndex: int
    [<Key(3)>] sorterMutationSource: sorterMutationSourceDto option
    [<Key(4)>] sorterEvalDto: sorterEvalDto option
}

[<MessagePackObject>]
type sorterPoolDto = {
    [<Key(0)>] sorterPoolId: Guid
    [<Key(1)>] name: string
    [<Key(2)>] sorterPoolMemberDtos: sorterPoolMemberDto array
}

[<MessagePackObject>]
type sorterPoolSetDto = {
    [<Key(0)>] sorterPoolSetId: Guid
    [<Key(1)>] generationNumber: int
    [<Key(2)>] sorterPools: sorterPoolDto array
}


// ---------------------------------------------------------------------
// 3. Main Result Target Wrapper
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterRunResultV1Dto = {
    [<Key(0)>] spsDescriptionDtos: sorterPoolSetDescriptionDto array
    [<Key(1)>] spsFinalDto: sorterPoolSetDto
}

[<MessagePackObject>]
type sorterRunResultDto =
    | [<Key(0)>] V1 of sorterRunResultV1Dto
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
                            spmDescriptionDto.sorterPoolMemberId = UMX.untag m.SorterPoolMemberId
                            sorterModelId = UMX.untag m.SorterModelId
                            sorterMutationIndex = UMX.untag m.MutationIndex
                            sorterMutationSource = m.SorterMutationSource |> Option.map SorterMutationSourceDto.toDto
                            sorterEval = m.SorterEval |> Option.map SorterEvalDto.fromDomain
                        }
                    )
                { 
                    sorterPoolDescriptionDto.sorterPoolId = UMX.untag p.SorterPoolId;
                    sorterPoolName = UMX.untag p.SorterPoolName;
                    spmDescriptionDtos = memberDtos 
                }
            )
        {
            sorterPoolSetId = UMX.untag domain.SorterPoolSetId
            generationNumber = UMX.untag domain.GenerationNumber
            sorterPoolDescriptionDtos = poolDtos
        }

    let fromDto (dto: sorterPoolSetDescriptionDto) : sorterPoolSetDescription =
        let poolDomains =
            dto.sorterPoolDescriptionDtos
            |> Array.map (fun p ->
                let memberDomains =
                    p.spmDescriptionDtos
                    |> Array.map (fun m ->
                        let evalOpt = m.sorterEval |> Option.map SorterEvalDto.toDomain
                        let sourceOpt = m.sorterMutationSource |> Option.map SorterMutationSourceDto.fromDto
                        spmDescription.Create
                            (UMX.tag m.sorterPoolMemberId)
                            (UMX.tag m.sorterModelId)
                            (UMX.tag m.sorterMutationIndex)
                            sourceOpt
                            evalOpt
                    )
                sorterPoolDescription.create
                                (p.sorterPoolId |> UMX.tag<sorterPoolId>) 
                                (p.sorterPoolName |> UMX.tag<sorterPoolName>)
                                memberDomains
            )
        sorterPoolSetDescription.Create
            (UMX.tag dto.sorterPoolSetId, UMX.tag dto.generationNumber, poolDomains)


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
                            sorterPoolMemberId = UMX.untag m.SorterPoolMemberId
                            sorterModelDto = SorterModelDto.fromDomain m.SorterModel
                            sorterMutationIndex = UMX.untag m.MutationIndex
                            sorterMutationSource = m.SorterMutationSource |> Option.map SorterMutationSourceDto.toDto
                            sorterEvalDto = m.SorterEval |> Option.map SorterEvalDto.fromDomain
                        }
                    )
                    |> Seq.toArray
                { sorterPoolId = %p.SorterPoolId; name = %p.Name; sorterPoolMemberDtos = memberDtos }
            )
            |> Seq.toArray
        {
            sorterPoolSetId = UMX.untag domain.SorterPoolSetId
            generationNumber = UMX.untag domain.GenerationNumber
            sorterPools = poolDtos
        }

    let fromDto (dto: sorterPoolSetDto) : sorterPoolSet =
        let pools =
            dto.sorterPools
            |> Array.map (fun p ->
                let members =
                    p.sorterPoolMemberDtos
                    |> Array.map (fun m ->
                        let evalOpt = m.sorterEvalDto |> Option.map SorterEvalDto.toDomain
                        let sourceOpt = m.sorterMutationSource |> Option.map SorterMutationSourceDto.fromDto
                        
                        sorterPoolMember.Create
                            (UMX.tag m.sorterPoolMemberId)
                            (SorterModelDto.toDomain m.sorterModelDto)
                            (UMX.tag m.sorterMutationIndex)
                            sourceOpt
                            evalOpt
                    )
                sorterPool.create (UMX.tag p.sorterPoolId) (UMX.tag p.name) members
            )
        sorterPoolSet.Create(UMX.tag dto.sorterPoolSetId, UMX.tag dto.generationNumber, pools)


module SorterRunResultDto =

    let fromDomain (domain: sorterRunResult) : sorterRunResultDto =
        let v1 = {
            sorterRunResultV1Dto.spsDescriptionDtos = domain.IntermediateHistory |> Array.map SorterPoolSetDescriptionDto.toDto
            spsFinalDto = SorterPoolSetDto.toDto domain.FinalPoolSet
        }
        sorterRunResultDto.V1 v1

    let toDomain (dto: sorterRunResultDto) : sorterRunResult =
        match dto with
        | sorterRunResultDto.V1 v1Dto ->
            sorterRunResult.create
                (SorterPoolSetDto.fromDto v1Dto.spsFinalDto)
                (v1Dto.spsDescriptionDtos |> Array.map SorterPoolSetDescriptionDto.fromDto)

        | sorterRunResultDto.Unknown ->
            failwith "Cannot reconstruct evaluation run result from an Unknown DTO variant container state."