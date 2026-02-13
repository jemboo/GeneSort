namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opActionRatesDto = {
    [<Key(0)>]
    OrthoThresh: float
    [<Key(1)>]
    ParaThresh: float
}

module OpActionRatesDto =

    let toDomain (dto: opActionRatesDto) : opActionRates =
        opActionRates.create (dto.OrthoThresh, dto.ParaThresh - dto.OrthoThresh)

    let fromDomain (domain: opActionRates) : opActionRatesDto = {
        OrthoThresh = domain.OrthoRate
        ParaThresh = domain.OrthoRate + domain.ParaRate
    }

