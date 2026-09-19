namespace GeneSort.Sorting.Mp.Sorter

open GeneSort.Sorting.Sorter

type ceDto = {
    Index: int
}

module CeDto =

    let fromDomain (ce: ce) : ceDto =
        { Index = Ce.toIndex ce }

    let toDomain (dto: ceDto) : ce =
        Ce.fromIndex dto.Index