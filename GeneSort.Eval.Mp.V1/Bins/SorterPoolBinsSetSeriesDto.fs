namespace GeneSort.Eval.Mp.V1.Bins

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.Mp.V1.Bins
open GeneSort.Eval.V1.Sgd.Bins.Standard

// ---------------------------------------------------------------------
// 1. DTO Definition
// ---------------------------------------------------------------------

type sorterPoolBinsSetSeriesDto_Standard = {
    SorterPoolEvalBinsSetCollectionId: Guid
    SorterPoolEvalBinsSets: sorterPoolEvalBinsSetDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolBinsSetSeriesDto_Standard =

    let fromDomain (collection: sorterPoolBinsSetSeries_Standard) : sorterPoolBinsSetSeriesDto_Standard = {
        SorterPoolEvalBinsSetCollectionId = %collection.SorterPoolEvalBinsSetCollectionId
        SorterPoolEvalBinsSets =
            collection.SorterPoolEvalBinsSets
            |> Map.values
            |> Seq.map SorterPoolEvalBinsSetDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolBinsSetSeriesDto_Standard) : sorterPoolBinsSetSeries_Standard =
        let id = dto.SorterPoolEvalBinsSetCollectionId |> UMX.tag

        let setsMap =
            dto.SorterPoolEvalBinsSets
            |> Seq.map (fun setDto ->
                let set = SorterPoolEvalBinsSetDto.toDomain setDto
                (set.SorterPoolEvalBinsSetId, set))
            |> Map.ofSeq

        sorterPoolBinsSetSeries_Standard.recreate id setsMap