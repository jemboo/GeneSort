namespace GeneSort.Model.Mp.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.SorterPair

[<MessagePackObject; Union(0, typeof<SorterModelMakerDto>); Union(1, typeof<sorterPairModelMakerDto>)>]
type sortingModelMakerDto =
    | Single of SorterModelMakerDto
    | Pair of sorterPairModelMakerDto

module SortingModelMakerDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sortingModelMaker: sortingModelMaker) : sortingModelMakerDto =
        match sortingModelMaker with
        | sortingModelMaker.Single sorterModelMaker ->
            Single (SorterModelMakerDto.toSorterModelMakerDto sorterModelMaker)
        | sortingModelMaker.Pair sorterPairModelMaker ->
            Pair (SorterPairModelMakerDto.fromDomain sorterPairModelMaker)

    let toDomain (dto: sortingModelMakerDto) : sortingModelMaker =
        try
            match dto with
            | Single sorterModelMakerDto ->
                sortingModelMaker.Single (SorterModelMakerDto.fromSorterModelMakerDto sorterModelMakerDto)
            | Pair sorterPairModelMakerDto ->
                sortingModelMaker.Pair (SorterPairModelMakerDto.toDomain sorterPairModelMakerDto)
        with
        | ex -> failwith $"Failed to convert sortingModelMakerDto: {ex.Message}"