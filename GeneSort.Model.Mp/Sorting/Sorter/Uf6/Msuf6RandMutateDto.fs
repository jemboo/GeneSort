namespace GeneSort.Model.Mp.Sorting.Sorter.Uf6

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Mp.Sorter.Uf6
open GeneSort.Core.Mp

[<MessagePackObject>]
type msuf6RandMutateDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] msuf6Dto: msuf6Dto
      [<Key(3)>] uf6MutationRatesArray: Uf6MutationRatesArrayDto }

module Msuf6RandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msuf6RandMutate: msuf6RandMutate) : msuf6RandMutateDto =
        { id = %msuf6RandMutate.Id
          rngFactoryDto = msuf6RandMutate.RngFactory |> RngFactoryDto.fromDomain
          msuf6Dto = Msuf6Dto.fromDomain msuf6RandMutate.Msuf6
          uf6MutationRatesArray = Uf6MutationRatesArrayDto.fromDomain msuf6RandMutate.Uf6MutationRatesArray }

    let toDomain (dto: msuf6RandMutateDto) : msuf6RandMutate =
        try
            let msuf6 = Msuf6Dto.toDomain dto.msuf6Dto |> Result.toOption |> Option.get
            let uf6MutationRatesArray = Uf6MutationRatesArrayDto.toDomain dto.uf6MutationRatesArray
            let rngFactory = dto.rngFactoryDto |> RngFactoryDto.toDomain
            msuf6RandMutate.create rngFactory uf6MutationRatesArray msuf6
        with
        | ex -> failwith $"Failed to convert Msuf6RandMutateDto: {ex.Message}"