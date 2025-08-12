
namespace GeneSort.Core.Mp.TwoOrbitUnfolder

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject; Struct>]
type TwoOrbitUf4Dto =
    { [<Key(0)>] SeedType: TwoOrbitType
      [<Key(1)>] TwoOrbitUfSteps: TwoOrbitUfStepDto array }
    
    static member Create(seedType: TwoOrbitType, twoOrbitUnfolderSteps: TwoOrbitUfStepDto array) : Result<TwoOrbitUf4Dto, string> =
        if Array.isEmpty twoOrbitUnfolderSteps then
            Error "TwoOrbitUnfolderSteps array cannot be empty"
        else
            Ok { SeedType = seedType
                 TwoOrbitUfSteps = twoOrbitUnfolderSteps }

module TwoOrbitUf4Dto =
    type TwoOrbitUf4DtoError =
        | EmptyTwoOrbitUfSteps of string
        | StepConversionError of TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError

    let toTwoOrbitUnfolder4Dto (tou: TwoOrbitUf4) : TwoOrbitUf4Dto =
        { SeedType = tou.TwoOrbitType
          TwoOrbitUfSteps = tou.TwoOrbitUnfolderSteps |> Array.map TwoOrbitUnfolderStepDto.toTwoOrbitUnfolderStepDto }

    let toTwoOrbitUnfolder4 (dto: TwoOrbitUf4Dto) : Result<TwoOrbitUf4, TwoOrbitUf4DtoError> =
        let stepsResult = 
            dto.TwoOrbitUfSteps 
            |> Array.map TwoOrbitUnfolderStepDto.toTwoOrbitUnfolderStep
            |> Array.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok step -> Ok (arr @ [step])
                | Ok _, Error e -> Error (StepConversionError e)
                | Error e, _ -> Error e
            ) (Ok [])
        
        match stepsResult with
        | Error e -> Error e
        | Ok steps ->
            try
                let tou = TwoOrbitUf4.create dto.SeedType (steps |> List.toArray)
                Ok tou
            with
            | :? ArgumentException as ex when ex.Message.Contains("empty") ->
                Error (EmptyTwoOrbitUfSteps ex.Message)
            | ex ->
                Error (EmptyTwoOrbitUfSteps ex.Message)


    let getOrder (twoOrbitUnfolder: TwoOrbitUf4Dto) : int =
            4 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolder.TwoOrbitUfSteps))