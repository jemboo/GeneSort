namespace GeneSort.Core.MessagePack.TwoOrbitUnfolder


open System
open FSharp.UMX
open EvoMergeSort.Core
open MessagePack
open GeneSort.Core.MessagePack
open EvoMergeSort.Core.Uf6Seeds
open GeneSort.Core.MessagePack.TwoOrbit
open GeneSort.Core.MessagePack.TwoOrbit.Unfolder6Seeds


[<MessagePackObject>]
type TwoOrbitUf4GenRatesDTO =
    { [<Key(0)>] Order: int
      [<Key(1)>] SeedGenRates: {| Ortho: float; Para: float; SelfRefl: float |}
      [<Key(2)>] TwoOrbitTypeGenRatesList: TwoOrbitTypeGenRatesDTO list }
    
    static member Create(order: int, seedGenRates: {| Ortho: float; Para: float; SelfRefl: float |}, twoOrbitTypeGenRatesList: TwoOrbitTypeGenRatesDTO list) 
        : Result<TwoOrbitUf4GenRatesDTO, string> =
        if order < 4 then
            Error $"Order must be at least 4, got {order}"
        else if order % 2 <> 0 then
            Error $"Order must be even, got {order}"
        else if order % 4 <> 0 then
            Error $"Order must be divisible by 4, got {order}"
        else if seedGenRates.Ortho < 0.0 || seedGenRates.Ortho > 1.0 then
            Error $"SeedGenRates.Ortho must be between 0 and 1, got {seedGenRates.Ortho}"
        else if seedGenRates.Para < 0.0 || seedGenRates.Para > 1.0 then
            Error $"SeedGenRates.Para must be between 0 and 1, got {seedGenRates.Para}"
        else if seedGenRates.SelfRefl < 0.0 || seedGenRates.SelfRefl > 1.0 then
            Error $"SeedGenRates.SelfRefl must be between 0 and 1, got {seedGenRates.SelfRefl}"
        else if seedGenRates.Ortho + seedGenRates.Para + seedGenRates.SelfRefl > 1.0 then
            Error $"Sum of SeedGenRates must not exceed 1.0, got Ortho={seedGenRates.Ortho}, Para={seedGenRates.Para}, SelfRefl={seedGenRates.SelfRefl}"
        else
            try
                let expectedLength = MathUtils.exactLog2(order / 4)
                if twoOrbitTypeGenRatesList.Length <> expectedLength then
                    Error $"TwoOrbitTypeGenRatesList length ({twoOrbitTypeGenRatesList.Length}) must equal log2(order/4) ({expectedLength})"
                else
                    Ok { Order = order
                         SeedGenRates = seedGenRates
                         TwoOrbitTypeGenRatesList = twoOrbitTypeGenRatesList }
            with
            | :? ArgumentException as ex ->
                Error ex.Message

