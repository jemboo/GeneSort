namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type SorterTestSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] sorterTestDtos: sorterTestDto[]
}

module SorterTestSetDto =
    let toDto (sorterTestSet: SorterTestSet) : SorterTestSetDto =
        { Id = %sorterTestSet.Id
          sorterTestDtos = sorterTestSet.sorterTests |> Array.map SorterTestDto.toDto }

    let fromDto (dto: SorterTestSetDto) : SorterTestSet =
        let sorterTests = dto.sorterTestDtos |> Array.map SorterTestDto.fromDto
        let id = UMX.tag<sorterTestSetId> dto.Id
        SorterTestSet.create id sorterTests
