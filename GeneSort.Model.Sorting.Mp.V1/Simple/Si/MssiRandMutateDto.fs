namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si

open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

[<MessagePackObject>]
type mssiRandMutateDto = 
    {
      [<Key(0)>] rngFactoryDto: rngFactoryDto
      [<Key(1)>] opActionRatesDto: opActionRatesDto }

module MssiRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (mssiRandMutate: mssiRandMutate) : mssiRandMutateDto =
        {
          rngFactoryDto = mssiRandMutate.RngFactory |> RngFactoryDto.fromDomain
          opActionRatesDto = OpActionRatesDto.fromDomain mssiRandMutate.OpActionRates }

    let toDomain (dto: mssiRandMutateDto) : mssiRandMutate =
        mssiRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (OpActionRatesDto.toDomain dto.opActionRatesDto)