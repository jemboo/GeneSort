namespace GeneSort.Model.Mp.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Sorting
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Model.Mp.Sorting.Sorter

[<MessagePackObject>]
type msSplitPairsDto = {
    [<Key(0)>]
    SortingWidth: int
    [<Key(1)>]
    FirstPrefix: sorterModelDto
    [<Key(2)>]
    FirstSuffix: sorterModelDto
    [<Key(3)>]
    SecondPrefix: sorterModelDto
    [<Key(4)>]
    SecondSuffix: sorterModelDto
}

module MsSplitPairsDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (model: msSplitPairs) : msSplitPairsDto =
        {
            SortingWidth = %model.SortingWidth
            FirstPrefix = SorterModelDto.fromDomain model.FirstPrefix
            FirstSuffix = SorterModelDto.fromDomain model.FirstSuffix
            SecondPrefix = SorterModelDto.fromDomain model.SecondPrefix
            SecondSuffix = SorterModelDto.fromDomain model.SecondSuffix
        }

    let toDomain (dto: msSplitPairsDto) : msSplitPairs =
        try
            msSplitPairs.create
                (UMX.tag<sortingWidth> dto.SortingWidth)
                (SorterModelDto.toDomain dto.FirstPrefix)
                (SorterModelDto.toDomain dto.FirstSuffix)
                (SorterModelDto.toDomain dto.SecondPrefix)
                (SorterModelDto.toDomain dto.SecondSuffix)
        with
        | ex -> failwith $"Failed to convert MsSplitPairsDto: {ex.Message}"