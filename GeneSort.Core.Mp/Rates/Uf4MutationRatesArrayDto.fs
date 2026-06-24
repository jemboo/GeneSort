namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type uf4MutationRatesArrayDto =
    { [<Key(0)>] uf4MutationRatesDtos: uf4MutationRatesDto array }

module Uf4MutationRatesArrayDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (uf4MutationRatesArray: uf4MutationRatesArray) : uf4MutationRatesArrayDto =
        { uf4MutationRatesDtos = uf4MutationRatesArray.RatesArray |> Array.map Uf4MutationRatesDto.fromDomain }

    let toDomain (dto: uf4MutationRatesArrayDto) : uf4MutationRatesArray =
        try
            if Array.isEmpty dto.uf4MutationRatesDtos then
                failwith "Rates array cannot be empty"
            let rates = dto.uf4MutationRatesDtos |> Array.map Uf4MutationRatesDto.toDomain
            uf4MutationRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf4MutationRatesArrayDto: {ex.Message}"