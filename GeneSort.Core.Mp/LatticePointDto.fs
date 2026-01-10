namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject>]
type LatticePointDto =
    { [<Key(0)>] Coords: int[] }

module LatticePointDto =
    let fromDomain (lp: latticePoint) : LatticePointDto =
        { Coords = lp.Coords }

    let toDomain (dto: LatticePointDto) : latticePoint =
        latticePoint.create dto.Coords