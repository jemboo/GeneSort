
namespace GeneSort.Model.Mp.Sorter.Uf6

open GeneSort.Core
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps

[<MessagePackObject>]
type Uf6GenRatesArrayDto =
    { [<Key(0)>] uf6GenRatesDtos: uf6GenRatesDto array }

module Uf6GenRatesArrayDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (gen6RatesArray: uf6GenRatesArray) : Uf6GenRatesArrayDto =
        { uf6GenRatesDtos = gen6RatesArray.RatesArray |> Array.map Uf6GenRatesDto.fromDomain }

    let toDomain (dto: Uf6GenRatesArrayDto) : uf6GenRatesArray =
        try
            if Array.isEmpty dto.uf6GenRatesDtos then
                failwith "Rates array cannot be empty"
            let rates = dto.uf6GenRatesDtos |> Array.map Uf6GenRatesDto.toDomain
            uf6GenRatesArray.create rates
        with
        | ex -> failwith $"Failed to convert Uf6GenRatesArrayDto: {ex.Message}"