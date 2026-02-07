namespace GeneSort.Component.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Component
open GeneSort.Component.Sortable


[<MessagePackObject>]
type sortableBoolTestSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortableBoolTestDtos: sortableBoolTestDto[]
}

module SortableBoolTestSetDto =

    let fromDomain (sbts: sortableBoolTestSet) : sortableBoolTestSetDto =
        { Id = %sbts.Id
          SortableBoolTestDtos = sbts.sortableTests |> Array.map SortableBoolTestDto.fromDomain }

    let toDomain (dto: sortableBoolTestSetDto) : sortableBoolTestSet =
        sortableBoolTestSet.create
            (UMX.tag<sortableTestSetId> dto.Id)
            (dto.SortableBoolTestDtos |> Array.map SortableBoolTestDto.toDomain)


