namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsGenRatesDto = {
    [<Key(0)>]
    orthoThresh: float
    [<Key(1)>]
    paraThresh: float
    [<Key(2)>]
    selfReflThresh: float
}

module OpsGenRatesDto =

    let toDomain (dto: opsGenRatesDto) : opsGenRates =
        opsGenRates.create (dto.orthoThresh, dto.paraThresh - dto.orthoThresh, dto.selfReflThresh - dto.paraThresh)

    let fromDomain (domain: opsGenRates) : opsGenRatesDto = {
        orthoThresh = domain.OrthoRate
        paraThresh = domain.OrthoRate + domain.ParaRate
        selfReflThresh = domain.OrthoRate + domain.ParaRate + domain.SelfReflRate
    }

