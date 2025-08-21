namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
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
type SortableArrayDto = {
[<Key(0)>] Kind: int // 0 = Bools, 1 = Ints
[<Key(1)>] BoolValues: bool[] // used when Kind=0
[<Key(2)>] IntValues: int[] // used when Kind=1
}

[<MessagePackObject>]
type SortableArraySetDto = {
[<Key(0)>] Id: Guid
[<Key(1)>] Arrays: SortableArrayDto[]
}

[<MessagePackObject>]
type SortableSetModelDto = {
[<Key(0)>] Kind: int // 0 = MsasF, 1 = MsasO
[<Key(1)>] MsasF: MsasFDto // used when Kind=0
[<Key(2)>] MsasO: MsasODto // used when Kind=1
}

module MsasFDtoConv =
    let toDto (m: MsasF) : MsasFDto =
        {
            Id = %m.Id
            SortingWidth = %m.SortingWidth
        }

    let fromDto (dto: MsasFDto) : MsasF =
        // Id is deterministic from SortingWidth via MsasF.create; dto.Id is informational
        MsasF.create (dto.SortingWidth |> UMX.tag<sortingWidth>)


module MsasODtoConv =

    let private getMaxOrbit (m: MsasO) : int =
        let p = m.GetType().GetProperty("maxOrbit", Reflection.BindingFlags.NonPublic ||| Reflection.BindingFlags.Instance)
        p.GetValue(m) :?> int

    let toDto (m: MsasO) : MsasODto =
        {
            Id = %m.Id
            SeedPermutation = m.SeedPermutation |> PermutationDto.toPermutationDto
            MaxOrbit = getMaxOrbit m
        }

    let fromDto (dto: MsasODto) : MsasO =
        let perm = dto.SeedPermutation |> PermutationDto.toPermutation |> Result.toOption |> Option.get
        MsasO.create perm dto.MaxOrbit


module SortableSetModelDto =

    let fromDto (dto: SortableSetModelDto) : SortableSetModel =
        match dto.Kind with
        | 0 -> dto.MsasF |> MsasFDtoConv.fromDto |> SortableSetModel.MsasF
        | 1 -> dto.MsasO |> MsasODtoConv.fromDto |> SortableSetModel.MsasO
        | k -> failwithf "Unknown SortableSetModelDto.Kind = %d" k

