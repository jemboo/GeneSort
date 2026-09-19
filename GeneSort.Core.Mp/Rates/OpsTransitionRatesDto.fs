namespace GeneSort.Core.Mp.RatesAndOps

open GeneSort.Core

type opsTransitionRatesDto = {
    orthoRates: opsActionRatesDto
    paraRates: opsActionRatesDto
    selfReflRates: opsActionRatesDto
}

module OpsTransitionRatesDto =

    let toDomain (dto: opsTransitionRatesDto) : opsTransitionRates =
        opsTransitionRates.create(
            OpsActionRatesDto.toDomain dto.orthoRates,
            OpsActionRatesDto.toDomain dto.paraRates,
            OpsActionRatesDto.toDomain dto.selfReflRates
        )

    let fromDomain (domain: opsTransitionRates) : opsTransitionRatesDto = {
        orthoRates = OpsActionRatesDto.fromDomain domain.OrthoRates
        paraRates = OpsActionRatesDto.fromDomain domain.ParaRates
        selfReflRates = OpsActionRatesDto.fromDomain domain.SelfReflRates
    }