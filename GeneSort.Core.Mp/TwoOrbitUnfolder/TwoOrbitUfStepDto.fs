
namespace GeneSort.Core.Mp.TwoOrbitUnfolder

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core

[<MessagePackObject; Struct>]
type TwoOrbitUfStepDto =
    { [<Key(0)>] TwoOrbitTypes: TwoOrbitType array
      [<Key(1)>] Order: int }
    
    static member Create(twoOrbitTypes: TwoOrbitType array, order: int) : Result<TwoOrbitUfStepDto, string> =
        if order < 4 then
            Error $"Order must be at least 4, got {order}"
        else if order % 2 <> 0 then
            Error $"Order must be even, got {order}"
        else if Array.length twoOrbitTypes <> order / 2 then
            Error $"TwoOrbitTypes length ({Array.length twoOrbitTypes}) must be order / 2 ({order / 2})"
        else
            Ok { TwoOrbitTypes = twoOrbitTypes; Order = order }


module TwoOrbitUnfolderStepDto =
    type TwoOrbitUnfolderStepDtoError =
        | InvalidOrder of string
        | NotEvenOrder of string
        | InvalidTwoOrbitTypesLength of string

    let toTwoOrbitUnfolderStepDto (step: TwoOrbitUfStep) : TwoOrbitUfStepDto =
        { TwoOrbitTypes = step.TwoOrbitTypes
          Order = step.Order }

    let toTwoOrbitUnfolderStep (dto: TwoOrbitUfStepDto) : Result<TwoOrbitUfStep, TwoOrbitUnfolderStepDtoError> =
        try
            let step = TwoOrbitUfStep.create dto.TwoOrbitTypes dto.Order
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


