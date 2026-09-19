namespace GeneSort.Core.Mp.RatesAndOps

open GeneSort.Core

type opActionRatesArrayDto = {
    opActionRatesDtos: opActionRatesDto array
}

module OpActionRatesArrayDto =

    let toDomain (dto: opActionRatesArrayDto) : opActionRatesArray =
        let rates = Array.map OpActionRatesDto.toDomain dto.opActionRatesDtos
        opActionRatesArray.create rates

    let fromDomain (domain: opActionRatesArray) : opActionRatesArrayDto = {
        opActionRatesDtos = Array.map OpActionRatesDto.fromDomain domain.RatesArray
    }