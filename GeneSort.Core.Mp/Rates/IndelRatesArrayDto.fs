namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type indelRatesArrayDto = {
    [<Key(0)>]
    Rates: indelRatesDto array
}

module IndelRatesArrayDto =

    let toDomain (dto: indelRatesArrayDto) : indelRatesArray =
        let rates = Array.map IndelRatesDto.toDomain dto.Rates
        indelRatesArray.create rates

    let fromDomain (domain: indelRatesArray) : indelRatesArrayDto = {
        Rates = Array.map IndelRatesDto.fromDomain domain.RatesArray
    }

