namespace GeneSort.Eval.Mp.V1.Bins.Soss

open System
open FSharp.UMX
open MessagePack
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Soss
open GeneSort.Eval.Mp.V1.Bins.Soss

// ---------------------------------------------------------------------
// 1. DTO Definition
// ---------------------------------------------------------------------

type sorterPoolBinsSetSeriesDto_Soss = {
    SorterPoolEvalBinsSetCollectionId: Guid
    SorterPoolEvalBinsSets: sorterPoolEvalBinsSetDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolBinsSetSeriesDto_Soss =

    let fromDomain (collection: sorterPoolBinsSetSeries_Soss) : sorterPoolBinsSetSeriesDto_Soss = {
        SorterPoolEvalBinsSetCollectionId = %collection.SorterPoolEvalBinsSetCollectionId
        SorterPoolEvalBinsSets =
            collection.SorterPoolEvalBinsSets
            |> Map.values
            |> Seq.map SorterPoolEvalBinsSetDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolBinsSetSeriesDto_Soss) : sorterPoolBinsSetSeries_Soss =
        let id = dto.SorterPoolEvalBinsSetCollectionId |> UMX.tag

        let setsMap =
            dto.SorterPoolEvalBinsSets
            |> Seq.map (fun setDto ->
                let set = SorterPoolEvalBinsSetDto.toDomain setDto
                (set.SorterPoolEvalBinsSetId, set))
            |> Map.ofSeq

        sorterPoolBinsSetSeries_Soss.recreate id setsMap