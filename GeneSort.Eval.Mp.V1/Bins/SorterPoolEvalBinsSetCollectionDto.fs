namespace GeneSort.Eval.Mp.V1.Bins

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.Mp.V1.Bins

// ---------------------------------------------------------------------
// 1. DTO Definition
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterPoolEvalBinsSetCollectionDto = {
    [<Key(0)>] SorterPoolEvalBinsSetCollectionId: Guid
    [<Key(1)>] SorterPoolEvalBinsSets: sorterPoolEvalBinsSetDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolEvalBinsSetCollectionDto =

    let fromDomain (collection: sorterPoolBinsSetSeries) : sorterPoolEvalBinsSetCollectionDto = {
        SorterPoolEvalBinsSetCollectionId = %collection.SorterPoolEvalBinsSetCollectionId
        SorterPoolEvalBinsSets =
            collection.SorterPoolEvalBinsSets
            |> Map.values
            |> Seq.map SorterPoolEvalBinsSetDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolEvalBinsSetCollectionDto) : sorterPoolBinsSetSeries =
        let id = dto.SorterPoolEvalBinsSetCollectionId |> UMX.tag

        let setsMap =
            dto.SorterPoolEvalBinsSets
            |> Seq.map (fun setDto ->
                let set = SorterPoolEvalBinsSetDto.toDomain setDto
                (set.SorterPoolEvalBinsSetId, set))
            |> Map.ofSeq

        sorterPoolBinsSetSeries.recreate id setsMap