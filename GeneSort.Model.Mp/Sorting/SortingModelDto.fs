namespace GeneSort.Model.Mp.Sorting
open GeneSort.Model.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Mp.Sorting.SorterPair
open GeneSort.Model.Mp.Sorting.Sorter


[<MessagePackObject>]
[<Union(0, typeof<sorterModelDto>); Union(1, typeof<sorterPairModelDto>)>]
type sortingModelDto =
    | Single of sorterModelDto
    | Pair of sorterPairModelDto


module SortingModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSortingModelDto (sortingModel: sortingModel) : sortingModelDto =
        match sortingModel with
        | sortingModel.Single single -> Single (SorterModelDto.toSorterModelDto single)
        | sortingModel.Pair pair -> Pair (SorterPairModelDto.toSorterPairModelDto pair)

    let fromSortingModelDto (dto: sortingModelDto) : sortingModel =
        try
            match dto with
            | Single smDto -> sortingModel.Single (SorterModelDto.fromSorterModelDto smDto)
            | Pair pairDto -> sortingModel.Pair (SorterPairModelDto.fromSorterPairModelDto pairDto)
        with
        | ex -> failwith $"Failed to convert SortingModelDto: {ex.Message}"