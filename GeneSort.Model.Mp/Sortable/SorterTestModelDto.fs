﻿namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Core.Mp
open GeneSort.Sorter


[<MessagePackObject>]
type MsasFDto = {
[<Key(0)>] Id: Guid
[<Key(1)>] SortingWidth: int
}

[<MessagePackObject>]
type MsasODto = {
[<Key(0)>] Id: Guid
[<Key(1)>] SeedPermutation: PermutationDto
[<Key(2)>] MaxOrbit: int
}

[<MessagePackObject>]
type SorterTestModelDto = {
[<Key(0)>] Kind: int // 0 = MsasF, 1 = MsasO
[<Key(1)>] MsasF: MsasFDto // used when Kind=0
[<Key(2)>] MsasO: MsasODto // used when Kind=1
}

module MsasFDtoConv =

    let fromDomain (m: MsasF) : MsasFDto =
        {
            Id = %m.Id
            SortingWidth = %m.SortingWidth
        }

    let toDomain (dto: MsasFDto) : MsasF =
        // Id is deterministic from SortingWidth via MsasF.create; dto.Id is informational
        MsasF.create (dto.SortingWidth |> UMX.tag<sortingWidth>)


module MsasODtoConv =

    let private getMaxOrbit (m: MsasO) : int =
        let p = m.GetType().GetProperty("maxOrbit", Reflection.BindingFlags.NonPublic ||| Reflection.BindingFlags.Instance)
        p.GetValue(m) :?> int

    let toDto (m: MsasO) : MsasODto =
        {
            Id = %m.Id
            SeedPermutation = m.SeedPermutation |> PermutationDto.fromDomain
            MaxOrbit = getMaxOrbit m
        }

    let fromDto (dto: MsasODto) : MsasO =
        let perm = dto.SeedPermutation |> PermutationDto.toDomain |> Result.toOption |> Option.get
        MsasO.create perm dto.MaxOrbit


module SorterTestModelDto =

    let fromDomain (dto: SorterTestModelDto) : sortableTestModel =
        match dto.Kind with
        | 0 -> dto.MsasF |> MsasFDtoConv.toDomain |> sortableTestModel.MsasF
        | 1 -> dto.MsasO |> MsasODtoConv.fromDto |> sortableTestModel.MsasO
        | k -> failwithf "Unknown SorterTestModelDto.Kind = %d" k


    let toDomain (m: sortableTestModel) : SorterTestModelDto =
        match m with
        | sortableTestModel.MsasF msasF ->
            {
                Kind = 0
                MsasF = msasF |> MsasFDtoConv.fromDomain
                MsasO = Unchecked.defaultof<MsasODto>
            }
        | sortableTestModel.MsasO msasO ->
            {
                Kind = 1
                MsasF = Unchecked.defaultof<MsasFDto>
                MsasO = msasO |> MsasODtoConv.toDto
            }

