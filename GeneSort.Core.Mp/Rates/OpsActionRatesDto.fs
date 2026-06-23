namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsActionRatesDto = {
    [<Key(0)>]
    orthoThresh: float
    [<Key(1)>]
    paraThresh: float
    [<Key(2)>]
    selfReflThresh: float
}

module OpsActionRatesDto =

    let toDomain (dto: opsActionRatesDto) : opsActionRates =
        opsActionRates.create (dto.orthoThresh, dto.paraThresh - dto.orthoThresh, dto.selfReflThresh - dto.paraThresh)

    let fromDomain (domain: opsActionRates) : opsActionRatesDto = {
        orthoThresh = domain.OrthoRate
        paraThresh = domain.OrthoRate + domain.ParaRate
        selfReflThresh = domain.OrthoRate + domain.ParaRate + domain.SelfReflRate
    }

