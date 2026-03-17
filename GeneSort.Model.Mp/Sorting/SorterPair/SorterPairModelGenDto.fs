namespace GeneSort.Model.Mp.Sorting.SorterPair
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.SorterPair
open GeneSort.Model.Mp.SorterPair.SplitPairs

[<MessagePackObject; Union(0, typeof<msSplitPairsGenDto>); Union(1, typeof<msSplitPairsGenDto>)>]
type sorterPairModelGenDto =
    | SplitPairs of msSplitPairsGenDto
    | SplitPairs2 of msSplitPairsGenDto

module SorterPairModelGenDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sorterPairModelGen: sorterPairModelGen) : sorterPairModelGenDto =
        match sorterPairModelGen with
        | sorterPairModelGen.SplitPairs msSplitPairsGen ->
            SplitPairs (MsSplitPairsGenDto.fromDomain msSplitPairsGen)
        | sorterPairModelGen.SplitPairs2 msSplitPairsGen ->
            SplitPairs2 (MsSplitPairsGenDto.fromDomain msSplitPairsGen)

    let toDomain (dto: sorterPairModelGenDto) : sorterPairModelGen =
        try
            match dto with
            | SplitPairs msSplitPairsGenDto ->
                sorterPairModelGen.SplitPairs (MsSplitPairsGenDto.toDomain msSplitPairsGenDto)
            | SplitPairs2 msSplitPairsGenDto ->
                sorterPairModelGen.SplitPairs2 (MsSplitPairsGenDto.toDomain msSplitPairsGenDto)
        with
        | ex -> failwith $"Failed to convert sorterPairModelGenDto: {ex.Message}"