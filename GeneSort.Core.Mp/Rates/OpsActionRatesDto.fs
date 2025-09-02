namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpsActionRatesDto = {
    [<Key(0)>]
    OrthoThresh: float
    [<Key(1)>]
    ParaThresh: float
    [<Key(2)>]
    SelfReflThresh: float
}

module OpsActionRatesDto =

    let fromDomain (dto: OpsActionRatesDto) : OpsActionRates =
        OpsActionRates.create (dto.OrthoThresh, dto.ParaThresh - dto.OrthoThresh, dto.SelfReflThresh - dto.ParaThresh)

    let toDomain (domain: OpsActionRates) : OpsActionRatesDto = {
        OrthoThresh = domain.OrthoRate
        ParaThresh = domain.OrthoRate + domain.ParaRate
        SelfReflThresh = domain.OrthoRate + domain.ParaRate + domain.SelfReflRate
    }

