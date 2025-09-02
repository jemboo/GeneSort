namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpsTransitionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsTransitionRatesDto array
}

module OpsTransitionRatesArrayDto =

    let toDomain (dto: OpsTransitionRatesArrayDto) : OpsTransitionRatesArray =
        let rates = Array.map OpsTransitionRatesDto.toDomain dto.Rates
        OpsTransitionRatesArray.create rates

    let fromDomain (domain: OpsTransitionRatesArray) : OpsTransitionRatesArrayDto = {
        Rates = Array.map OpsTransitionRatesDto.fromDomain domain.RatesArray
    }


