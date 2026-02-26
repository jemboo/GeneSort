namespace GeneSort.Model.Mp.SorterPair.SplitPairs
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Sorting
open GeneSort.Model.Mp.Sorting.Sorter

[<MessagePackObject>]
type msSplitPairsMutateDto =
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] firstPrefixMaker: sorterModelMutatorDto
      [<Key(2)>] firstSuffixMaker: sorterModelMutatorDto
      [<Key(3)>] secondPrefixMaker: sorterModelMutatorDto
      [<Key(4)>] secondSuffixMaker: sorterModelMutatorDto }

module MsSplitPairsMutateDto =

    let fromDomain (gen: msSplitPairsMutator) : msSplitPairsMutateDto =
        { sortingWidth = %(MsSplitPairsMutator.getSortingWidth gen)
          firstPrefixMaker = SorterModelMutatorDto.fromDomain gen.FirstPrefixMutator
          firstSuffixMaker = SorterModelMutatorDto.fromDomain gen.FirstSuffixMutator
          secondPrefixMaker = SorterModelMutatorDto.fromDomain gen.SecondPrefixMutator
          secondSuffixMaker = SorterModelMutatorDto.fromDomain gen.SecondSuffixMutator }

    let toDomain (dto: msSplitPairsMutateDto) : msSplitPairsMutator =
        try
            msSplitPairsMutator.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (SorterModelMutatorDto.toDomain dto.firstPrefixMaker)
                (SorterModelMutatorDto.toDomain dto.firstSuffixMaker)
                (SorterModelMutatorDto.toDomain dto.secondPrefixMaker)
                (SorterModelMutatorDto.toDomain dto.secondSuffixMaker)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"