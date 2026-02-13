namespace GeneSort.Model.Mp.SorterPair
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Model.Mp.Sorter
open GeneSort.Sorting

[<MessagePackObject>]
type msSplitPairsGenDto =
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] firstPrefixMaker: SorterModelMakerDto
      [<Key(2)>] firstSuffixMaker: SorterModelMakerDto
      [<Key(3)>] secondPrefixMaker: SorterModelMakerDto
      [<Key(4)>] secondSuffixMaker: SorterModelMakerDto }

module MsSplitPairsGenDto =
    let toMsSplitPairsGenDto (gen: msSplitPairsGen) : msSplitPairsGenDto =
        { sortingWidth = %(MsSplitPairsGen.getSortingWidth gen)
          firstPrefixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.FirstPrefixMaker
          firstSuffixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.FirstSuffixMaker
          secondPrefixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.SecondPrefixMaker
          secondSuffixMaker = SorterModelMakerDto.toSorterModelMakerDto gen.SecondSuffixMaker }

    let fromMsSplitPairsGenDto (dto: msSplitPairsGenDto) : msSplitPairsGen =
        try
            msSplitPairsGen.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.firstPrefixMaker)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.firstSuffixMaker)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.secondPrefixMaker)
                (SorterModelMakerDto.fromSorterModelMakerDto dto.secondSuffixMaker)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"