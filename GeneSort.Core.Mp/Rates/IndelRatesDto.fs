namespace GeneSort.Core.Mp.RatesAndOps

open MessagePack
open GeneSort.Core


[<MessagePackObject>]
type indelRatesDto = {
    [<Key(0)>]
    mutationThresh: float
    [<Key(1)>]
    insertionThresh: float
    [<Key(2)>]
    deletionThresh: float
}

module IndelRatesDto =

    let toDomain (dto: indelRatesDto) : indelRates =
        indelRates.create (dto.mutationThresh, dto.insertionThresh - dto.mutationThresh, dto.deletionThresh - dto.insertionThresh)

    let fromDomain (domain: indelRates) : indelRatesDto = {
        mutationThresh = domain.MutationRate
        insertionThresh = domain.MutationRate + domain.InsertionRate
        deletionThresh = domain.MutationRate + domain.InsertionRate + domain.DeletionRate
    }

