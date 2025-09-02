namespace GeneSort.Core.Mp

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject; Struct>]
type Perm_RsDto =
    { [<Key(0)>] Perm_Si: Perm_SiDto }
    
    static member Create(arr: int array) : Result<Perm_RsDto, string> =
        if arr.Length < 4 then
            Error "Perm_Rs order must be at least 4"
        else if arr.Length % 2 <> 0 then
            Error "Perm_Rs order must be divisible by 2"
        else
            match Perm_SiDto.Create(arr) with
            | Error e -> Error e
            | Ok permSiDto ->
                let permSi = Perm_Si.create arr
                if not (Perm_Si.isReflectionSymmetric permSi) then
                    Error "Invalid Perm_Rs: permutation must be reflection-symmetric"
                else
                    Ok { Perm_Si = permSiDto }


module Perm_RsDto =
    type Perm_RsDtoError =
        | OrderTooSmall of string
        | OrderNotDivisibleByTwo of string
        | NotReflectionSymmetric of string
        | PermSiConversionError of Perm_SiDto.Perm_SiDtoError

    let toPerm_RsDto (permRs: Perm_Rs) : Perm_RsDto =
        { Perm_Si = Perm_SiDto.fromDomain permRs.Perm_Si }

    let toPerm_Rs (dto: Perm_RsDto) : Result<Perm_Rs, Perm_RsDtoError> =
        match Perm_SiDto.toDomain dto.Perm_Si with
        | Error e -> Error (PermSiConversionError e)
        | Ok permSi ->
            try
                let permRs = Perm_Rs.create permSi.Array
                Ok permRs
            with
            | :? ArgumentException as ex when ex.Message.Contains("order must be at least 4") ->
                Error (OrderTooSmall ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("divisible by 2") ->
                Error (OrderNotDivisibleByTwo ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("reflection-symmetric") ->
                Error (NotReflectionSymmetric ex.Message)
            | ex ->
                Error (NotReflectionSymmetric ex.Message) // Fallback for unexpected errors
