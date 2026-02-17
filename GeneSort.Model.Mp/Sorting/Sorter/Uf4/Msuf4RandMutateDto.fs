namespace GeneSort.Model.Mp.Sorting.Sorter.Uf4

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Mp.Sorter.Uf4

[<MessagePackObject>]
type msuf4RandMutateDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngType: rngType
      [<Key(2)>] msuf4: msuf4Dto
      [<Key(3)>] uf4MutationRatesArrayDtos: Uf4MutationRatesArrayDto }

module Msuf4RandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msuf4RandMutate: msuf4RandMutate) : msuf4RandMutateDto =
        { id = %msuf4RandMutate.Id
          rngType = msuf4RandMutate.RngType
          msuf4 = Msuf4Dto.fromDomain msuf4RandMutate.Msuf4
          uf4MutationRatesArrayDtos = Uf4MutationRatesArrayDto.fromDomain msuf4RandMutate.Uf4MutationRatesArray }

    let toDomain (dto: msuf4RandMutateDto) : msuf4RandMutate =
        try
            if dto.rngType = Unchecked.defaultof<rngType> then
                failwith "rngType must be specified"
            let msuf4 = Msuf4Dto.toDomain dto.msuf4 |> Result.toOption |> Option.get
            let uf4MutationRatesArray = Uf4MutationRatesArrayDto.toDomain dto.uf4MutationRatesArrayDtos
            if uf4MutationRatesArray.Length <> %msuf4.StageLength then
                failwith $"Uf4MutationRatesArray length ({uf4MutationRatesArray.Length}) must equal StageLength ({%msuf4.StageLength})"

            msuf4RandMutate.create dto.rngType uf4MutationRatesArray msuf4
        with
        | ex -> failwith $"Failed to convert Msuf4RandMutateDto: {ex.Message}"