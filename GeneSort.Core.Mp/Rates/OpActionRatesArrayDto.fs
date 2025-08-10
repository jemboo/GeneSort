namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpActionRatesDto array
}

module OpActionRatesArrayDto =
    let toOpActionRatesArray (dto: OpActionRatesArrayDto) : OpActionRatesArray =
        let rates = Array.map OpActionRatesDto.toOpActionRates dto.Rates
        OpActionRatesArray.create rates

    let fromOpActionRatesArray (domain: OpActionRatesArray) : OpActionRatesArrayDto = {
        Rates = Array.map OpActionRatesDto.fromOpActionRates domain.RatesArray
    }