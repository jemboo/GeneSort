namespace GeneSort.Core.Mp.RatesAndOps
open MessagePack
open GeneSort.Core

[<MessagePackObject>]
type Seed6ActionRatesDto = {
    [<Key(0)>]
    ortho1Thresh: float
    [<Key(1)>]
    ortho2Thresh: float
    [<Key(2)>]
    para1Thresh: float
    [<Key(3)>]
    para2Thresh: float
    [<Key(4)>]
    para3Thresh: float
    [<Key(5)>]
    para4Thresh: float
    [<Key(6)>]
    selfReflThresh: float
}

module Seed6ActionRatesDto =

    let toDomain (dto: Seed6ActionRatesDto) : seed6ActionRates =
        seed6ActionRates.create(
            dto.ortho1Thresh,
            dto.ortho2Thresh - dto.ortho1Thresh,
            dto.para1Thresh - dto.ortho2Thresh,
            dto.para2Thresh - dto.para1Thresh,
            dto.para3Thresh - dto.para2Thresh,
            dto.para4Thresh - dto.para3Thresh,
            dto.selfReflThresh - dto.para4Thresh
        )

    let fromDomain (domain: seed6ActionRates) : Seed6ActionRatesDto = {
        ortho1Thresh = domain.Ortho1Rate
        ortho2Thresh = domain.Ortho1Rate + domain.Ortho2Rate
        para1Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate
        para2Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate
        para3Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate + domain.Para3Rate
        para4Thresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate + domain.Para3Rate + domain.Para4Rate
        selfReflThresh = domain.Ortho1Rate + domain.Ortho2Rate + domain.Para1Rate + domain.Para2Rate + domain.Para3Rate + domain.Para4Rate + domain.SelfReflRate
    }