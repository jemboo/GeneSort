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
      [<Key(2)>] firstPrefixGen: sorterModelGenDto
      [<Key(3)>] firstSuffixGen: sorterModelGenDto
      [<Key(4)>] secondPrefixGen: sorterModelGenDto
      [<Key(5)>] secondSuffixGen: sorterModelGenDto }

module MsSplitPairsGenDto =

    let fromDomain (gen: msSplitPairsGen) : msSplitPairsGenDto =
        { sortingWidth = %(MsSplitPairsGen.getSortingWidth gen)
          rngFactoryDto = gen.RngFactory |> RngFactoryDto.fromDomain
          firstPrefixGen = SorterModelGenDto.fromDomain gen.FirstPrefixGen
          firstSuffixGen = SorterModelGenDto.fromDomain gen.FirstSuffixGen
          secondPrefixGen = SorterModelGenDto.fromDomain gen.SecondPrefixGen
          secondSuffixGen = SorterModelGenDto.fromDomain gen.SecondSuffixGen }


    let toDomain (dto: msSplitPairsGenDto) : msSplitPairsGen =
        try
            msSplitPairsGen.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (SorterModelGenDto.toDomain dto.firstPrefixGen)
                (SorterModelGenDto.toDomain dto.firstSuffixGen)
                (SorterModelGenDto.toDomain dto.secondPrefixGen)
                (SorterModelGenDto.toDomain dto.secondSuffixGen)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"