namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opActionRatesArrayDto = {
    [<Key(0)>]
    opActionRatesDtos: opActionRatesDto array
}

module OpActionRatesArrayDto =

    let toDomain (dto: opActionRatesArrayDto) : opActionRatesArray =
        let rates = Array.map OpActionRatesDto.toDomain dto.opActionRatesDtos
        opActionRatesArray.create rates

    let fromDomain (domain: opActionRatesArray) : opActionRatesArrayDto = {
        opActionRatesDtos = Array.map OpActionRatesDto.fromDomain domain.RatesArray
    }