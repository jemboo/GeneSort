namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsActionRatesDto = {
    [<Key(0)>]
    OrthoThresh: float
    [<Key(1)>]
    ParaThresh: float
    [<Key(2)>]
    SelfReflThresh: float
}

module OpsActionRatesDto =

    let toDomain (dto: opsActionRatesDto) : opsActionRates =
        opsActionRates.create (dto.OrthoThresh, dto.ParaThresh - dto.OrthoThresh, dto.SelfReflThresh - dto.ParaThresh)

    let fromDomain (domain: opsActionRates) : opsActionRatesDto = {
        OrthoThresh = domain.OrthoRate
        ParaThresh = domain.OrthoRate + domain.ParaRate
        SelfReflThresh = domain.OrthoRate + domain.ParaRate + domain.SelfReflRate
    }

