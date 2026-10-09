namespace GeneSort.Eval.Mp.V1.Sgd

open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.Mp.V1.Sgd.Soss
open GeneSort.Eval.Mp.V1.Bins
open GeneSort.Eval.Mp.V1.Bins.Soss

[<RequireQualifiedAccess>]
type sorterPoolSetDto =
    | Standard of sorterPoolSetDto_Standard
    | Soss of sorterPoolSetDto_Soss

module SorterPoolSetDto =
    let fromDomain = function
        | sorterPoolSet.Standard value -> sorterPoolSetDto.Standard (SorterPoolSetDto_Standard.toDto value)
        | sorterPoolSet.Soss value -> sorterPoolSetDto.Soss (SorterPoolSetDto_Soss.toDto value)
    let toDomain = function
        | sorterPoolSetDto.Standard value -> sorterPoolSet.Standard (SorterPoolSetDto_Standard.fromDto value)
        | sorterPoolSetDto.Soss value -> sorterPoolSet.Soss (SorterPoolSetDto_Soss.fromDto value)

[<RequireQualifiedAccess>]
type spSummarySetDto =
    | Standard of sorterPoolSetSummarySetDto_Standard
    | Soss of sorterPoolSetSummarySetDto_Soss

module SpSummarySetDto =
    let fromDomain = function
        | spSummarySet.Standard value -> spSummarySetDto.Standard (SorterPoolSetSummarySetDto_Standard.toDto value)
        | spSummarySet.Soss value -> spSummarySetDto.Soss (SorterPoolSetSummarySetDto_Soss.toDto value)
    let toDomain = function
        | spSummarySetDto.Standard value -> spSummarySet.Standard (SorterPoolSetSummarySetDto_Standard.fromDto value)
        | spSummarySetDto.Soss value -> spSummarySet.Soss (SorterPoolSetSummarySetDto_Soss.fromDto value)

[<RequireQualifiedAccess>]
type sorterPoolBinsSetSeriesDto =
    | Standard of sorterPoolBinsSetSeriesDto_Standard
    | Soss of sorterPoolBinsSetSeriesDto_Soss

module SorterPoolBinsSetSeriesDto =
    let fromDomain = function
        | sorterPoolBinsSetSeries.Standard value -> sorterPoolBinsSetSeriesDto.Standard (SorterPoolBinsSetSeriesDto_Standard.fromDomain value)
        | sorterPoolBinsSetSeries.Soss value -> sorterPoolBinsSetSeriesDto.Soss (SorterPoolBinsSetSeriesDto_Soss.fromDomain value)
    let toDomain = function
        | sorterPoolBinsSetSeriesDto.Standard value -> sorterPoolBinsSetSeries.Standard (SorterPoolBinsSetSeriesDto_Standard.toDomain value)
        | sorterPoolBinsSetSeriesDto.Soss value -> sorterPoolBinsSetSeries.Soss (SorterPoolBinsSetSeriesDto_Soss.toDomain value)

[<RequireQualifiedAccess>]
type spshDto =
    | Standard of sorterPoolSetHistoryDto_Standard
    | Soss of sorterPoolSetHistoryDto_Soss

module SpshDto =
    let fromDomain = function
        | spsh.Standard value -> spshDto.Standard (SorterPoolSetHistoryDto_Standard.fromDomain value)
        | spsh.Soss value -> spshDto.Soss (SorterPoolSetHistoryDto_Soss.fromDomain value)
    let toDomain = function
        | spshDto.Standard value -> spsh.Standard (SorterPoolSetHistoryDto_Standard.toDomain value)
        | spshDto.Soss value -> spsh.Soss (SorterPoolSetHistoryDto_Soss.toDomain value)
