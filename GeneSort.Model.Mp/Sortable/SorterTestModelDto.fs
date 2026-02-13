namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Core.Mp
open GeneSort.Sorting


[<MessagePackObject>]
type msasFDto = {
[<Key(0)>] Id: Guid
[<Key(1)>] SortingWidth: int
}

[<MessagePackObject>]
type msasODto = {
[<Key(0)>] Id: Guid
[<Key(1)>] SeedPermutation: PermutationDto
[<Key(2)>] MaxOrbit: int
}

[<MessagePackObject>]
type sorterTestModelDto = {
[<Key(0)>] Kind: int // 0 = MsasF, 1 = MsasO
[<Key(1)>] MsasF: msasFDto // used when Kind=0
[<Key(2)>] MsasO: msasODto // used when Kind=1
}

module MsasFDtoConv =

    let fromDomain (m: msasF) : msasFDto =
        {
            Id = %m.Id
            SortingWidth = %m.SortingWidth
        }

    let toDomain (dto: msasFDto) : msasF =
        // Id is deterministic from SortingWidth via MsasF.create; dto.Id is informational
        msasF.create (dto.SortingWidth |> UMX.tag<sortingWidth>)


module MsasODtoConv =

    let toDto (m: msasO) : msasODto =
        {
            Id = %m.Id
            SeedPermutation = m.SeedPermutation |> PermutationDto.fromDomain
            MaxOrbit = m.MaxOrbit
        }

    let fromDto (dto: msasODto) : msasO =
        let perm = dto.SeedPermutation |> PermutationDto.toDomain |> Result.toOption |> Option.get
        msasO.create perm dto.MaxOrbit


module SorterTestModelDto =

    let fromDomain (dto: sorterTestModelDto) : sortableTestModel =
        match dto.Kind with
        | 0 -> dto.MsasF |> MsasFDtoConv.toDomain |> sortableTestModel.MsasF
        | 1 -> dto.MsasO |> MsasODtoConv.fromDto |> sortableTestModel.MsasO
        | k -> failwithf "Unknown SorterTestModelDto.Kind = %d" k


    let toDomain (m: sortableTestModel) : sorterTestModelDto =
        match m with
        | sortableTestModel.MsasF msasF ->
            {
                Kind = 0
                MsasF = msasF |> MsasFDtoConv.fromDomain
                MsasO = Unchecked.defaultof<msasODto>
            }
        | sortableTestModel.MsasO msasO ->
            {
                Kind = 1
                MsasF = Unchecked.defaultof<msasFDto>
                MsasO = msasO |> MsasODtoConv.toDto
            }

