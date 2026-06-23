namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opActionRatesDto = {
    [<Key(0)>]
    orthoThresh: float
    [<Key(1)>]
    paraThresh: float
}

module OpActionRatesDto =

    let toDomain (dto: opActionRatesDto) : opActionRates =
        opActionRates.create (dto.orthoThresh, dto.paraThresh - dto.orthoThresh)

    let fromDomain (domain: opActionRates) : opActionRatesDto = {
        orthoThresh = domain.OrthoRate
        paraThresh = domain.OrthoRate + domain.ParaRate
    }

