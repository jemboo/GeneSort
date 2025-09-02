namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf4MutationRatesArrayDto =
    { [<Key(0)>] Rates: Uf4MutationRatesDto array }

module Uf4MutationRatesArrayDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (uf4MutationRatesArray: Uf4MutationRatesArray) : Uf4MutationRatesArrayDto =
        { Rates = uf4MutationRatesArray.RatesArray |> Array.map Uf4MutationRatesDto.fromDomain }

    let toDomain (dto: Uf4MutationRatesArrayDto) : Uf4MutationRatesArray =
        try
            if Array.isEmpty dto.Rates then
                failwith "Rates array cannot be empty"
            let rates = dto.Rates |> Array.map Uf4MutationRatesDto.toDomain
            Uf4MutationRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf4MutationRatesArrayDto: {ex.Message}"