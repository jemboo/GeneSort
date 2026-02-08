namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type msuf6RandMutateDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngType: rngType
      [<Key(2)>] msuf6Dto: msuf6Dto
      [<Key(3)>] uf6MutationRatesArray: Uf6MutationRatesArrayDto }

module Msuf6RandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf6RandMutateDto (msuf6RandMutate: msuf6RandMutate) : msuf6RandMutateDto =
        { id = %msuf6RandMutate.Id
          rngType = msuf6RandMutate.RngType
          msuf6Dto = Msuf6Dto.toMsuf6Dto msuf6RandMutate.Msuf6
          uf6MutationRatesArray = Uf6MutationRatesArrayDto.fromDomain msuf6RandMutate.Uf6MutationRatesArray }

    let fromMsuf6RandMutateDto (dto: msuf6RandMutateDto) : msuf6RandMutate =
        try
            if dto.rngType = Unchecked.defaultof<rngType> then
                failwith "rngType must be specified"
            let msuf6 = Msuf6Dto.fromMsuf6Dto dto.msuf6Dto |> Result.toOption |> Option.get
            let uf6MutationRatesArray = Uf6MutationRatesArrayDto.toDomain dto.uf6MutationRatesArray

            msuf6RandMutate.create dto.rngType msuf6 uf6MutationRatesArray
        with
        | ex -> failwith $"Failed to convert Msuf6RandMutateDto: {ex.Message}"