namespace GeneSort.Model.Mp.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Sorting
open GeneSort.Model.Sorting.SorterPair.SplitPairs
open GeneSort.Model.Mp.Sorting.Sorter
open GeneSort.Model.Sorting

[<MessagePackObject>]
type msSplitPairsDto = {
    [<Key(0)>]
    SortingWidth: int
    [<Key(1)>]
    Id: Guid
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

    let fromDomain (model: msSplitPairs) : msSplitPairsDto =
        {
            SortingWidth = %model.SortingWidth
            Id = %model.Id
            FirstPrefix = SorterModelDto.fromDomain model.FirstPrefix
            FirstSuffix = SorterModelDto.fromDomain model.FirstSuffix
            SecondPrefix = SorterModelDto.fromDomain model.SecondPrefix
            SecondSuffix = SorterModelDto.fromDomain model.SecondSuffix
        }

    let toDomain (dto: msSplitPairsDto) : msSplitPairs =
        try
            msSplitPairs.create
                (UMX.tag<sorterPairModelId> dto.Id)
                (UMX.tag<sortingWidth> dto.SortingWidth)
                (SorterModelDto.toDomain dto.FirstPrefix)
                (SorterModelDto.toDomain dto.FirstSuffix)
                (SorterModelDto.toDomain dto.SecondPrefix)
                (SorterModelDto.toDomain dto.SecondSuffix)
        with
        | ex -> failwith $"Failed to convert MsSplitPairsDto: {ex.Message}"