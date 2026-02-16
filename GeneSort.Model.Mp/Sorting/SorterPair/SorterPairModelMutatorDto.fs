namespace GeneSort.Model.Mp.Sorting.SorterPair
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.SorterPair
open GeneSort.Model.Mp.SorterPair.SplitPairs

[<MessagePackObject; Union(0, typeof<msSplitPairsMutateDto>); Union(1, typeof<msSplitPairsMutateDto>)>]
type sorterPairModelMutatorDto =
    | SplitPairs of msSplitPairsMutateDto
    | SplitPairs2 of msSplitPairsMutateDto

module SorterPairModelMutatorDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sorterPairModelMutator: sorterPairModelMutator) : sorterPairModelMutatorDto =
        match sorterPairModelMutator with
        | sorterPairModelMutator.SplitPairs msSplitPairsGen ->
            SplitPairs (MsSplitPairsMutateDto.fromDomain msSplitPairsGen)
        | sorterPairModelMutator.SplitPairs2 msSplitPairsGen ->
            SplitPairs2 (MsSplitPairsMutateDto.fromDomain msSplitPairsGen)

    let toDomain (dto: sorterPairModelMutatorDto) : sorterPairModelMutator =
        try
            match dto with
            | SplitPairs msSplitPairsMutateDto ->
                sorterPairModelMutator.SplitPairs (MsSplitPairsMutateDto.toDomain msSplitPairsMutateDto)
            | SplitPairs2 msSplitPairsMutateDto ->
                sorterPairModelMutator.SplitPairs2 (MsSplitPairsMutateDto.toDomain msSplitPairsMutateDto)
        with
        | ex -> failwith $"Failed to convert sorterPairModelMakerDto: {ex.Message}"