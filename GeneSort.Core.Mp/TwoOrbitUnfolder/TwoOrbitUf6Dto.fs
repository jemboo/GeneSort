namespace GeneSort.Core.Mp.TwoOrbitUnfolder


open System
open FSharp.UMX
open GeneSort.Core
open MessagePack

[<MessagePackObject>]
type TwoOrbitUf6DTO =
    { [<Key(0)>] SeedType: Seed6TwoOrbitType
      [<Key(1)>] TwoOrbitUfSteps: TwoOrbitUfStepDTO list }
    
    static member Create(seedType: Seed6TwoOrbitType, twoOrbitUnfolderSteps: TwoOrbitUfStepDTO list) : Result<TwoOrbitUf6DTO, string> =
        if List.isEmpty twoOrbitUnfolderSteps then
            Error "TwoOrbitUnfolderSteps list cannot be empty"
        else
            let computedOrder = 6 * (MathUtils.integerPower 2 (List.length twoOrbitUnfolderSteps))
            if twoOrbitUnfolderSteps |> List.exists (fun step -> step.Order < 4 || step.Order % 2 <> 0) then
                Error $"All TwoOrbitUfStep orders must be at least 4 and even"
            else
                Ok { SeedType = seedType
                     TwoOrbitUfSteps = twoOrbitUnfolderSteps }

module TwoOrbitUf6DTO =
    type TwoOrbitUf6DTOError =
        | EmptyTwoOrbitUnfolderSteps of string
        | InvalidStepOrder of string
        | StepConversionError of TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError

    let toTwoOrbitUf6DTO (tou: TwoOrbitUf6) : TwoOrbitUf6DTO =
        { SeedType = tou.Seed6TwoOrbitType
          TwoOrbitUfSteps = tou.TwoOrbitUnfolderSteps |> List.map TwoOrbitUnfolderStepDTO.toTwoOrbitUnfolderStepDTO }

    let toTwoOrbitUf6 (dto: TwoOrbitUf6DTO) : Result<TwoOrbitUf6, TwoOrbitUf6DTOError> =
        let stepsResult = 
            dto.TwoOrbitUfSteps 
            |> List.map TwoOrbitUnfolderStepDTO.toTwoOrbitUnfolderStep
            |> List.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok step -> Ok (arr @ [step])
                | Ok _, Error e -> Error (StepConversionError e)
                | Error e, _ -> Error e
            ) (Ok [])
        
        match stepsResult with
        | Error e -> Error e
        | Ok steps ->
            try
                let tou = TwoOrbitUf6.create dto.SeedType steps
                Ok tou
            with
            | :? ArgumentException as ex when ex.Message.Contains("empty") ->
                Error (EmptyTwoOrbitUnfolderSteps ex.Message)
            | ex ->
                Error (EmptyTwoOrbitUnfolderSteps ex.Message)


    let getOrder (twoOrbitUnfolder: TwoOrbitUf6DTO) : int =
            6 * (MathUtils.integerPower 2 (List.length twoOrbitUnfolder.TwoOrbitUfSteps))