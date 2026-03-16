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
      [<Key(2)>] firstPrefixMaker: sorterModelGenDto
      [<Key(3)>] firstSuffixMaker: sorterModelGenDto
      [<Key(4)>] secondPrefixMaker: sorterModelGenDto
      [<Key(5)>] secondSuffixMaker: sorterModelGenDto }

module MsSplitPairsGenDto =

    let fromDomain (gen: msSplitPairsGen) : msSplitPairsGenDto =
        { sortingWidth = %(MsSplitPairsGen.getSortingWidth gen)
          rngFactoryDto = gen.RngFactory |> RngFactoryDto.fromDomain
          firstPrefixMaker = SorterModelGenDto.fromDomain gen.FirstPrefixMaker
          firstSuffixMaker = SorterModelGenDto.fromDomain gen.FirstSuffixMaker
          secondPrefixMaker = SorterModelGenDto.fromDomain gen.SecondPrefixMaker
          secondSuffixMaker = SorterModelGenDto.fromDomain gen.SecondSuffixMaker }


    let toDomain (dto: msSplitPairsGenDto) : msSplitPairsGen =
        try
            msSplitPairsGen.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (SorterModelGenDto.toDomain dto.firstPrefixMaker)
                (SorterModelGenDto.toDomain dto.firstSuffixMaker)
                (SorterModelGenDto.toDomain dto.secondPrefixMaker)
                (SorterModelGenDto.toDomain dto.secondSuffixMaker)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"