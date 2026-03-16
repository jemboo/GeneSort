namespace GeneSort.Model.Mp.Sorting
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting
open GeneSort.Sorting

[<MessagePackObject>]
type sortingSetMakerDto =
    { [<Key(0)>] sortingMaker: sortingGenDto
      [<Key(1)>] firstIndex: int
      [<Key(2)>] count: int }

module SortingSetMakerDto =

    let fromDomain (maker: sortingGenSegment) : sortingSetMakerDto =
        { sortingMaker = SortingGenDto.fromDomain maker.SortingMaker
          firstIndex = %maker.FirstIndex
          count = %maker.Count }

    let toDomain (dto: sortingSetMakerDto) : sortingGenSegment =
        try
            sortingGenSegment.create
                (SortingGenDto.toDomain dto.sortingMaker)
                (UMX.tag<sorterCount> dto.firstIndex)
                (UMX.tag<sorterCount> dto.count)
        with
        | ex -> failwith $"Failed to convert sortingSetMakerDto: {ex.Message}"