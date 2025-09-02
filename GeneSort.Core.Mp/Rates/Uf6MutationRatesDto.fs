namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open GeneSort.Core
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf6MutationRatesDto =
    { [<Key(0)>] Order: int
      [<Key(1)>] Seed6TransitionRates: Seed6TransitionRatesDto
      [<Key(2)>] OpsTransitionRates: OpsTransitionRatesArrayDto }

module Uf6MutationRatesDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (uf6MutationRates: Uf6MutationRates) : Uf6MutationRatesDto =
        { Order = uf6MutationRates.order
          Seed6TransitionRates = Seed6TransitionRatesDto.toDomain uf6MutationRates.seed6TransitionRates
          OpsTransitionRates = OpsTransitionRatesArrayDto.fromDomain uf6MutationRates.opsTransitionRates }

    let toDomain (dto: Uf6MutationRatesDto) : Uf6MutationRates =
        try
            if dto.Order < 6 || dto.Order % 6 <> 0 then
                failwith $"Order must be at least 6 and divisible by 6, got {dto.Order}"
            if dto.OpsTransitionRates.Rates.Length <> MathUtils.exactLog2 (dto.Order / 6) && dto.Order <> 6 then
                failwith $"OpsTransitionRates length ({dto.OpsTransitionRates.Rates.Length}) must match log2(order/6) ({MathUtils.exactLog2 (dto.Order / 6)})"
            { order = dto.Order
              seed6TransitionRates = Seed6TransitionRatesDto.fromDomain dto.Seed6TransitionRates
              opsTransitionRates = OpsTransitionRatesArrayDto.toDomain dto.OpsTransitionRates }
        with
        | ex -> failwith $"Failed to convert Uf6MutationRatesDto: {ex.Message}"