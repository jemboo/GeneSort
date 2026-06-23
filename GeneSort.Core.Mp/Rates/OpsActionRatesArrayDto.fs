namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type opsActionRatesArrayDto = {
    [<Key(0)>]
    opsActionRatesDtos: opsActionRatesDto array
}

module OpsActionRatesArrayDto =

    let toDomain (dto: opsActionRatesArrayDto) : opsActionRatesArray =
        let rates = Array.map OpsActionRatesDto.toDomain dto.opsActionRatesDtos
        opsActionRatesArray.create rates

    let fromDomain (domain: opsActionRatesArray) : opsActionRatesArrayDto = {
        opsActionRatesDtos = Array.map OpsActionRatesDto.fromDomain domain.RatesArray
    }
     