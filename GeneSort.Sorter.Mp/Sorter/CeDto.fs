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

    let fromDomain (ce: ce) : CeDto =
        { Index = Ce.toIndex ce }

    let toDomain (dto: CeDto) : ce =
        Ce.fromIndex dto.Index