namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject>]
type latticePointDto =
    { [<Key(0)>] coords: int[] }

module LatticePointDto =
    let fromDomain (lp: latticePoint) : latticePointDto =
        { coords = lp.Coords }

    let toDomain (dto: latticePointDto) : latticePoint =
        latticePoint.create dto.coords