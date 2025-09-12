
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

    let fromDomain (gen6RatesArray: uf6GenRatesArray) : Uf6GenRatesArrayDto =
        { Rates = gen6RatesArray.RatesArray |> Array.map Uf6GenRatesDto.fromDomain }

    let toDomain (dto: Uf6GenRatesArrayDto) : uf6GenRatesArray =
        try
            if Array.isEmpty dto.Rates then
                failwith "Rates array cannot be empty"
            let rates = dto.Rates |> Array.map Uf6GenRatesDto.toDomain
            uf6GenRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf6GenRatesArrayDto: {ex.Message}"