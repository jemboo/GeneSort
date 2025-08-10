namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpsTransitionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsTransitionRatesDto array
}

module OpsTransitionRatesArrayDto =
    let toOpsTransitionRatesArray (dto: OpsTransitionRatesArrayDto) : OpsTransitionRatesArray =
        let rates = Array.map OpsTransitionRatesDto.toOpsTransitionRates dto.Rates
        OpsTransitionRatesArray.create rates

    let fromOpsTransitionRatesArray (domain: OpsTransitionRatesArray) : OpsTransitionRatesArrayDto = {
        Rates = Array.map OpsTransitionRatesDto.fromOpsTransitionRates domain.RatesArray
    }


