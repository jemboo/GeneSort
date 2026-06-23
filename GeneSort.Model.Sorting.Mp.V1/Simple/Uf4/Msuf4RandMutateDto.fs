namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf4

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Core.Mp
open GeneSort.Model.Mp.Sorter.Uf4

[<MessagePackObject>]
type msuf4RandMutateDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] uf4MutationRatesArrayDtos: Uf4MutationRatesDto }

module Msuf4RandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msuf4RandMutate: msuf4RandMutate) : msuf4RandMutateDto =
        { id = %msuf4RandMutate.Id
          rngFactoryDto = msuf4RandMutate.RngFactory |> RngFactoryDto.fromDomain
          uf4MutationRatesArrayDtos = Uf4MutationRatesDto.fromDomain msuf4RandMutate.Uf4MutationRates }

    let toDomain (dto: msuf4RandMutateDto) : msuf4RandMutate =
            msuf4RandMutate.create 
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (dto.uf4MutationRatesArrayDtos |> Uf4MutationRatesDto.toDomain)