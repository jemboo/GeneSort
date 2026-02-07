namespace GeneSort.Model.Mp.Sorter
open GeneSort.Model.Sorter
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp


[<MessagePackObject>]
[<Union(0, typeof<sorterModelDto>); Union(1, typeof<sorterPairModelDto>)>]
type sortingModelDto =
    | Sorter of sorterModelDto
    | SorterPair of sorterPairModelDto

module SortingModelDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSortingModelDto (sortingModel: sortingModel) : sortingModelDto =
        match sortingModel with
        | sortingModel.Sorter sorter -> Sorter (SorterModelDto.toSorterModelDto sorter)
        | sortingModel.SorterPair sorterPair -> SorterPair (SorterPairModelDto.toSorterModelDto sorterPair)

    let fromSortingModelDto (dto: sortingModelDto) : sortingModel =
        try
            match dto with
            | Sorter sorterDto -> sortingModel.Sorter (SorterModelDto.fromSorterModelDto sorterDto)
            | SorterPair sorterPairDto -> sortingModel.SorterPair (SorterPairModelDto.fromSorterModelDto sorterPairDto)
        with
        | ex -> failwith $"Failed to convert SortingModelDto: {ex.Message}"