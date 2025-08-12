
namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps

[<MessagePackObject>]
type Uf6GenRatesArrayDto =
    { [<Key(0)>] Rates: Uf6GenRatesDto array }

module Uf6GenRatesArrayDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toUf6GenRatesArrayDto (uf6GenRatesArray: Uf6GenRatesArray) : Uf6GenRatesArrayDto =
        { Rates = uf6GenRatesArray.RatesArray |> Array.map Uf6GenRatesDto.toUf6GenRatesDto }

    let fromUf6GenRatesArrayDto (dto: Uf6GenRatesArrayDto) : Uf6GenRatesArray =
        try
            if Array.isEmpty dto.Rates then
                failwith "Rates array cannot be empty"
            let rates = dto.Rates |> Array.map Uf6GenRatesDto.fromUf6GenRatesDto
            Uf6GenRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf6GenRatesArrayDto: {ex.Message}"