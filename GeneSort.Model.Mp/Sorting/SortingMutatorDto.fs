namespace GeneSort.Model.Mp.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting.SorterPair
open GeneSort.Model.Mp.Sorting.Sorter

[<MessagePackObject; Union(0, typeof<sorterModelGenDto>); Union(1, typeof<sorterPairModelGenDto>)>]
type sortingMutatorDto =
    | Single of sorterModelMutatorDto
    | Pair of sorterPairModelMutatorDto

module SortingMutatorDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sortingMutator: sortingMutator) : sortingMutatorDto =
        match sortingMutator with
        | sortingMutator.Single sorterModelMutator ->
            Single (SorterModelMutatorDto.fromDomain sorterModelMutator)
        | sortingMutator.Pair sorterPairModelMutator ->
            Pair (SorterPairModelMutatorDto.fromDomain sorterPairModelMutator)

    let toDomain (dto: sortingMutatorDto) : sortingMutator =
        try
            match dto with
            | Single sorterModelMutatorDto ->
                sortingMutator.Single (SorterModelMutatorDto.toDomain sorterModelMutatorDto)
            | Pair sorterPairModelMutatorDto ->
                sortingMutator.Pair (SorterPairModelMutatorDto.toDomain sorterPairModelMutatorDto)
        with
        | ex -> failwith $"Failed to convert sortingMutatorDto: {ex.Message}"