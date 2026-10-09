namespace GeneSort.Eval.Mp.V1.Sgd

open GeneSort.Eval.V1.Sgd

[<RequireQualifiedAccess>]
type sorterPoolSetUnionDto =
    | Standard of sorterPoolSetDto
    | Soss of Soss.sorterPoolSetDto

module SorterPoolSetUnionDto =
    let fromDomain = function
        | sorterPoolSet.Standard value -> sorterPoolSetUnionDto.Standard (SorterPoolSetDto.toDto value)
        | sorterPoolSet.Soss value -> sorterPoolSetUnionDto.Soss (Soss.SorterPoolSetDto.toDto value)
    let toDomain = function
        | sorterPoolSetUnionDto.Standard value -> sorterPoolSet.Standard (SorterPoolSetDto.fromDto value)
        | sorterPoolSetUnionDto.Soss value -> sorterPoolSet.Soss (Soss.SorterPoolSetDto.fromDto value)

[<RequireQualifiedAccess>]
type spSummarySetUnionDto =
    | Standard of sorterPoolSetSummarySetDto
    | Soss of Soss.sorterPoolSetSummarySetDto

module SpSummarySetUnionDto =
    let fromDomain = function
        | spSummarySet.Standard value -> spSummarySetUnionDto.Standard (SorterPoolSetSummarySetDto.toDto value)
        | spSummarySet.Soss value -> spSummarySetUnionDto.Soss (Soss.SorterPoolSetSummarySetDto.toDto value)
    let toDomain = function
        | spSummarySetUnionDto.Standard value -> spSummarySet.Standard (SorterPoolSetSummarySetDto.fromDto value)
        | spSummarySetUnionDto.Soss value -> spSummarySet.Soss (Soss.SorterPoolSetSummarySetDto.fromDto value)

[<RequireQualifiedAccess>]
type sorterPoolBinsSetSeriesUnionDto =
    | Standard of GeneSort.Eval.Mp.V1.Bins.sorterPoolBinsSetSeriesDto
    | Soss of GeneSort.Eval.Mp.V1.Bins.Soss.sorterPoolBinsSetSeriesDto

module SorterPoolBinsSetSeriesUnionDto =
    let fromDomain = function
        | sorterPoolBinsSetSeries.Standard value -> sorterPoolBinsSetSeriesUnionDto.Standard (GeneSort.Eval.Mp.V1.Bins.SorterPoolBinsSetSeriesDto.fromDomain value)
        | sorterPoolBinsSetSeries.Soss value -> sorterPoolBinsSetSeriesUnionDto.Soss (GeneSort.Eval.Mp.V1.Bins.Soss.SorterPoolBinsSetSeriesDto.fromDomain value)
    let toDomain = function
        | sorterPoolBinsSetSeriesUnionDto.Standard value -> sorterPoolBinsSetSeries.Standard (GeneSort.Eval.Mp.V1.Bins.SorterPoolBinsSetSeriesDto.toDomain value)
        | sorterPoolBinsSetSeriesUnionDto.Soss value -> sorterPoolBinsSetSeries.Soss (GeneSort.Eval.Mp.V1.Bins.Soss.SorterPoolBinsSetSeriesDto.toDomain value)

[<RequireQualifiedAccess>]
type spshUnionDto =
    | Standard of sorterPoolSetHistoryDto
    | Soss of Soss.sorterPoolSetHistoryDto

module SpshUnionDto =
    let fromDomain = function
        | spsh.Standard value -> spshUnionDto.Standard (SorterPoolSetHistoryDto.fromDomain value)
        | spsh.Soss value -> spshUnionDto.Soss (Soss.SorterPoolSetHistoryDto.fromDomain value)
    let toDomain = function
        | spshUnionDto.Standard value -> spsh.Standard (SorterPoolSetHistoryDto.toDomain value)
        | spshUnionDto.Soss value -> spsh.Soss (Soss.SorterPoolSetHistoryDto.toDomain value)
