namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type Seed6GenRatesDto = {
    [<Key(0)>]
    Ortho1Thresh: float
    [<Key(1)>]
    Ortho2Thresh: float
    [<Key(2)>]
    Para1Thresh: float
    [<Key(3)>]
    Para2Thresh: float
    [<Key(4)>]
    Para3Thresh: float
    [<Key(5)>]
    Para4Thresh: float
    [<Key(6)>]
    SelfReflThresh: float
}

module Seed6GenRatesDto =

    let fromDomain (dto: Seed6GenRatesDto) : Seed6GenRates =
        Seed6GenRates.create(
            dto.Ortho1Thresh,
            dto.Ortho2Thresh - dto.Ortho1Thresh,
            dto.Para1Thresh - dto.Ortho2Thresh,
            dto.Para2Thresh - dto.Para1Thresh,
            dto.Para3Thresh - dto.Para2Thresh,
            dto.Para4Thresh - dto.Para3Thresh,
            dto.SelfReflThresh - dto.Para4Thresh
        )

    let toDomain (domain: Seed6GenRates) : Seed6GenRatesDto = {
        Ortho1Thresh = domain.Ortho1Rate
        Ortho2Thresh = domain.Ortho1Rate + domain.Ortho2Rate
        Para1Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate
        Para2Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate
        Para3Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate + domain.Para3Rate
        Para4Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate + domain.Para3Rate + domain.Para4Rate
        SelfReflThresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate + domain.Para3Rate + domain.Para4Rate + domain.SelfReflRate
    }