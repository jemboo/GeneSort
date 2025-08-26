namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


[<MessagePackObject>]
type sorterBoolTestSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTests: sorterBoolTestDto[]
}

module SorterBoolTestSetDto =
    let toDto (sbts: sorterBoolTestSet) : sorterBoolTestSetDto =
        { Id = %sbts.Id
          SorterTests = sbts.sorterTests |> Array.map SorterBoolTestDto.toDto }

    let fromDto (dto: sorterBoolTestSetDto) : sorterBoolTestSet =
        sorterBoolTestSet.create
            (UMX.tag<sorterTestSetId> dto.Id)
            (dto.SorterTests |> Array.map SorterBoolTestDto.fromDto)


