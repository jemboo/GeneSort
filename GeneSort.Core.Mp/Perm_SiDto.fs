namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack


[<MessagePackObject; Struct>]
type Perm_SiDto =
    { [<Key(0)>] Permutation: PermutationDto }
    
    static member Create(arr: int array) : Result<Perm_SiDto, string> =
        match PermutationDto.Create(arr) with
        | Error e -> Error e
        | Ok permDto ->
            let perm = Permutation.create arr
            if not (Permutation.isSelfInverse perm) then
                Error "Invalid: permutation must be self-inverse"
            else
                Ok { Permutation = permDto }

module Perm_SiDto =
    type Perm_SiDtoError =
        | NullArray of string
        | EmptyArray of string
        | InvalidPermutation of string
        | NotSelfInverse of string
        | PermutationConversionError of PermutationDto.PermutationDtoError

    let toPerm_SiDto (permSi: Perm_Si) : Perm_SiDto =
        { Permutation = PermutationDto.toPermutationDto permSi.Permutation }

    let toPerm_Si (dto: Perm_SiDto) : Result<Perm_Si, Perm_SiDtoError> =
        match PermutationDto.toPermutation dto.Permutation with
        | Error e -> Error (PermutationConversionError e)
        | Ok perm ->
            try
                let permSi = Perm_Si.create perm.Array
                Ok permSi
            with
            | :? ArgumentException as ex when ex.Message.Contains("array must contain items") ->
                Error (EmptyArray ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("self-inverse") ->
                Error (NotSelfInverse ex.Message)
            | ex ->
                Error (InvalidPermutation ex.Message) // Fallback for unexpected errors