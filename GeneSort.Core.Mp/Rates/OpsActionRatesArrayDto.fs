namespace GeneSort.Core.Mp.RatesAndOps

open GeneSort.Core

type opsActionRatesArrayDto = {
    opsActionRatesDtos: opsActionRatesDto array
}

module OpsActionRatesArrayDto =

    let toDomain (dto: opsActionRatesArrayDto) : opsActionRatesArray =
        let rates = Array.map OpsActionRatesDto.toDomain dto.opsActionRatesDtos
        opsActionRatesArray.create rates

    let fromDomain (domain: opsActionRatesArray) : opsActionRatesArrayDto = {
        opsActionRatesDtos = Array.map OpsActionRatesDto.fromDomain domain.RatesArray
    }