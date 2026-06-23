namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsTransitionRatesDto = {
    [<Key(0)>]
    OrthoRates: opsActionRatesDto
    [<Key(1)>]
    ParaRates: opsActionRatesDto
    [<Key(2)>]
    SelfReflRates: opsActionRatesDto
}

module OpsTransitionRatesDto =

    let toDomain (dto: opsTransitionRatesDto) : opsTransitionRates =
        opsTransitionRates.create(
            OpsActionRatesDto.toDomain dto.OrthoRates,
            OpsActionRatesDto.toDomain dto.ParaRates,
            OpsActionRatesDto.toDomain dto.SelfReflRates
        )

    let fromDomain (domain: opsTransitionRates) : opsTransitionRatesDto = {
        OrthoRates = OpsActionRatesDto.fromDomain domain.OrthoRates
        ParaRates = OpsActionRatesDto.fromDomain domain.ParaRates
        SelfReflRates = OpsActionRatesDto.fromDomain domain.SelfReflRates
    }