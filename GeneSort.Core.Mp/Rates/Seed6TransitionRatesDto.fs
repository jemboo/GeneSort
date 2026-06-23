namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type Seed6TransitionRatesDto = {
    [<Key(0)>]
    ortho1Rates: Seed6ActionRatesDto
    [<Key(1)>]
    ortho2Rates: Seed6ActionRatesDto
    [<Key(2)>]
    para1Rates: Seed6ActionRatesDto
    [<Key(3)>]
    para2Rates: Seed6ActionRatesDto
    [<Key(4)>]
    para3Rates: Seed6ActionRatesDto
    [<Key(5)>]
    para4Rates: Seed6ActionRatesDto
    [<Key(6)>]
    selfReflRates: Seed6ActionRatesDto
}

module Seed6TransitionRatesDto =

    let toDomain (dto: Seed6TransitionRatesDto) : seed6TransitionRates =
        seed6TransitionRates.create(
            Seed6ActionRatesDto.toDomain dto.ortho1Rates,
            Seed6ActionRatesDto.toDomain dto.ortho2Rates,
            Seed6ActionRatesDto.toDomain dto.para1Rates,
            Seed6ActionRatesDto.toDomain dto.para2Rates,
            Seed6ActionRatesDto.toDomain dto.para3Rates,
            Seed6ActionRatesDto.toDomain dto.para4Rates,
            Seed6ActionRatesDto.toDomain dto.selfReflRates
        )

    let fromDomain (domain: seed6TransitionRates) : Seed6TransitionRatesDto = {
        ortho1Rates = Seed6ActionRatesDto.fromDomain domain.Ortho1Rates
        ortho2Rates = Seed6ActionRatesDto.fromDomain domain.Ortho2Rates
        para1Rates = Seed6ActionRatesDto.fromDomain domain.Para1Rates
        para2Rates = Seed6ActionRatesDto.fromDomain domain.Para2Rates
        para3Rates = Seed6ActionRatesDto.fromDomain domain.Para3Rates
        para4Rates = Seed6ActionRatesDto.fromDomain domain.Para4Rates
        selfReflRates = Seed6ActionRatesDto.fromDomain domain.SelfReflRates
    }