namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

//[<MessagePackObject>]
//type Uf4MutationRatesDto =
//    { [<Key(0)>] Order: int
//      [<Key(1)>] SeedOpsTransitionRates: TwoOrbitUnfolderStepDTO
//      [<Key(2)>] TwoOrbitPairOpsTransitionRates: TwoOrbitUnfolderStepDTO array }

//module Uf4MutationRatesDto =

//    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
//    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

//    let toUf4MutationRatesDto (uf4MutationRates: Uf4MutationRates) : Uf4MutationRatesDto =
//        { Order = uf4MutationRates.order
//          SeedOpsTransitionRates = TwoOrbitUnfolderStepDTO.fromOpsTransitionRates uf4MutationRates.seedOpsTransitionRates
//          TwoOrbitPairOpsTransitionRates = uf4MutationRates.twoOrbitPairOpsTransitionRates.RatesArray
//                                          |> Array.map TwoOrbitUnfolderStepDTO.fromOpsTransitionRates }

//    let fromUf4MutationRatesDto (dto: Uf4MutationRatesDto) : Uf4MutationRates =
//        try
//            if dto.Order < 4 || dto.Order % 4 <> 0 then
//                failwith $"Order must be at least 4 and divisible by 4, got {dto.Order}"
//            if dto.TwoOrbitPairOpsTransitionRates.Length <> MathUtils.exactLog2 (dto.Order / 4) then
//                failwith $"TwoOrbitPairOpsTransitionRates length ({dto.TwoOrbitPairOpsTransitionRates.Length}) must match log2(order/4) ({MathUtils.exactLog2 (dto.Order / 4)})"
//            if Array.isEmpty dto.TwoOrbitPairOpsTransitionRates then
//                failwith "TwoOrbitPairOpsTransitionRates array cannot be empty"
//            { order = dto.Order
//              seedOpsTransitionRates = TwoOrbitUnfolderStepDTO.toOpsTransitionRates dto.SeedOpsTransitionRates
//              twoOrbitPairOpsTransitionRates = OpsTransitionRatesArray.create
//                                                (dto.TwoOrbitPairOpsTransitionRates
//                                                 |> Array.map TwoOrbitUnfolderStepDTO.toOpsTransitionRates) }
//        with
//        | ex -> failwith $"Failed to convert Uf4MutationRatesDto: {ex.Message}"