
namespace GeneSort.Core.Mp.TwoOrbitUnfolder

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core

[<MessagePackObject; Struct>]
type TwoOrbitUfStepDto =
    { [<Key(0)>] TwoOrbitPairTypes: TwoOrbitPairType array
      [<Key(1)>] Order: int }
    
    static member Create(twoOrbitPairTypes: TwoOrbitPairType array, order: int) : Result<TwoOrbitUfStepDto, string> =
        if order < 4 then
            Error $"Order must be at least 4, got {order}"
        else if order % 2 <> 0 then
            Error $"Order must be even, got {order}"
        else if Array.length twoOrbitPairTypes <> order / 2 then
            Error $"TwoOrbitTypes length ({Array.length twoOrbitPairTypes}) must be order / 2 ({order / 2})"
        else
            Ok { TwoOrbitPairTypes = twoOrbitPairTypes; Order = order }


module TwoOrbitUnfolderStepDto =

    type TwoOrbitUnfolderStepDtoError =
        | InvalidOrder of string
        | NotEvenOrder of string
        | InvalidTwoOrbitTypesLength of string

    let fromDomain (step: TwoOrbitUfStep) : TwoOrbitUfStepDto =
        { TwoOrbitPairTypes = step.TwoOrbitPairTypes
          Order = step.Order }

    let toDomain (dto: TwoOrbitUfStepDto) : Result<TwoOrbitUfStep, TwoOrbitUnfolderStepDtoError> =
        try
            let step = TwoOrbitUfStep.create dto.TwoOrbitPairTypes dto.Order
            Ok step
        with
        | :? ArgumentException as ex when ex.Message.Contains("at least 4") ->
            Error (InvalidOrder ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("even") ->
            Error (NotEvenOrder ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("length") ->
            Error (InvalidTwoOrbitTypesLength ex.Message)
        | ex ->
            Error (InvalidOrder ex.Message)


