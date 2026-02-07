namespace GeneSort.Component.Mp.Sorter

open GeneSort.Component.Sorter
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