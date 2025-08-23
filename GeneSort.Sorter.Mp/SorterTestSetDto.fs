namespace GeneSort.Sorter.Mp

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open MessagePack



[<MessagePackObject>]
type SorterTestSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTests: SorterTestDto[]
}

module SorterTestSetDto =
    let toDto (sorterTestSet: SorterTestSet) : SorterTestSetDto =
        { Id = %sorterTestSet.Id
          SorterTests = sorterTestSet.sorterTests |> Array.map SorterTestDto.toDto }

    let fromDto (dto: SorterTestSetDto) : SorterTestSet =
        let sorterTests = dto.SorterTests |> Array.map SorterTestDto.fromDto
        let id = UMX.tag<sorterTestSetId> dto.Id
        SorterTestSet.create id sorterTests
