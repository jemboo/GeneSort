namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type OpsGenRatesArrayDto = {
    [<Key(0)>]
    Rates: OpsGenRatesDto array
}

module OpsGenRatesArrayDto =
    let toOpsGenRatesArray (dto: OpsGenRatesArrayDto) : OpsGenRatesArray =
        let rates = Array.map OpsGenRatesDto.toOpsGenRates dto.Rates
        OpsGenRatesArray.create rates

    let fromOpsGenRatesArray (domain: OpsGenRatesArray) : OpsGenRatesArrayDto = {
        Rates = Array.map OpsGenRatesDto.fromOpsGenRates domain.RatesArray
    }