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
    let toOpsTransitionRates (dto: OpsTransitionRatesDto) : OpsTransitionRates =
        OpsTransitionRates.create(
            OpsActionRatesDto.toOpsActionRates dto.OrthoRates,
            OpsActionRatesDto.toOpsActionRates dto.ParaRates,
            OpsActionRatesDto.toOpsActionRates dto.SelfReflRates
        )

    let fromOpsTransitionRates (domain: OpsTransitionRates) : OpsTransitionRatesDto = {
        OrthoRates = OpsActionRatesDto.fromOpsActionRates domain.OrthoRates
        ParaRates = OpsActionRatesDto.fromOpsActionRates domain.ParaRates
        SelfReflRates = OpsActionRatesDto.fromOpsActionRates domain.SelfReflRates
    }