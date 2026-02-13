namespace GeneSort.Model.Mp.SorterPair
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.SorterPair

[<MessagePackObject; Union(0, typeof<msSplitPairsGenDto>); Union(1, typeof<msSplitPairsGenDto>)>]
type sorterPairModelMakerDto =
    | SplitPairs of msSplitPairsGenDto
    | SplitPairs2 of msSplitPairsGenDto

module SorterPairModelMakerDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sorterPairModelMaker: sorterPairModelMaker) : sorterPairModelMakerDto =
        match sorterPairModelMaker with
        | sorterPairModelMaker.SplitPairs msSplitPairsGen ->
            SplitPairs (MsSplitPairsGenDto.toMsSplitPairsGenDto msSplitPairsGen)
        | sorterPairModelMaker.SplitPairs2 msSplitPairsGen ->
            SplitPairs2 (MsSplitPairsGenDto.toMsSplitPairsGenDto msSplitPairsGen)

    let toDomain (dto: sorterPairModelMakerDto) : sorterPairModelMaker =
        try
            match dto with
            | SplitPairs msSplitPairsGenDto ->
                sorterPairModelMaker.SplitPairs (MsSplitPairsGenDto.fromMsSplitPairsGenDto msSplitPairsGenDto)
            | SplitPairs2 msSplitPairsGenDto ->
                sorterPairModelMaker.SplitPairs2 (MsSplitPairsGenDto.fromMsSplitPairsGenDto msSplitPairsGenDto)
        with
        | ex -> failwith $"Failed to convert sorterPairModelMakerDto: {ex.Message}"