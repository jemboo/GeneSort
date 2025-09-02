namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf6MutationRatesArrayDto =
    { [<Key(0)>] Rates: Uf6MutationRatesDto array }

module Uf6MutationRatesArrayDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (uf6MutationRatesArray: Uf6MutationRatesArray) : Uf6MutationRatesArrayDto =
        { Rates = uf6MutationRatesArray.RatesArray |> Array.map Uf6MutationRatesDto.fromDomain }

    let toDomain (dto: Uf6MutationRatesArrayDto) : Uf6MutationRatesArray =
        try
            if Array.isEmpty dto.Rates then
                failwith "Rates array cannot be empty"
            let rates = dto.Rates |> Array.map Uf6MutationRatesDto.toDomain
            Uf6MutationRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf6MutationRatesArrayDto: {ex.Message}"