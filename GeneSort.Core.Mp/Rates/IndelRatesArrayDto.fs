namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type IndelRatesArrayDto = {
    [<Key(0)>]
    Rates: IndelRatesDto array
}

module IndelRatesArrayDto =

    let fromDomain (dto: IndelRatesArrayDto) : IndelRatesArray =
        let rates = Array.map IndelRatesDto.toDomain dto.Rates
        IndelRatesArray.create rates

    let toDomain (domain: IndelRatesArray) : IndelRatesArrayDto = {
        Rates = Array.map IndelRatesDto.fromDomain domain.RatesArray
    }

