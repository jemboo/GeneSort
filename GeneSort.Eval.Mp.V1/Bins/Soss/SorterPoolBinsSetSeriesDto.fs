namespace GeneSort.Eval.Mp.V1.Bins.Soss

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.Mp.V1.Bins.Soss
open GeneSort.Eval.V1.Sgd.Bins.Soss

// ---------------------------------------------------------------------
// 1. DTO Definition
// ---------------------------------------------------------------------

type sorterPoolBinsSetSeriesDto = {
    SorterPoolEvalBinsSetCollectionId: Guid
    SorterPoolEvalBinsSets: sorterPoolEvalBinsSetDto array
}

// ---------------------------------------------------------------------
// 2. Conversion Module
// ---------------------------------------------------------------------

module SorterPoolBinsSetSeriesDto =

    let fromDomain (collection: sorterPoolBinsSetSeries_Soss) : sorterPoolBinsSetSeriesDto = {
        SorterPoolEvalBinsSetCollectionId = %collection.SorterPoolEvalBinsSetCollectionId
        SorterPoolEvalBinsSets =
            collection.SorterPoolEvalBinsSets
            |> Map.values
            |> Seq.map SorterPoolEvalBinsSetDto.fromDomain
            |> Seq.toArray
    }

    let toDomain (dto: sorterPoolBinsSetSeriesDto) : sorterPoolBinsSetSeries_Soss =
        let id = dto.SorterPoolEvalBinsSetCollectionId |> UMX.tag

        let setsMap =
            dto.SorterPoolEvalBinsSets
            |> Seq.map (fun setDto ->
                let set = SorterPoolEvalBinsSetDto.toDomain setDto
                (set.SorterPoolEvalBinsSetId, set))
            |> Map.ofSeq

        sorterPoolBinsSetSeries_Soss.recreate id setsMap