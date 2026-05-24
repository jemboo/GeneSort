namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open GeneSort.Core
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf4MutationRatesDto =
    { [<Key(0)>] Order: int
      [<Key(1)>] SeedOpsTransitionRates: OpsTransitionRatesDto
      [<Key(2)>] TwoOrbitPairOpsTransitionRates: OpsTransitionRatesArrayDto }

module Uf4MutationRatesDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (uf4Mr: uf4MutationRates) : Uf4MutationRatesDto =
        { Order = uf4Mr.Order
          SeedOpsTransitionRates = OpsTransitionRatesDto.fromDomain uf4Mr.SeedOpsTransitionRates
          TwoOrbitPairOpsTransitionRates = OpsTransitionRatesArrayDto.fromDomain uf4Mr.TwoOrbitPairOpsTransitionRates }

    let toDomain (dto: Uf4MutationRatesDto) : uf4MutationRates =
        try
            if dto.Order < 4 || dto.Order % 4 <> 0 then
                failwith $"Order must be at least 4 and divisible by 4, got {dto.Order}"
            if dto.TwoOrbitPairOpsTransitionRates.Rates.Length <> MathUtils.exactLog2 (dto.Order / 4) && 
                        dto.Order <> 4 then
                failwith $"TwoOrbitPairOpsTransitionRates length ({dto.TwoOrbitPairOpsTransitionRates.Rates.Length}) 
                            must match log2(order/4) ({MathUtils.exactLog2 (dto.Order / 4)})"

            uf4MutationRates.create dto.Order
                 (OpsTransitionRatesDto.toDomain dto.SeedOpsTransitionRates)
                 (OpsTransitionRatesArrayDto.toDomain dto.TwoOrbitPairOpsTransitionRates)
        with
        | ex -> failwith $"Failed to convert Uf4MutationRatesDto: {ex.Message}"