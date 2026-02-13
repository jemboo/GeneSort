namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opActionRatesArrayDto = {
    [<Key(0)>]
    Rates: opActionRatesDto array
}

module OpActionRatesArrayDto =

    let toDomain (dto: opActionRatesArrayDto) : opActionRatesArray =
        let rates = Array.map OpActionRatesDto.toDomain dto.Rates
        opActionRatesArray.create rates

    let fromDomain (domain: opActionRatesArray) : opActionRatesArrayDto = {
        Rates = Array.map OpActionRatesDto.fromDomain domain.RatesArray
    }