namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpActionRatesDto = {
    [<Key(0)>]
    OrthoThresh: float
    [<Key(1)>]
    ParaThresh: float
}

module OpActionRatesDto =

    let fromDomain (dto: OpActionRatesDto) : OpActionRates =
        OpActionRates.create (dto.OrthoThresh, dto.ParaThresh - dto.OrthoThresh)

    let toDomain (domain: OpActionRates) : OpActionRatesDto = {
        OrthoThresh = domain.OrthoRate
        ParaThresh = domain.OrthoRate + domain.ParaRate
    }

