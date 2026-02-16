namespace GeneSort.Model.Mp.Sorting
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting
open GeneSort.Sorting

[<MessagePackObject>]
type sortingModelSetMakerDto =
    { [<Key(0)>] sortingModelMaker: sortingModelMakerDto
      [<Key(1)>] firstIndex: int
      [<Key(2)>] count: int }

module SortingModelSetMakerDto =

    let fromDomain (maker: sortingModelSetMaker) : sortingModelSetMakerDto =
        { sortingModelMaker = SortingModelMakerDto.fromDomain maker.SortingModelMaker
          firstIndex = %maker.FirstIndex
          count = %maker.Count }

    let toDomain (dto: sortingModelSetMakerDto) : sortingModelSetMaker =
        try
            sortingModelSetMaker.create
                (SortingModelMakerDto.toDomain dto.sortingModelMaker)
                (UMX.tag<sorterCount> dto.firstIndex)
                (UMX.tag<sorterCount> dto.count)
        with
        | ex -> failwith $"Failed to convert sortingModelSetMakerDto: {ex.Message}"