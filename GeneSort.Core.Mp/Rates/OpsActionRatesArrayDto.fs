namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type opsActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsActionRatesDto array
}

module OpsActionRatesArrayDto =

    let toDomain (dto: opsActionRatesArrayDto) : opsActionRatesArray =
        let rates = Array.map OpsActionRatesDto.toDomain dto.Rates
        opsActionRatesArray.create rates

    let fromDomain (domain: opsActionRatesArray) : opsActionRatesArrayDto = {
        Rates = Array.map OpsActionRatesDto.fromDomain domain.RatesArray
    }
     