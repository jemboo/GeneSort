namespace GeneSort.Core.Mp.RatesAndOps

open GeneSort.Core

type indelRatesDto = {
    mutationThresh: float
    insertionThresh: float
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