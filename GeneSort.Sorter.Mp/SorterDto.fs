namespace GeneSort.Sorter.Mp

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open MessagePack

[<MessagePackObject>]
type SorterDto = {
    [<Key(0)>]
    SorterId: Guid
    [<Key(1)>]
    Width: int
    [<Key(2)>]
    Ces: CeDto array
}

module SorterDto =
    let toSorterDto (sorter: Sorter) : SorterDto =
        { SorterId = %sorter.SorterId
          Width = %sorter.Width
          Ces = sorter.Ces |> Array.map CeDto.toCeDto }

    let fromSorterDto (dto: SorterDto) : Sorter =
        if dto.SorterId = Guid.Empty then
            failwith "Sorter ID must not be empty"
        if dto.Width < 1 then
            failwith "Width must be at least 1"
        Sorter.create
            (UMX.tag<sorterId> dto.SorterId)
            (UMX.tag<sortingWidth> dto.Width)
            (dto.Ces |> Array.map CeDto.fromCeDto)
