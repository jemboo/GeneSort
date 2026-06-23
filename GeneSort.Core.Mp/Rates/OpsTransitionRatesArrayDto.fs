namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsTransitionRatesArrayDto = {
    [<Key(0)>]
    opsTransitionRatesDtos: opsTransitionRatesDto array
}

module OpsTransitionRatesArrayDto =

    let toDomain (dto: opsTransitionRatesArrayDto) : opsTransitionRatesArray =
        let rates = Array.map OpsTransitionRatesDto.toDomain dto.opsTransitionRatesDtos
        opsTransitionRatesArray.create rates

    let fromDomain (domain: opsTransitionRatesArray) : opsTransitionRatesArrayDto = {
        opsTransitionRatesDtos = Array.map OpsTransitionRatesDto.fromDomain domain.RatesArray
    }


