namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.SortingOps.Mp
open GeneSort.Eval.V1
open GeneSort.Model.Mp.Sorting.Mp.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting

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
    [<Key(5)>] birthday: int
}

[<MessagePackObject>]
type sorterPoolDto = {
    [<Key(0)>] sorterPoolId: Guid
    [<Key(1)>] name: string
    [<Key(2)>] sorterPoolMemberDtos: sorterPoolMemberDto array
    [<Key(3)>] ceLength: int
}

[<MessagePackObject>]
type sorterPoolSetDto = {
    [<Key(0)>] sorterPoolSetId: Guid
    [<Key(1)>] generationNumber: int
    [<Key(2)>] sorterPools: sorterPoolDto array
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
                            sorterMutationSource = m.SorterMutationSource |> Option.map SorterMutationSourceDto.toDto
                            sorterEvalDto = m.SorterEval |> Option.map SorterEvalDto.fromDomain
                            birthday = m.Birthday |> UMX.untag
                        }
                    )
                    |> Seq.toArray

                { sorterPoolId = %p.SorterPoolId; 
                  name = %p.Name; 
                  sorterPoolMemberDtos = memberDtos; 
                  ceLength = %p.RawCeLength }
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
                            (UMX.tag m.birthday)
                    )
                sorterPool.create 
                            (p.sorterPoolId |> UMX.tag<sorterPoolId>) 
                            (p.name |> UMX.tag<sorterPoolName>) 
                            members
                            (p.ceLength |> UMX.tag<ceLength> )
            )
        sorterPoolSet.Create(UMX.tag dto.sorterPoolSetId, UMX.tag dto.generationNumber, pools)

