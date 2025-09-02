namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type OpsActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsActionRatesDto array
}

module OpsActionRatesArrayDto =

    let fromDomain (dto: OpsActionRatesArrayDto) : OpsActionRatesArray =
        let rates = Array.map OpsActionRatesDto.fromDomain dto.Rates
        OpsActionRatesArray.create rates

    let toDomain (domain: OpsActionRatesArray) : OpsActionRatesArrayDto = {
        Rates = Array.map OpsActionRatesDto.toDomain domain.RatesArray
    }
