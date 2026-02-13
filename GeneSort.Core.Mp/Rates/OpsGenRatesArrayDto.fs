namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type OpsGenRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsGenRatesDto array
}

module OpsGenRatesArrayDto =

    let toDomain (dto: OpsGenRatesArrayDto) : opsGenRatesArray =
        let rates = Array.map OpsGenRatesDto.toDomain dto.Rates
        opsGenRatesArray.create rates

    let fromDomain (domain: opsGenRatesArray) : OpsGenRatesArrayDto = {
        Rates = Array.map OpsGenRatesDto.fromDomain domain.RatesArray
    }