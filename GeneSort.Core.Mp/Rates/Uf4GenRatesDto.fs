namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type Uf4GenRatesDto = {
    [<Key(0)>]
    Order: int
    [<Key(1)>]
    OpsGenRatesDto: OpsGenRatesDto
    [<Key(2)>]
    OpsGenRatesDtoList: OpsGenRatesDto array
}

module Uf4GenRatesDto =
    let toUf4GenRates (dto: Uf4GenRatesDto) : Uf4GenRates =
        {
            order = dto.Order
            seedOpsGenRates = OpsGenRatesDto.toOpsGenRates dto.OpsGenRatesDto
            opsGenRatesArray = Array.map OpsGenRatesDto.toOpsGenRates dto.OpsGenRatesDtoList
        }

    let fromUf4GenRates (domain: Uf4GenRates) : Uf4GenRatesDto = {
        Order = domain.order
        OpsGenRatesDto = OpsGenRatesDto.fromOpsGenRates domain.seedOpsGenRates
        OpsGenRatesDtoList = Array.map OpsGenRatesDto.fromOpsGenRates domain.opsGenRatesArray
    }