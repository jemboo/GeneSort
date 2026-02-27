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

    let fromDomain (sorting: sorting) : sortingModelDto =
        match sorting with
        | sorting.Single single -> Single (SorterModelDto.fromDomain single)
        | sorting.Pair pair -> Pair (SorterPairModelDto.fromDomain pair)


    let toDomain (dto: sortingModelDto) : sorting =
        try
            match dto with
            | Single smDto -> sorting.Single (SorterModelDto.toDomain smDto)
            | Pair pairDto -> sorting.Pair (SorterPairModelDto.toDomain pairDto)
        with
        | ex -> failwith $"Failed to convert SortingModelDto: {ex.Message}"