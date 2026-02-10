namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type IndelRatesArrayDto = {
    [<Key(0)>]
    Rates: IndelRatesDto array
}

module IndelRatesArrayDto =

    let toDomain (dto: IndelRatesArrayDto) : indelRatesArray =
        let rates = Array.map IndelRatesDto.toDomain dto.Rates
        indelRatesArray.create rates

    let fromDomain (domain: indelRatesArray) : IndelRatesArrayDto = {
        Rates = Array.map IndelRatesDto.fromDomain domain.RatesArray
    }

