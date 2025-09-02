namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type OpsActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsActionRatesDto array
}

module OpsActionRatesArrayDto =

    let toDomain (dto: OpsActionRatesArrayDto) : OpsActionRatesArray =
        let rates = Array.map OpsActionRatesDto.toDomain dto.Rates
        OpsActionRatesArray.create rates

    let fromDomain (domain: OpsActionRatesArray) : OpsActionRatesArrayDto = {
        Rates = Array.map OpsActionRatesDto.fromDomain domain.RatesArray
    }
