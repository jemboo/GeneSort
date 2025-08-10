namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type OpsActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsActionRatesDto array
}

module OpsActionRatesArrayDto =
    let toOpsActionRatesArray (dto: OpsActionRatesArrayDto) : OpsActionRatesArray =
        let rates = Array.map OpsActionRatesDto.toOpsActionRates dto.Rates
        OpsActionRatesArray.create rates

    let fromOpsActionRatesArray (domain: OpsActionRatesArray) : OpsActionRatesArrayDto = {
        Rates = Array.map OpsActionRatesDto.fromOpsActionRates domain.RatesArray
    }
