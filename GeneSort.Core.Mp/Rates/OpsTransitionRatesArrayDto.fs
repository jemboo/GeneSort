namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type opsTransitionRatesArrayDto = {
    [<Key(0)>]
    Rates: opsTransitionRatesDto array
}

module OpsTransitionRatesArrayDto =

    let toDomain (dto: opsTransitionRatesArrayDto) : opsTransitionRatesArray =
        let rates = Array.map OpsTransitionRatesDto.toDomain dto.Rates
        opsTransitionRatesArray.create rates

    let fromDomain (domain: opsTransitionRatesArray) : opsTransitionRatesArrayDto = {
        Rates = Array.map OpsTransitionRatesDto.fromDomain domain.RatesArray
    }


