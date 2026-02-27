namespace GeneSort.Model.Mp.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting.SorterPair
open GeneSort.Model.Mp.Sorting.Sorter

[<MessagePackObject; Union(0, typeof<sorterModelMakerDto>); Union(1, typeof<sorterPairModelMakerDto>)>]
type sortingMutatorDto =
    | Single of sorterModelMutatorDto
    | Pair of sorterPairModelMutatorDto

module SortingMutatorDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sortingMaker: sortingMutator) : sortingMutatorDto =
        match sortingMaker with
        | sortingMutator.Single sorterModelMutator ->
            Single (SorterModelMutatorDto.fromDomain sorterModelMutator)
        | sortingMutator.Pair sorterPairModelMutator ->
            Pair (SorterPairModelMutatorDto.fromDomain sorterPairModelMutator)

    let toDomain (dto: sortingMutatorDto) : sortingMutator =
        try
            match dto with
            | Single sorterModelMakerDto ->
                sortingMutator.Single (SorterModelMutatorDto.toDomain sorterModelMakerDto)
            | Pair sorterPairModelMakerDto ->
                sortingMutator.Pair (SorterPairModelMutatorDto.toDomain sorterPairModelMakerDto)
        with
        | ex -> failwith $"Failed to convert sortingMakerDto: {ex.Message}"