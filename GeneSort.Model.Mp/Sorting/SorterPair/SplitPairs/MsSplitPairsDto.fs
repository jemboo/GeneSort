namespace GeneSort.Model.Mp.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.SorterPair
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Sorting
open GeneSort.Sorting
open GeneSort.Model.Sorting.SorterPair.SplitPairs

[<MessagePackObject>]
type msSplitPairsDto = {
    [<Key(0)>]
    Id: Guid
    [<Key(1)>]
    SortingWidth: int
    [<Key(2)>]
    FirstPrefix: sorterModelDto
    [<Key(3)>]
    FirstSuffix: sorterModelDto
    [<Key(4)>]
    SecondPrefix: sorterModelDto
    [<Key(5)>]
    SecondSuffix: sorterModelDto
}

module MsSplitPairsDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsSplitPairsDto (model: msSplitPairs) : msSplitPairsDto =
        {
            Id = %model.Id
            SortingWidth = %model.SortingWidth
            FirstPrefix = SorterModelDto.toSorterModelDto model.FirstPrefix
            FirstSuffix = SorterModelDto.toSorterModelDto model.FirstSuffix
            SecondPrefix = SorterModelDto.toSorterModelDto model.SecondPrefix
            SecondSuffix = SorterModelDto.toSorterModelDto model.SecondSuffix
        }

    let fromMsSplitPairsDto (dto: msSplitPairsDto) : msSplitPairs =
        try
            msSplitPairs.create
                (UMX.tag<sorterModelID> dto.Id)
                (UMX.tag<sortingWidth> dto.SortingWidth)
                (SorterModelDto.fromSorterModelDto dto.FirstPrefix)
                (SorterModelDto.fromSorterModelDto dto.FirstSuffix)
                (SorterModelDto.fromSorterModelDto dto.SecondPrefix)
                (SorterModelDto.fromSorterModelDto dto.SecondSuffix)
        with
        | ex -> failwith $"Failed to convert MsSplitPairsDto: {ex.Message}"