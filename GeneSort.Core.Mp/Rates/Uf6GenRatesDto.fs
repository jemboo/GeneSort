namespace GeneSort.Core.Mp.RatesAndOps

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf6GenRatesDto =
    { [<Key(0)>] Order: int
      [<Key(1)>] SeedGenRatesUf6: Seed6GenRatesDto
      [<Key(2)>] OpsGenRatesArray: OpsGenRatesArrayDto }

module Uf6GenRatesDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toUf6GenRatesDto (uf6GenRates: Uf6GenRates) : Uf6GenRatesDto =
        { Order = uf6GenRates.order
          SeedGenRatesUf6 = Seed6GenRatesDto.fromSeed6GenRates uf6GenRates.seedGenRatesUf6
          OpsGenRatesArray = OpsGenRatesArrayDto.fromOpsGenRatesArray uf6GenRates.opsGenRatesArray }

    let fromUf6GenRatesDto (dto: Uf6GenRatesDto) : Uf6GenRates =
        try
            if dto.Order < 6 || dto.Order % 6 <> 0 then
                failwith $"Order must be at least 6 and divisible by 6, got {dto.Order}"
            if dto.OpsGenRatesArray.Rates.Length <> MathUtils.exactLog2 (dto.Order / 6) then
                failwith $"OpsGenRatesArray length ({dto.OpsGenRatesArray.Rates.Length}) must match log2(order/6) ({MathUtils.exactLog2 (dto.Order / 6)})"
            { order = dto.Order
              seedGenRatesUf6 = Seed6GenRatesDto.toSeed6GenRates dto.SeedGenRatesUf6
              opsGenRatesArray = OpsGenRatesArrayDto.toOpsGenRatesArray dto.OpsGenRatesArray }
        with
        | ex -> failwith $"Failed to convert Uf6GenRatesDto: {ex.Message}"