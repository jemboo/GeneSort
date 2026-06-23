namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp
open GeneSort.Model.Sorting.V1.Simple.Rs

[<MessagePackObject>]
type msrsRandMutateDto = 
    { 
      [<Key(0)>] rngFactoryDto: rngFactoryDto
      [<Key(1)>] opsActionRates: opsActionRatesDto }

module MsrsRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msrsRandMutate: msrsRandMutate) : msrsRandMutateDto =
        { 
          rngFactoryDto = msrsRandMutate.RngFactory |> RngFactoryDto.fromDomain
          opsActionRates = OpsActionRatesDto.fromDomain msrsRandMutate.OpsActionRates }

    let toDomain (dto: msrsRandMutateDto) : msrsRandMutate =
            msrsRandMutate.create
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (OpsActionRatesDto.toDomain dto.opsActionRates)