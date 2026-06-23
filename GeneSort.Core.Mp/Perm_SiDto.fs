namespace GeneSort.Core.Mp

open GeneSort.Core
open MessagePack


[<MessagePackObject; Struct>]
type permSiDto =
    { [<Key(0)>] permutationDto: permutationDto }
    
    static member Create(arr: int array) : permSiDto =
        let permDto = permutationDto.Create(arr)
        { permutationDto = permDto }

module PermSiDto =

    let fromDomain (permSi: permSi) : permSiDto =
        { permutationDto = PermutationDto.fromDomain permSi.Permutation }

    let toDomain (dto: permSiDto) : permSi =
        let perm = PermutationDto.toDomain dto.permutationDto
        permSi.create perm.Array