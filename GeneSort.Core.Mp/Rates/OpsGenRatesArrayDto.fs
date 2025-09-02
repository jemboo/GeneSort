namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type OpsGenRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsGenRatesDto array
}

module OpsGenRatesArrayDto =

    let fromDomain (dto: OpsGenRatesArrayDto) : OpsGenRatesArray =
        let rates = Array.map OpsGenRatesDto.fromDomain dto.Rates
        OpsGenRatesArray.create rates

    let toDomain (domain: OpsGenRatesArray) : OpsGenRatesArrayDto = {
        Rates = Array.map OpsGenRatesDto.toDomain domain.RatesArray
    }