namespace GeneSort.Model.Mp.SorterPair.SplitPairs
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Sorting
open GeneSort.Model.Mp.Sorting.Sorter
open GeneSort.Core.Mp

[<MessagePackObject>]
type msSplitPairsMutateDto =
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] FirstPrefixGen: sorterModelMutatorDto
      [<Key(3)>] FirstSuffixGen: sorterModelMutatorDto
      [<Key(4)>] SecondPrefixGen: sorterModelMutatorDto
      [<Key(5)>] SecondSuffixGen: sorterModelMutatorDto }

module MsSplitPairsMutateDto =

    let fromDomain (gen: msSplitPairsMutator) : msSplitPairsMutateDto =
        { sortingWidth = %(MsSplitPairsMutator.getSortingWidth gen)
          rngFactoryDto = gen.RngFactory |> RngFactoryDto.fromDomain
          FirstPrefixGen = SorterModelMutatorDto.fromDomain gen.FirstPrefixMutator
          FirstSuffixGen = SorterModelMutatorDto.fromDomain gen.FirstSuffixMutator
          SecondPrefixGen = SorterModelMutatorDto.fromDomain gen.SecondPrefixMutator
          SecondSuffixGen = SorterModelMutatorDto.fromDomain gen.SecondSuffixMutator }

    let toDomain (dto: msSplitPairsMutateDto) : msSplitPairsMutator =
        try
            msSplitPairsMutator.create
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (SorterModelMutatorDto.toDomain dto.FirstPrefixGen)
                (SorterModelMutatorDto.toDomain dto.FirstSuffixGen)
                (SorterModelMutatorDto.toDomain dto.SecondPrefixGen)
                (SorterModelMutatorDto.toDomain dto.SecondSuffixGen)
        with
        | ex -> failwith $"Failed to convert msSplitPairsGenDto: {ex.Message}"