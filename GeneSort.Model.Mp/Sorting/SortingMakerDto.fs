namespace GeneSort.Model.Mp.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.SorterPair
open GeneSort.Model.Mp.Sorting.Sorter
open GeneSort.Model.Mp.Sorting.SorterPair

[<MessagePackObject; Union(0, typeof<sorterModelMakerDto>); Union(1, typeof<sorterPairModelMakerDto>)>]
type sortingMakerDto =
    | Single of sorterModelMakerDto
    | Pair of sorterPairModelMakerDto

module SortingMakerDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sortingMaker: sortingMaker) : sortingMakerDto =
        match sortingMaker with
        | sortingMaker.Single sorterModelMaker ->
            Single (SorterModelMakerDto.fromDomain sorterModelMaker)
        | sortingMaker.Pair sorterPairModelMaker ->
            Pair (SorterPairModelMakerDto.fromDomain sorterPairModelMaker)


    let toDomain (dto: sortingMakerDto) : sortingMaker =
        try
            match dto with
            | Single sorterModelMakerDto ->
                sortingMaker.Single (SorterModelMakerDto.toDomain sorterModelMakerDto)
            | Pair sorterPairModelMakerDto ->
                sortingMaker.Pair (SorterPairModelMakerDto.toDomain sorterPairModelMakerDto)
        with
        | ex -> failwith $"Failed to convert sortingMakerDto: {ex.Message}"