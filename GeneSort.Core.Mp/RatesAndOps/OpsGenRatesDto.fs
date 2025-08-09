namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpsGenRatesDto = {
    [<Key(0)>]
    OrthoThresh: float
    [<Key(1)>]
    ParaThresh: float
    [<Key(2)>]
    SelfReflThresh: float
}

module OpsGenRatesDto =
    let toOpsGenRates (dto: OpsGenRatesDto) : OpsGenRates =
        OpsGenRates.create (dto.OrthoThresh, dto.ParaThresh - dto.OrthoThresh, dto.SelfReflThresh - dto.ParaThresh)

    let fromOpsGenRates (domain: OpsGenRates) : OpsGenRatesDto = {
        OrthoThresh = domain.OrthoRate
        ParaThresh = domain.OrthoRate + domain.ParaRate
        SelfReflThresh = domain.OrthoRate + domain.ParaRate + domain.SelfReflRate
    }

