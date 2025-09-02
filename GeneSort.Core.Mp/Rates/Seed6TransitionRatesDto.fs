namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type Seed6TransitionRatesDto = {
    [<Key(0)>]
    Ortho1Rates: Seed6ActionRatesDto
    [<Key(1)>]
    Ortho2Rates: Seed6ActionRatesDto
    [<Key(2)>]
    Para1Rates: Seed6ActionRatesDto
    [<Key(3)>]
    Para2Rates: Seed6ActionRatesDto
    [<Key(4)>]
    Para3Rates: Seed6ActionRatesDto
    [<Key(5)>]
    Para4Rates: Seed6ActionRatesDto
    [<Key(6)>]
    SelfReflRates: Seed6ActionRatesDto
}

module Seed6TransitionRatesDto =

    let fromDomain (dto: Seed6TransitionRatesDto) : Seed6TransitionRates =
        Seed6TransitionRates.create(
            Seed6ActionRatesDto.fromDomain dto.Ortho1Rates,
            Seed6ActionRatesDto.fromDomain dto.Ortho2Rates,
            Seed6ActionRatesDto.fromDomain dto.Para1Rates,
            Seed6ActionRatesDto.fromDomain dto.Para2Rates,
            Seed6ActionRatesDto.fromDomain dto.Para3Rates,
            Seed6ActionRatesDto.fromDomain dto.Para4Rates,
            Seed6ActionRatesDto.fromDomain dto.SelfReflRates
        )

    let toDomain (domain: Seed6TransitionRates) : Seed6TransitionRatesDto = {
        Ortho1Rates = Seed6ActionRatesDto.toDomain domain.Ortho1Rates
        Ortho2Rates = Seed6ActionRatesDto.toDomain domain.Ortho2Rates
        Para1Rates = Seed6ActionRatesDto.toDomain domain.Para1Rates
        Para2Rates = Seed6ActionRatesDto.toDomain domain.Para2Rates
        Para3Rates = Seed6ActionRatesDto.toDomain domain.Para3Rates
        Para4Rates = Seed6ActionRatesDto.toDomain domain.Para4Rates
        SelfReflRates = Seed6ActionRatesDto.toDomain domain.SelfReflRates
    }