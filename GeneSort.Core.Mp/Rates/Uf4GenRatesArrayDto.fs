namespace GeneSort.Core.Mp.RatesAndOps

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type Uf4GenRatesArrayDto =
    { [<Key(0)>] Rates: Uf4GenRatesDto array }

module Uf4GenRatesArrayDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toUf4GenRatesArrayDto (uf4GenRatesArray: Uf4GenRatesArray) : Uf4GenRatesArrayDto =
        { Rates = uf4GenRatesArray.RatesArray |> Array.map Uf4GenRatesDto.toUf4GenRatesDto }

    let fromUf4GenRatesArrayDto (dto: Uf4GenRatesArrayDto) : Uf4GenRatesArray =
        try
            if Array.isEmpty dto.Rates then
                failwith "Rates array cannot be empty"
            let rates = dto.Rates |> Array.map Uf4GenRatesDto.fromUf4GenRatesDto
            Uf4GenRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf4GenRatesArrayDto: {ex.Message}"