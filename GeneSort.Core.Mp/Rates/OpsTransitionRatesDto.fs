namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsTransitionRatesDto = {
    [<Key(0)>]
    orthoRates: opsActionRatesDto
    [<Key(1)>]
    paraRates: opsActionRatesDto
    [<Key(2)>]
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