namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sorterIntTestSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTests: sorterIntTestDto[]
}

module SorterIntTestSetDto =

    let fromDomain (sits: sorterIntTestSet) : sorterIntTestSetDto =
        { Id = %sits.Id
          SorterTests = sits.sorterTests |> Array.map SorterIntTestDto.fromDomain }

    let toDomain (dto: sorterIntTestSetDto) : sorterIntTestSet =
        sorterIntTestSet.create
            (UMX.tag<sorterTestSetId> dto.Id)
            (dto.SorterTests |> Array.map SorterIntTestDto.toDomain)
