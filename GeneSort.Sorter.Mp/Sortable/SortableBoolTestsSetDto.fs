namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


[<MessagePackObject>]
type sortableBoolTestsSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortableBoolTestsDtos: sortableBoolTestsDto[]
}

module SortableBoolTestsSetDto =

    let fromDomain (sbts: sortableBoolTestsSet) : sortableBoolTestsSetDto =
        { Id = %sbts.Id
          SortableBoolTestsDtos = sbts.sortableTests |> Array.map SortableBoolTestsDto.fromDomain }

    let toDomain (dto: sortableBoolTestsSetDto) : sortableBoolTestsSet =
        sortableBoolTestsSet.create
            (UMX.tag<sortableTestsSetId> dto.Id)
            (dto.SortableBoolTestsDtos |> Array.map SortableBoolTestsDto.toDomain)


