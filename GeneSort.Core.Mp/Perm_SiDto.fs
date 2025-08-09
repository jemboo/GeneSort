namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack


[<MessagePackObject; Struct>]
type Perm_SiDTO =
    { [<Key(0)>] Permutation: PermutationDTO }
    
    static member Create(arr: int array) : Result<Perm_SiDTO, string> =
        match PermutationDTO.Create(arr) with
        | Error e -> Error e
        | Ok permDTO ->
            let perm = Permutation.create arr
            if not (Permutation.isSelfInverse perm) then
                Error "Invalid: permutation must be self-inverse"
            else
                Ok { Permutation = permDTO }

module Perm_SiDTO =
    type Perm_SiDTOError =
        | NullArray of string
        | EmptyArray of string
        | InvalidPermutation of string
        | NotSelfInverse of string
        | PermutationConversionError of PermutationDTO.PermutationDTOError

    let toPerm_SiDTO (permSi: Perm_Si) : Perm_SiDTO =
        { Permutation = PermutationDTO.toPermutationDTO permSi.Permutation }

    let toPerm_Si (dto: Perm_SiDTO) : Result<Perm_Si, Perm_SiDTOError> =
        match PermutationDTO.toPermutation dto.Permutation with
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