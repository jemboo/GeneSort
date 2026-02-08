namespace GeneSort.Model.Mp.Sorter
open GeneSort.Model.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp



[<MessagePackObject>]
[<Union(0, typeof<sortingModelSingleDto>); Union(1, typeof<sortingModelPairDto>)>]
type sortingModelDto =
    | Single of sortingModelSingleDto
    | Pair of sortingModelPairDto

module SortingModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSortingModelDto (sortingModel: sortingModel) : sortingModelDto =
        match sortingModel with
        | sortingModel.Single single -> Single (SortingModelSingleDto.toSortingModelSingleDto single)
        | sortingModel.Pair pair -> Pair (SortingModelPairDto.toSortingModelPairDto pair)

    let fromSortingModelDto (dto: sortingModelDto) : sortingModel =
        try
            match dto with
            | Single singleDto -> sortingModel.Single (SortingModelSingleDto.fromSortingModelSingleDto singleDto)
            | Pair pairDto -> sortingModel.Pair (SortingModelPairDto.fromSortingModelPairDto pairDto)
        with
        | ex -> failwith $"Failed to convert SortingModelDto: {ex.Message}"