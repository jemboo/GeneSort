namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject; Struct>]
type permutationDto =
    { [<Key(0)>] intArray: int array }
    
    static member Create(arr: int array) : permutationDto =
        if isNull arr then
            failwith "Array cannot be null"
        else if arr.Length = 0 then
            failwith "Array cannot be empty"
        else
            let n = arr.Length
            let sorted = Array.sort arr
            let isValid = Array.forall2 (fun i v -> i = v) [|0 .. n-1|] sorted
            if not isValid then
                failwith "Invalid permutation: must contain each integer from 0 to n-1 exactly once"
            else
                { intArray = arr }

module PermutationDto =

    let fromDomain (perm: permutation) : permutationDto =
        { intArray = perm.Array }

    let toDomain (dto: permutationDto) : permutation =
        try
            permutation.create dto.intArray
        with
        | ex ->
            failwith ex.Message // Fallback for unexpected errors