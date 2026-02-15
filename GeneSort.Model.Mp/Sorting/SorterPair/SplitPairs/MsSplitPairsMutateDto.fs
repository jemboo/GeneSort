namespace GeneSort.Model.Mp.SorterPair.SplitPairs
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Sorting
open GeneSort.Model.Mp.Sorting.Sorter

[<MessagePackObject>]
type msSplitPairsMutateDto =
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] firstPrefixMaker: sorterModelMakerDto
      [<Key(2)>] firstSuffixMaker: sorterModelMakerDto
      [<Key(3)>] secondPrefixMaker: sorterModelMakerDto
      [<Key(4)>] secondSuffixMaker: sorterModelMakerDto }

module MsSplitPairsMutateDto =
    let toMsSplitPairsGenDto (gen: msSplitPairsMutator) : msSplitPairsMutateDto =
        { sortingWidth = %(MsSplitPairsMutator.getSortingWidth gen)
          firstPrefixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.FirstPrefixMaker
          firstSuffixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.FirstSuffixMaker
          secondPrefixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.SecondPrefixMaker
          secondSuffixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.SecondSuffixMaker }

    let fromMsSplitPairsGenDto (dto: msSplitPairsMutateDto) : msSplitPairsMutator =
        try
            msSplitPairsMutator.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.firstPrefixMaker)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.firstSuffixMaker)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.secondPrefixMaker)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.secondSuffixMaker)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"