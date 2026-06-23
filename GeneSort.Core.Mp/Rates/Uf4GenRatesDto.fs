
namespace GeneSort.Core.Mp.RatesAndOps

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type uf4GenRatesDto =
    { [<Key(0)>] order: int
      [<Key(1)>] seedOpsGenRatesDto: opsGenRatesDto
      [<Key(2)>] opsGenRatesArrayDtos: opsGenRatesArrayDto }

module Uf4GenRatesDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (uf4GenRates: uf4GenRates) : uf4GenRatesDto =
        { order = uf4GenRates.Order
          seedOpsGenRatesDto = OpsGenRatesDto.fromDomain uf4GenRates.SeedOpsGenRates
          opsGenRatesArrayDtos = OpsGenRatesArrayDto.fromDomain uf4GenRates.OpsGenRatesArray }

    let toDomain (dto: uf4GenRatesDto) : uf4GenRates =
        try
            if dto.order < 4 || dto.order % 4 <> 0 then
                failwith $"Order must be at least 4 and divisible by 4, got {dto.order}"
            if dto.opsGenRatesArrayDtos.opsGenRatesDtos.Length <> MathUtils.exactLog2 (dto.order / 4) then
                failwith $"OpsGenRatesArray length ({dto.opsGenRatesArrayDtos.opsGenRatesDtos.Length}) must match log2(order/4) ({MathUtils.exactLog2 (dto.order / 4)})"

            uf4GenRates.create 
                dto.order 
                (OpsGenRatesDto.toDomain dto.seedOpsGenRatesDto) 
                (OpsGenRatesArrayDto.toDomain dto.opsGenRatesArrayDtos)

        with
        | ex -> failwith $"Failed to convert Uf4GenRatesDto: {ex.Message}"