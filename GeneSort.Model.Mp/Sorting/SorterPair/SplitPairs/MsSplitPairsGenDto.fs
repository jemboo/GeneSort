namespace GeneSort.Model.Mp.SorterPair.SplitPairs
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Model.Mp.Sorter
open GeneSort.Sorting
open GeneSort.Model.Mp.Sorting.Sorter
open GeneSort.Core.Mp

[<MessagePackObject>]
type msSplitPairsGenDto =
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] firstPrefixMaker: sorterModelMakerDto
      [<Key(3)>] firstSuffixMaker: sorterModelMakerDto
      [<Key(4)>] secondPrefixMaker: sorterModelMakerDto
      [<Key(5)>] secondSuffixMaker: sorterModelMakerDto }

module MsSplitPairsGenDto =

    let fromDomain (gen: msSplitPairsGen) : msSplitPairsGenDto =
        { sortingWidth = %(MsSplitPairsGen.getSortingWidth gen)
          rngFactoryDto = gen.RngFactory |> RngFactoryDto.fromDomain
          firstPrefixMaker = SorterModelMakerDto.fromDomain gen.FirstPrefixMaker
          firstSuffixMaker = SorterModelMakerDto.fromDomain gen.FirstSuffixMaker
          secondPrefixMaker = SorterModelMakerDto.fromDomain gen.SecondPrefixMaker
          secondSuffixMaker = SorterModelMakerDto.fromDomain gen.SecondSuffixMaker }


    let toDomain (dto: msSplitPairsGenDto) : msSplitPairsGen =
        try
            msSplitPairsGen.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (SorterModelMakerDto.toDomain dto.firstPrefixMaker)
                (SorterModelMakerDto.toDomain dto.firstSuffixMaker)
                (SorterModelMakerDto.toDomain dto.secondPrefixMaker)
                (SorterModelMakerDto.toDomain dto.secondSuffixMaker)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"