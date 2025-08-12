
namespace GeneSort.Core.Mp.TwoOrbitUnfolder

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject; Struct>]
type TwoOrbitUf4DTO =
    { [<Key(0)>] SeedType: TwoOrbitType
      [<Key(1)>] TwoOrbitUfSteps: TwoOrbitUfStepDTO array }
    
    static member Create(seedType: TwoOrbitType, twoOrbitUnfolderSteps: TwoOrbitUfStepDTO array) : Result<TwoOrbitUf4DTO, string> =
        if Array.isEmpty twoOrbitUnfolderSteps then
            Error "TwoOrbitUnfolderSteps list cannot be empty"
        else
            Ok { SeedType = seedType
                 TwoOrbitUfSteps = twoOrbitUnfolderSteps }

module TwoOrbitUf4DTO =
    type TwoOrbitUf4DTOError =
        | EmptyTwoOrbitUfSteps of string
        | StepConversionError of TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError

    let toTwoOrbitUnfolder4DTO (tou: TwoOrbitUf4) : TwoOrbitUf4DTO =
        { SeedType = tou.TwoOrbitType
          TwoOrbitUfSteps = tou.TwoOrbitUnfolderSteps |> Array.map TwoOrbitUnfolderStepDTO.toTwoOrbitUnfolderStepDTO }

    let toTwoOrbitUnfolder4 (dto: TwoOrbitUf4DTO) : Result<TwoOrbitUf4, TwoOrbitUf4DTOError> =
        let stepsResult = 
            dto.TwoOrbitUfSteps 
            |> Array.map TwoOrbitUnfolderStepDTO.toTwoOrbitUnfolderStep
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


    let getOrder (twoOrbitUnfolder: TwoOrbitUf4DTO) : int =
            4 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolder.TwoOrbitUfSteps))