
namespace GeneSort.Core.Mp.TwoOrbitUnfolder

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core

[<MessagePackObject; Struct>]
type TwoOrbitUfStepDTO =
    { [<Key(0)>] TwoOrbitTypes: TwoOrbitType list
      [<Key(1)>] Order: int }
    
    static member Create(twoOrbitTypes: TwoOrbitType list, order: int) : Result<TwoOrbitUfStepDTO, string> =
        if order < 4 then
            Error $"Order must be at least 4, got {order}"
        else if order % 2 <> 0 then
            Error $"Order must be even, got {order}"
        else if List.length twoOrbitTypes <> order / 2 then
            Error $"TwoOrbitTypes length ({List.length twoOrbitTypes}) must be order / 2 ({order / 2})"
        else
            Ok { TwoOrbitTypes = twoOrbitTypes; Order = order }

module TwoOrbitUnfolderStepDTO =
    type TwoOrbitUnfolderStepDTOError =
        | InvalidOrder of string
        | NotEvenOrder of string
        | InvalidTwoOrbitTypesLength of string

    let toTwoOrbitUnfolderStepDTO (step: TwoOrbitUfStep) : TwoOrbitUfStepDTO =
        { TwoOrbitTypes = step.TwoOrbitTypes
          Order = step.Order }

    let toTwoOrbitUnfolderStep (dto: TwoOrbitUfStepDTO) : Result<TwoOrbitUfStep, TwoOrbitUnfolderStepDTOError> =
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


