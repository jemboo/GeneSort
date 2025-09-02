namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter.Uf4
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Msuf4RandMutateDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] RngType: rngType
      [<Key(2)>] Msuf4: Msuf4Dto
      [<Key(3)>] Uf4MutationRatesArray: Uf4MutationRatesArrayDto }

module Msuf4RandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf4RandMutateDto (msuf4RandMutate: Msuf4RandMutate) : Msuf4RandMutateDto =
        { Id = %msuf4RandMutate.Id
          RngType = msuf4RandMutate.RngType
          Msuf4 = Msuf4Dto.toMsuf4Dto msuf4RandMutate.Msuf4
          Uf4MutationRatesArray = Uf4MutationRatesArrayDto.fromDomain msuf4RandMutate.Uf4MutationRatesArray }

    let fromMsuf4RandMutateDto (dto: Msuf4RandMutateDto) : Msuf4RandMutate =
        try
            if dto.RngType = Unchecked.defaultof<rngType> then
                failwith "rngType must be specified"
            let msuf4 = Msuf4Dto.fromMsuf4Dto dto.Msuf4 |> Result.toOption |> Option.get
            let uf4MutationRatesArray = Uf4MutationRatesArrayDto.toDomain dto.Uf4MutationRatesArray
            if uf4MutationRatesArray.Length <> %msuf4.StageCount then
                failwith $"Uf4MutationRatesArray length ({uf4MutationRatesArray.Length}) must equal StageCount ({%msuf4.StageCount})"

            Msuf4RandMutate.create dto.RngType msuf4 uf4MutationRatesArray
        with
        | ex -> failwith $"Failed to convert Msuf4RandMutateDto: {ex.Message}"