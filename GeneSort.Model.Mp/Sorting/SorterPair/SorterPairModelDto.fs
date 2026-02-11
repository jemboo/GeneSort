namespace GeneSort.Model.Mp.Sorting.SorterPair

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.SorterPair

[<MessagePackObject>]
[<Union(0, typeof<msSplitPairsDto>); Union(1, typeof<msSplitPairsDto>)>]
type sorterPairModelDto =
    | SplitPairs of msSplitPairsDto
    | SplitPairs2 of msSplitPairsDto

module SorterPairModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterPairModelDto (model: sorterPairModel) : sorterPairModelDto =
        match model with
        | sorterPairModel.SplitPairs sp -> SplitPairs (MsSplitPairsDto.toMsSplitPairsDto sp)
        | sorterPairModel.SplitPairs2 sp -> SplitPairs2 (MsSplitPairsDto.toMsSplitPairsDto sp)

    let fromSorterPairModelDto (dto: sorterPairModelDto) : sorterPairModel =
        try
            match dto with
            | SplitPairs spDto -> sorterPairModel.SplitPairs (MsSplitPairsDto.fromMsSplitPairsDto spDto)
            | SplitPairs2 spDto -> sorterPairModel.SplitPairs2 (MsSplitPairsDto.fromMsSplitPairsDto spDto)
        with
        | ex -> failwith $"Failed to convert SorterPairModelDto: {ex.Message}"