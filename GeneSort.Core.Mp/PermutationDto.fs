namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject; Struct>]
type PermutationDTO =
    { [<Key(0)>] Array: int array }
    
    static member Create(arr: int array) : Result<PermutationDTO, string> =
        if isNull arr then
            Error "Array cannot be null"
        else if arr.Length = 0 then
            Error "Array cannot be empty"
        else
            let n = arr.Length
            let sorted = Array.sort arr
            let isValid = Array.forall2 (fun i v -> i = v) [|0 .. n-1|] sorted
            if not isValid then
                Error "Invalid permutation: must contain each integer from 0 to n-1 exactly once"
            else
                Ok { Array = arr }

module PermutationDTO =
    type PermutationDTOError =
        | NullArray of string
        | EmptyArray of string
        | InvalidPermutation of string

    let toPermutationDTO (perm: Permutation) : PermutationDTO =
        { Array = perm.Array }

    let toPermutation (dto: PermutationDTO) : Result<Permutation, PermutationDTOError> =
        try
            let perm = Permutation.create dto.Array
            Ok perm
        with
        | :? ArgumentException as ex when ex.Message.Contains("empty") ->
            Error (EmptyArray ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("Invalid permutation") ->
            Error (InvalidPermutation ex.Message)
        | ex ->
            Error (InvalidPermutation ex.Message) // Fallback for unexpected errors