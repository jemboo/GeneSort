namespace GeneSort.Eval.Mp.V1.Sgd

open System
open FSharp.UMX
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1
open GeneSort.Model.Sorting.Mp.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Mp.V1

type sorterPoolMemberDto = {
    sorterPoolMemberId: Guid
    sorterModelDto: sorterModelDto 
    sorterMutationIndex: int
    sorterMutationMod: int
    sorterMutationSource: sorterMutationSourceDto option
    sorterEvalDto: sorterEvalDto option
    birthday: int
}

type sorterPoolDto = {
    sorterPoolId: Guid
    name: string
    sorterPoolMemberDtos: sorterPoolMemberDto array
    ceLength: int
    mutationMod: int
    parentSorterPoolId: Nullable<Guid>
    sorterPoolTag: string
}

type sorterPoolSetDto = {
    sorterPoolSetId: Guid
    generationNumber: int
    sorterPools: sorterPoolDto array
    latticeBounds: string
}

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
                            sorterMutationMod = UMX.untag m.MutationMod
                            sorterMutationSource = m.SorterMutationSource |> Option.map SorterMutationSourceDto.toDto
                            sorterEvalDto = m.SorterEval |> Option.map SorterEvalDto.fromDomain
                            birthday = m.Birthday |> UMX.untag
                        }
                    )
                    |> Seq.toArray

                { 
                    sorterPoolId = %p.SorterPoolId
                    name = %p.Name
                    sorterPoolTag = (p.SorterPoolTag |> SorterPoolTag.toString)
                    sorterPoolMemberDtos = memberDtos
                    ceLength = %p.RawCeLength
                    mutationMod = %p.MutationMod
                    parentSorterPoolId = p.ParentSorterPoolId |> Option.map UMX.untag |> Option.toNullable
                }
            )
            |> Seq.toArray

        {
            sorterPoolSetId = UMX.untag domain.SorterPoolSetId
            generationNumber = UMX.untag domain.GenerationNumber
            sorterPools = poolDtos
            latticeBounds = LatticeBounds.toString domain.LatticeBounds
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
                        
                        sorterPoolMember.create
                            (UMX.tag m.sorterPoolMemberId)
                            (SorterModelDto.toDomain m.sorterModelDto)
                            (UMX.tag m.sorterMutationIndex)
                            (UMX.tag m.sorterMutationMod)
                            sourceOpt
                            evalOpt
                            (UMX.tag m.birthday)
                    )
                let parentIdOpt = 
                    p.parentSorterPoolId 
                    |> Option.ofNullable 
                    |> Option.map UMX.tag<sorterPoolId>

                sorterPool.create 
                    (p.sorterPoolId |> UMX.tag<sorterPoolId>) 
                    parentIdOpt
                    (p.name |> UMX.tag<sorterPoolName>) 
                    (SorterPoolTag.fromString p.sorterPoolTag)
                    members
                    (p.ceLength |> UMX.tag<ceLength>)
                    (p.mutationMod |> UMX.tag<mutationMod>)
            )

        let bounds = LatticeBounds.fromString dto.latticeBounds

        sorterPoolSet.create 
            (UMX.tag dto.sorterPoolSetId) 
            (UMX.tag dto.generationNumber) 
            bounds 
            (Some (pools :> seq<_>))