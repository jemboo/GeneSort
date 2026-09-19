namespace GeneSort.Core.Mp.RatesAndOps

open GeneSort.Core

type opsTransitionRatesArrayDto = {
    opsTransitionRatesDtos: opsTransitionRatesDto array
}

module OpsTransitionRatesArrayDto =

    let toDomain (dto: opsTransitionRatesArrayDto) : opsTransitionRatesArray =
        let rates = Array.map OpsTransitionRatesDto.toDomain dto.opsTransitionRatesDtos
        opsTransitionRatesArray.create rates

    let fromDomain (domain: opsTransitionRatesArray) : opsTransitionRatesArrayDto = {
        opsTransitionRatesDtos = Array.map OpsTransitionRatesDto.fromDomain domain.RatesArray
    }