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
    let toSeed6TransitionRates (dto: Seed6TransitionRatesDto) : Seed6TransitionRates =
        Seed6TransitionRates.create(
            Seed6ActionRatesDto.toSeed6ActionRates dto.Ortho1Rates,
            Seed6ActionRatesDto.toSeed6ActionRates dto.Ortho2Rates,
            Seed6ActionRatesDto.toSeed6ActionRates dto.Para1Rates,
            Seed6ActionRatesDto.toSeed6ActionRates dto.Para2Rates,
            Seed6ActionRatesDto.toSeed6ActionRates dto.Para3Rates,
            Seed6ActionRatesDto.toSeed6ActionRates dto.Para4Rates,
            Seed6ActionRatesDto.toSeed6ActionRates dto.SelfReflRates
        )

    let fromSeed6TransitionRates (domain: Seed6TransitionRates) : Seed6TransitionRatesDto = {
        Ortho1Rates = Seed6ActionRatesDto.fromSeed6ActionRates domain.Ortho1Rates
        Ortho2Rates = Seed6ActionRatesDto.fromSeed6ActionRates domain.Ortho2Rates
        Para1Rates = Seed6ActionRatesDto.fromSeed6ActionRates domain.Para1Rates
        Para2Rates = Seed6ActionRatesDto.fromSeed6ActionRates domain.Para2Rates
        Para3Rates = Seed6ActionRatesDto.fromSeed6ActionRates domain.Para3Rates
        Para4Rates = Seed6ActionRatesDto.fromSeed6ActionRates domain.Para4Rates
        SelfReflRates = Seed6ActionRatesDto.fromSeed6ActionRates domain.SelfReflRates
    }