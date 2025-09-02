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
            OpsActionRatesDto.fromDomain dto.OrthoRates,
            OpsActionRatesDto.fromDomain dto.ParaRates,
            OpsActionRatesDto.fromDomain dto.SelfReflRates
        )

    let fromDomain (domain: OpsTransitionRates) : OpsTransitionRatesDto = {
        OrthoRates = OpsActionRatesDto.toDomain domain.OrthoRates
        ParaRates = OpsActionRatesDto.toDomain domain.ParaRates
        SelfReflRates = OpsActionRatesDto.toDomain domain.SelfReflRates
    }