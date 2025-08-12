namespace GeneSort.Core.Mp.TwoOrbitUnfolder


open System
open FSharp.UMX
open GeneSort.Core
open MessagePack

[<MessagePackObject>]
type TwoOrbitUf6DTO =
    { [<Key(0)>] SeedType: TwoOrbitTripleType
      [<Key(1)>] TwoOrbitUfSteps: TwoOrbitUfStepDTO array }
    
    static member Create(seedType: TwoOrbitTripleType, twoOrbitUnfolderSteps: TwoOrbitUfStepDTO array) : Result<TwoOrbitUf6DTO, string> =
        if Array.isEmpty twoOrbitUnfolderSteps then
            Error "TwoOrbitUnfolderSteps list cannot be empty"
        else
            let computedOrder = 6 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolderSteps))
            if twoOrbitUnfolderSteps |> Array.exists (fun step -> step.Order < 4 || step.Order % 2 <> 0) then
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
          TwoOrbitUfSteps = tou.TwoOrbitUnfolderSteps |> Array.map TwoOrbitUnfolderStepDTO.toTwoOrbitUnfolderStepDTO }

    let toTwoOrbitUf6 (dto: TwoOrbitUf6DTO) : Result<TwoOrbitUf6, TwoOrbitUf6DTOError> =
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
                let tou = TwoOrbitUf6.create dto.SeedType (steps |> List.toArray)
                Ok tou
            with
            | :? ArgumentException as ex when ex.Message.Contains("empty") ->
                Error (EmptyTwoOrbitUnfolderSteps ex.Message)
            | ex ->
                Error (EmptyTwoOrbitUnfolderSteps ex.Message)


    let getOrder (twoOrbitUnfolder: TwoOrbitUf6DTO) : int =
            6 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolder.TwoOrbitUfSteps))