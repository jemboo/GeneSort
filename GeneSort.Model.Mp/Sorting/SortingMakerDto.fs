namespace GeneSort.Model.Mp.Sorting
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.SorterPair
open GeneSort.Model.Mp.Sorting.Sorter
open GeneSort.Model.Mp.Sorting.SorterPair

[<MessagePackObject; Union(0, typeof<sorterModelGenDto>); Union(1, typeof<sorterPairModelGenDto>)>]
type sortingGenDto =
    | Single of sorterModelGenDto
    | Pair of sorterPairModelGenDto

module SortingGenDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sortingGen: sortingGen) : sortingGenDto =
        match sortingGen with
        | sortingGen.Single sorterModelGen ->
            Single (SorterModelGenDto.fromDomain sorterModelGen)
        | sortingGen.Pair sorterPairModelMaker ->
            Pair (SorterPairModelGenDto.fromDomain sorterPairModelMaker)


    let toDomain (dto: sortingGenDto) : sortingGen =
        try
            match dto with
            | Single sorterModelMakerDto ->
                sortingGen.Single (SorterModelGenDto.toDomain sorterModelMakerDto)
            | Pair sorterPairModelMakerDto ->
                sortingGen.Pair (SorterPairModelGenDto.toDomain sorterPairModelMakerDto)
        with
        | ex -> failwith $"Failed to convert sortingMakerDto: {ex.Message}"