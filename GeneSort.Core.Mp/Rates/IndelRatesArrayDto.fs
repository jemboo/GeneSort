namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type IndelRatesArrayDto = {
    [<Key(0)>]
    Rates: IndelRatesDto array
}

module IndelRatesArrayDto =
    let toIndelRatesArray (dto: IndelRatesArrayDto) : IndelRatesArray =
        let rates = Array.map IndelRatesDto.toIndelRates dto.Rates
        IndelRatesArray.create rates

    let fromIndelRatesArray (domain: IndelRatesArray) : IndelRatesArrayDto = {
        Rates = Array.map IndelRatesDto.fromIndelRates domain.RatesArray
    }

