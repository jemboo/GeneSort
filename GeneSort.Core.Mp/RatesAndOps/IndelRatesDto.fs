namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type IndelRatesDto = {
    [<Key(0)>]
    MutationThresh: float
    [<Key(1)>]
    InsertionThresh: float
    [<Key(2)>]
    DeletionThresh: float
}

module IndelRatesDto =
    let toIndelRates (dto: IndelRatesDto) : IndelRates =
        IndelRates.create (dto.MutationThresh, dto.InsertionThresh - dto.MutationThresh, dto.DeletionThresh - dto.InsertionThresh)

    let fromIndelRates (domain: IndelRates) : IndelRatesDto = {
        MutationThresh = domain.MutationRate
        InsertionThresh = domain.MutationRate + domain.InsertionRate
        DeletionThresh = domain.MutationRate + domain.InsertionRate + domain.DeletionRate
    }

