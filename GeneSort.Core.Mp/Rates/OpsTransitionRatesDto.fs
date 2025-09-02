namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpsTransitionRatesDto = {
    [<Key(0)>]
    OrthoRates: OpsActionRatesDto
    [<Key(1)>]
    ParaRates: OpsActionRatesDto
    [<Key(2)>]
    SelfReflRates: OpsActionRatesDto
}

module OpsTransitionRatesDto =

    let toDomain (dto: OpsTransitionRatesDto) : OpsTransitionRates =
        OpsTransitionRates.create(
            OpsActionRatesDto.toDomain dto.OrthoRates,
            OpsActionRatesDto.toDomain dto.ParaRates,
            OpsActionRatesDto.toDomain dto.SelfReflRates
        )

    let fromDomain (domain: OpsTransitionRates) : OpsTransitionRatesDto = {
        OrthoRates = OpsActionRatesDto.fromDomain domain.OrthoRates
        ParaRates = OpsActionRatesDto.fromDomain domain.ParaRates
        SelfReflRates = OpsActionRatesDto.fromDomain domain.SelfReflRates
    }