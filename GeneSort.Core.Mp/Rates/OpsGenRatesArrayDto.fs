namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type opsGenRatesArrayDto = {
    [<Key(0)>]
    opsGenRatesDtos: opsGenRatesDto array
}

module OpsGenRatesArrayDto =

    let toDomain (dto: opsGenRatesArrayDto) : opsGenRatesArray =
        let rates = Array.map OpsGenRatesDto.toDomain dto.opsGenRatesDtos
        opsGenRatesArray.create rates

    let fromDomain (domain: opsGenRatesArray) : opsGenRatesArrayDto = {
        opsGenRatesDtos = Array.map OpsGenRatesDto.fromDomain domain.RatesArray
    }