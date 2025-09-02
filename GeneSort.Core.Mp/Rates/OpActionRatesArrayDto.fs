namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpActionRatesDto array
}

module OpActionRatesArrayDto =

    let toDomain (dto: OpActionRatesArrayDto) : OpActionRatesArray =
        let rates = Array.map OpActionRatesDto.toDomain dto.Rates
        OpActionRatesArray.create rates

    let fromDomain (domain: OpActionRatesArray) : OpActionRatesArrayDto = {
        Rates = Array.map OpActionRatesDto.fromDomain domain.RatesArray
    }