namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type OpActionRatesArrayDto = {
    [<Key(0)>]
    Rates: OpActionRatesDto array
}

module OpActionRatesArrayDto =

    let fromDomain (dto: OpActionRatesArrayDto) : OpActionRatesArray =
        let rates = Array.map OpActionRatesDto.fromDomain dto.Rates
        OpActionRatesArray.create rates

    let toDomain (domain: OpActionRatesArray) : OpActionRatesArrayDto = {
        Rates = Array.map OpActionRatesDto.toDomain domain.RatesArray
    }