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

    let toDomain (dto: Seed6TransitionRatesDto) : seed6TransitionRates =
        seed6TransitionRates.create(
            Seed6ActionRatesDto.toDomain dto.Ortho1Rates,
            Seed6ActionRatesDto.toDomain dto.Ortho2Rates,
            Seed6ActionRatesDto.toDomain dto.Para1Rates,
            Seed6ActionRatesDto.toDomain dto.Para2Rates,
            Seed6ActionRatesDto.toDomain dto.Para3Rates,
            Seed6ActionRatesDto.toDomain dto.Para4Rates,
            Seed6ActionRatesDto.toDomain dto.SelfReflRates
        )

    let fromDomain (domain: seed6TransitionRates) : Seed6TransitionRatesDto = {
        Ortho1Rates = Seed6ActionRatesDto.fromDomain domain.Ortho1Rates
        Ortho2Rates = Seed6ActionRatesDto.fromDomain domain.Ortho2Rates
        Para1Rates = Seed6ActionRatesDto.fromDomain domain.Para1Rates
        Para2Rates = Seed6ActionRatesDto.fromDomain domain.Para2Rates
        Para3Rates = Seed6ActionRatesDto.fromDomain domain.Para3Rates
        Para4Rates = Seed6ActionRatesDto.fromDomain domain.Para4Rates
        SelfReflRates = Seed6ActionRatesDto.fromDomain domain.SelfReflRates
    }