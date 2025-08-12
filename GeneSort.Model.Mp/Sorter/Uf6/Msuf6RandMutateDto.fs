namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Msuf6RandMutateDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] RngType: rngType
      [<Key(2)>] Msuf6: Msuf6Dto
      [<Key(3)>] Uf6MutationRatesArray: Uf6MutationRatesArrayDto }

module Msuf6RandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf6RandMutateDto (msuf6RandMutate: Msuf6RandMutate) : Msuf6RandMutateDto =
        { Id = %msuf6RandMutate.Id
          RngType = msuf6RandMutate.RngType
          Msuf6 = Msuf6Dto.toMsuf6Dto msuf6RandMutate.Msuf6
          Uf6MutationRatesArray = Uf6MutationRatesArrayDto.toUf6MutationRatesArrayDto msuf6RandMutate.Uf6MutationRatesArray }

    let fromMsuf6RandMutateDto (dto: Msuf6RandMutateDto) : Msuf6RandMutate =
        try
            if dto.RngType = Unchecked.defaultof<rngType> then
                failwith "rngType must be specified"
            let msuf6 = Msuf6Dto.fromMsuf6Dto dto.Msuf6 |> Result.toOption |> Option.get
            let uf6MutationRatesArray = Uf6MutationRatesArrayDto.fromUf6MutationRatesArrayDto dto.Uf6MutationRatesArray

            Msuf6RandMutate.create dto.RngType msuf6 uf6MutationRatesArray
        with
        | ex -> failwith $"Failed to convert Msuf6RandMutateDto: {ex.Message}"