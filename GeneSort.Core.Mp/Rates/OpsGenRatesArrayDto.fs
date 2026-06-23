namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type opsGenRatesArrayDto = {
    [<Key(0)>]
    Rates: opsGenRatesDto array
}

module OpsGenRatesArrayDto =

    let toDomain (dto: opsGenRatesArrayDto) : opsGenRatesArray =
        let rates = Array.map OpsGenRatesDto.toDomain dto.Rates
        opsGenRatesArray.create rates

    let fromDomain (domain: opsGenRatesArray) : opsGenRatesArrayDto = {
        Rates = Array.map OpsGenRatesDto.fromDomain domain.RatesArray
    }