namespace GeneSort.Model.Mp.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting.SorterPair
open GeneSort.Model.Mp.Sorting.Sorter

[<MessagePackObject; Union(0, typeof<sorterModelMakerDto>); Union(1, typeof<sorterPairModelMakerDto>)>]
type sortingModelMutatorDto =
    | Single of sorterModelMutatorDto
    | Pair of sorterPairModelMutatorDto

module SortingModelMutatorDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sortingModelMaker: sortingModelMutator) : sortingModelMutatorDto =
        match sortingModelMaker with
        | sortingModelMutator.Single sorterModelMutator ->
            Single (SorterModelMutatorDto.fromDomain sorterModelMutator)
        | sortingModelMutator.Pair sorterPairModelMutator ->
            Pair (SorterPairModelMutatorDto.fromDomain sorterPairModelMutator)

    let toDomain (dto: sortingModelMutatorDto) : sortingModelMutator =
        try
            match dto with
            | Single sorterModelMakerDto ->
                sortingModelMutator.Single (SorterModelMutatorDto.toDomain sorterModelMakerDto)
            | Pair sorterPairModelMakerDto ->
                sortingModelMutator.Pair (SorterPairModelMutatorDto.toDomain sorterPairModelMakerDto)
        with
        | ex -> failwith $"Failed to convert sortingModelMakerDto: {ex.Message}"