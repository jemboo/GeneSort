
namespace GeneSort.Core.Mp.RatesAndOps

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf4GenRatesDto =
    { [<Key(0)>] Order: int
      [<Key(1)>] SeedOpsGenRates: OpsGenRatesDto
      [<Key(2)>] OpsGenRatesArray: OpsGenRatesArrayDto }

module Uf4GenRatesDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toUf4GenRatesDto (uf4GenRates: Uf4GenRates) : Uf4GenRatesDto =
        { Order = uf4GenRates.order
          SeedOpsGenRates = OpsGenRatesDto.fromOpsGenRates uf4GenRates.seedOpsGenRates
          OpsGenRatesArray = OpsGenRatesArrayDto.fromOpsGenRatesArray uf4GenRates.opsGenRatesArray }

    let fromUf4GenRatesDto (dto: Uf4GenRatesDto) : Uf4GenRates =
        try
            if dto.Order < 4 || dto.Order % 4 <> 0 then
                failwith $"Order must be at least 4 and divisible by 4, got {dto.Order}"
            if dto.OpsGenRatesArray.Rates.Length <> MathUtils.exactLog2 (dto.Order / 4) then
                failwith $"OpsGenRatesArray length ({dto.OpsGenRatesArray.Rates.Length}) must match log2(order/4) ({MathUtils.exactLog2 (dto.Order / 4)})"
            { order = dto.Order
              seedOpsGenRates = OpsGenRatesDto.toOpsGenRates dto.SeedOpsGenRates
              opsGenRatesArray = OpsGenRatesArrayDto.toOpsGenRatesArray dto.OpsGenRatesArray }
        with
        | ex -> failwith $"Failed to convert Uf4GenRatesDto: {ex.Message}"