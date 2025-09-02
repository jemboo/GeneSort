namespace GeneSort.Core.Mp.TwoOrbitUnfolder


open System
open FSharp.UMX
open GeneSort.Core
open MessagePack

[<MessagePackObject>]
type TwoOrbitUf6Dto =
    { [<Key(0)>] SeedType: TwoOrbitTripleType
      [<Key(1)>] TwoOrbitUfSteps: TwoOrbitUfStepDto array }
    
    static member Create(seedType: TwoOrbitTripleType, twoOrbitUnfolderSteps: TwoOrbitUfStepDto array) : Result<TwoOrbitUf6Dto, string> =
        if Array.isEmpty twoOrbitUnfolderSteps then
            Error "TwoOrbitUnfolderSteps list cannot be empty"
        else
            let computedOrder = 6 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolderSteps))
            if twoOrbitUnfolderSteps |> Array.exists (fun step -> step.Order < 4 || step.Order % 2 <> 0) then
                Error $"All TwoOrbitUfStep orders must be at least 4 and even"
            else
                Ok { SeedType = seedType
                     TwoOrbitUfSteps = twoOrbitUnfolderSteps }


module TwoOrbitUf6Dto =

    type TwoOrbitUf6DtoError =
        | EmptyTwoOrbitUnfolderSteps of string
        | InvalidStepOrder of string
        | StepConversionError of TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError

    let fromDomain (tou: TwoOrbitUf6) : TwoOrbitUf6Dto =
        { SeedType = tou.Seed6TwoOrbitType
          TwoOrbitUfSteps = tou.TwoOrbitUnfolderSteps |> Array.map TwoOrbitUnfolderStepDto.fromDomain }

    let toDomain (dto: TwoOrbitUf6Dto) : Result<TwoOrbitUf6, TwoOrbitUf6DtoError> =
        let stepsResult = 
            dto.TwoOrbitUfSteps 
            |> Array.map TwoOrbitUnfolderStepDto.toDomain
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


    let getOrder (twoOrbitUnfolder: TwoOrbitUf6Dto) : int =
            6 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolder.TwoOrbitUfSteps))