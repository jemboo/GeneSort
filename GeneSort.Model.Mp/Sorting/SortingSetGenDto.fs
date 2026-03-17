namespace GeneSort.Model.Mp.Sorting
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting
open GeneSort.Sorting

[<MessagePackObject>]
type sortingSetGenDto =
    { [<Key(0)>] sortingGen: sortingGenDto
      [<Key(1)>] firstIndex: int
      [<Key(2)>] count: int }

module SortingSetGenDto =

    let fromDomain (gen: sortingGenSegment) : sortingSetGenDto =
        { sortingGen = SortingGenDto.fromDomain gen.SortingGen
          firstIndex = %gen.FirstIndex
          count = %gen.Count }

    let toDomain (dto: sortingSetGenDto) : sortingGenSegment =
        try
            sortingGenSegment.create
                (SortingGenDto.toDomain dto.sortingGen)
                (UMX.tag<sorterCount> dto.firstIndex)
                (UMX.tag<sorterCount> dto.count)
        with
        | ex -> failwith $"Failed to convert sortingSetGenDto: {ex.Message}"