namespace GeneSort.Core.Mp.RatesAndOps

open GeneSort.Core

type indelRatesArrayDto = {
    indelRatesDtos: indelRatesDto array
}

module IndelRatesArrayDto =

    let toDomain (dto: indelRatesArrayDto) : indelRatesArray =
        let rates = Array.map IndelRatesDto.toDomain dto.indelRatesDtos
        indelRatesArray.create rates

    let fromDomain (domain: indelRatesArray) : indelRatesArrayDto = {
        indelRatesDtos = Array.map IndelRatesDto.fromDomain domain.RatesArray
    }