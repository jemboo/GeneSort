namespace GeneSort.Eval.Mp.V1.Sgd

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1
open GeneSort.Model.Sorting.Mp.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Mp.V1
open GeneSort.Eval.V1.Sgd.Standard

type sorterPoolMemberDto = {
    sorterPoolMemberId: Guid
    sorterModelDto: sorterModelDto 
    sorterMutationIndex: int
    sorterMutationMod: int
    sorterMutationSource: sorterMutationSourceDto option
    sorterEvalDto: sorterEvalDto option
    sorterEvalDtos: Map<string, sorterEvalDto>
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
                            sorterEvalDtos =
                                m.SorterEvalMap
                                |> Map.toSeq
                                |> Seq.map (fun (subsetId, evaluation) ->
                                    %subsetId, SorterEvalDto.fromDomain evaluation)
                                |> Map.ofSeq
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
                        let evalMap =
                            if obj.ReferenceEquals(m.sorterEvalDtos, null) then
                                m.sorterEvalDto
                                |> Option.map SorterEvalDto.toDomain
                                |> Option.map (fun evaluation ->
                                    Map.ofList [ SorterEval.getSortableTestsSubsetId evaluation, evaluation ])
                                |> Option.defaultValue Map.empty
                            else
                                m.sorterEvalDtos
                                |> Map.toSeq
                                |> Seq.map (fun (subsetId, evaluation) ->
                                    subsetId |> UMX.tag<sortableTestsSubsetId>,
                                    SorterEvalDto.toDomain evaluation)
                                |> Map.ofSeq
                        let sourceOpt = m.sorterMutationSource |> Option.map SorterMutationSourceDto.fromDto
                        
                        sorterPoolMember.create
                            (UMX.tag m.sorterPoolMemberId)
                            (SorterModelDto.toDomain m.sorterModelDto)
                            (UMX.tag m.sorterMutationIndex)
                            (UMX.tag m.sorterMutationMod)
                            sourceOpt
                            evalMap
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
