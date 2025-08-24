namespace GeneSort.Sorter.Mp.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open MessagePack


[<MessagePackObject>]
type CeDto = {
    [<Key(0)>]
    Index: int
}

module CeDto =
    let toCeDto (ce: Ce) : CeDto =
        { Index = Ce.toIndex ce }

    let fromCeDto (dto: CeDto) : Ce =
        Ce.fromIndex dto.Index