module TwoOrbitUf4GenRatesDTO =
    type TwoOrbitUf4GenRatesDTOError =
        | InvalidOrder of string
        | NotEvenOrder of string
        | NotDivisibleByFour of string
        | InvalidSeedGenRatesOrtho of string
        | InvalidSeedGenRatesPara of string
        | InvalidSeedGenRatesSelfRefl of string
        | InvalidSeedGenRatesSum of string
        | InvalidTwoOrbitTypeGenRatesLength of string
        | TwoOrbitTypeGenRatesConversionError of TwoOrbitTypeGenRatesDTO.TwoOrbitTypeGenRatesDTOError

    let toTwoOrbitUnfolder4GenRatesDTO (genRates: Uf4GenRates) : TwoOrbitUf4GenRatesDTO =
        { Order = genRates.order
          SeedGenRates = {| Ortho = genRates.seedGenRatesUf4.Ortho
                            Para = genRates.seedGenRatesUf4.Para
                            SelfRefl = genRates.seedGenRatesUf4.SelfRefl |}
          TwoOrbitTypeGenRatesList = genRates.opsGenRatesList |> List.map TwoOrbitTypeGenRatesDTO.toTwoOrbitTypeGenRatesDTO }

    let toTwoOrbitUnfolder4GenRates (dto: TwoOrbitUf4GenRatesDTO) : Result<Uf4GenRates, TwoOrbitUf4GenRatesDTOError> =
        let genRatesListResult = 
            dto.TwoOrbitTypeGenRatesList 
            |> List.map TwoOrbitTypeGenRatesDTO.toTwoOrbitTypeGenRates
            |> List.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok gr -> Ok (arr @ [gr])
                | Ok _, Error e -> Error (TwoOrbitTypeGenRatesConversionError e)
                | Error e, _ -> Error e
            ) (Ok [])
        
        match genRatesListResult with
        | Error e -> Error e
        | Ok genRatesList ->
            try
                let seedGenRates: SeedGenRatesUf4 = 
                        { Ortho = dto.SeedGenRates.Ortho
                          Para = dto.SeedGenRates.Para
                          SelfRefl = dto.SeedGenRates.SelfRefl }
                let twoOrbitUnfolder4GenRates = 
                    { order = dto.Order
                      seedGenRatesUf4 = seedGenRates
                      opsGenRatesList = genRatesList }
                Ok twoOrbitUnfolder4GenRates
            with
            | :? ArgumentException as ex when ex.Message.Contains("Order") && not (ex.Message.Contains("even")) && not (ex.Message.Contains("divisible by 4")) ->
                Error (InvalidOrder ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("even") ->
                Error (NotEvenOrder ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("divisible by 4") ->
                Error (NotDivisibleByFour ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("Ortho") ->
                Error (InvalidSeedGenRatesOrtho ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("Para") ->
                Error (InvalidSeedGenRatesPara ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("SelfRefl") ->
                Error (InvalidSeedGenRatesSelfRefl ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("sum") ->
                Error (InvalidSeedGenRatesSum ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("log2") ->
                Error (InvalidTwoOrbitTypeGenRatesLength ex.Message)
            | ex ->
                Error (InvalidOrder ex.Message)



[<MessagePackObject>]
type TwoOrbitUf6GenRatesDTO =
    { [<Key(0)>] Order: int
      [<Key(1)>] SeedGenRartes: Order6GenRatesDTO
      [<Key(2)>] TwoOrbitTypeGenRatesList: TwoOrbitTypeGenRatesDTO list }
    
    static member Create(order: int, seedGenRartes: Order6GenRatesDTO, twoOrbitTypeGenRatesList: TwoOrbitTypeGenRatesDTO list) 
        : Result<TwoOrbitUf6GenRatesDTO, string> =
        if order < 6 then
            Error $"Order must be at least 6, got {order}"
        else if order % 6 <> 0 then
            Error $"Order must be divisible by 6, got {order}"
        else
            try
                let expectedLength = MathUtils.exactLog2(order / 6)
                if twoOrbitTypeGenRatesList.Length <> expectedLength then
                    Error $"TwoOrbitTypeGenRatesList length ({twoOrbitTypeGenRatesList.Length}) must equal log2(order/6) ({expectedLength})"
                else
                    Ok { Order = order
                         SeedGenRartes = seedGenRartes
                         TwoOrbitTypeGenRatesList = twoOrbitTypeGenRatesList }
            with
            | :? ArgumentException as ex ->
                Error ex.Message


module TwoOrbitUf6GenRatesDTO =
    type TwoOrbitUf6GenRatesDTOError =
        | InvalidOrder of string
        | NotDivisibleBySix of string
        | NullTwoOrbitTypeGenRatesList of string
        | InvalidTwoOrbitTypeGenRatesLength of string
        | SeedGenRartesConversionError of Order6GenRatesDTO.Order6GenRatesDTOError
        | TwoOrbitTypeGenRatesConversionError of TwoOrbitTypeGenRatesDTO.TwoOrbitTypeGenRatesDTOError

    let toTwoOrbitUf6GenRatesDTO (genRates: Uf6GenRates) : TwoOrbitUf6GenRatesDTO =
        { Order = genRates.order
          SeedGenRartes = Order6GenRatesDTO.toOrder6GenRatesDTO genRates.seedGenRatesUf6
          TwoOrbitTypeGenRatesList = genRates.twoOrbitTypeGenRatesList |> List.map TwoOrbitTypeGenRatesDTO.toTwoOrbitTypeGenRatesDTO }

    let toTwoOrbitUf6GenRates (dto: TwoOrbitUf6GenRatesDTO) : Result<Uf6GenRates, TwoOrbitUf6GenRatesDTOError> =
        let seedGenRartesResult = Order6GenRatesDTO.toOrder6GenRates dto.SeedGenRartes
        let genRatesListResult = 
            dto.TwoOrbitTypeGenRatesList 
            |> List.map TwoOrbitTypeGenRatesDTO.toTwoOrbitTypeGenRates
            |> List.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok gr -> Ok (arr @ [gr])
                | Ok _, Error e -> Error (TwoOrbitTypeGenRatesConversionError e)
                | Error e, _ -> Error e
            ) (Ok [])
        
        match seedGenRartesResult, genRatesListResult with
        | Error e, _ -> Error (SeedGenRartesConversionError e)
        | _, Error e -> Error e
        | Ok seedGenRartes, Ok genRatesList ->
            try
                let genRates = 
                    { order = dto.Order
                      seedGenRatesUf6 = seedGenRartes
                      twoOrbitTypeGenRatesList = genRatesList }
                Ok genRates
            with
            | :? ArgumentException as ex when ex.Message.Contains("Order") && not (ex.Message.Contains("divisible by 6")) ->
                Error (InvalidOrder ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("divisible by 6") ->
                Error (NotDivisibleBySix ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("log2") ->
                Error (InvalidTwoOrbitTypeGenRatesLength ex.Message)
            | ex ->
                Error (InvalidOrder ex.Message)








