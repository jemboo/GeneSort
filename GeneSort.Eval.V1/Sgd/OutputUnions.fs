namespace GeneSort.Eval.V1.Sgd

open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Bins.Soss

[<RequireQualifiedAccess>]
type sorterPoolSet =
    | Standard of sorterPoolSet_Standard
    | Soss of sorterPoolSet_Soss
    member this.GenerationNumber =
        match this with
        | Standard value -> value.GenerationNumber
        | Soss value -> value.GenerationNumber
    member this.SorterPoolSetId =
        match this with
        | Standard value -> value.SorterPoolSetId
        | Soss value -> value.SorterPoolSetId

module SorterPoolSet =
    let asStandard = function
        | sorterPoolSet.Standard value -> Ok value
        | sorterPoolSet.Soss _ -> Error "Expected a Standard sorter pool set, but found Soss."
    let asSoss = function
        | sorterPoolSet.Soss value -> Ok value
        | sorterPoolSet.Standard _ -> Error "Expected a Soss sorter pool set, but found Standard."
    let toDataTableRecordsSnapshot prefix = function
        | sorterPoolSet.Standard value -> Standard.SorterPoolSetDescription.toDataTableRecordsSnapshot prefix value
        | sorterPoolSet.Soss value -> Soss.SorterPoolSetDescription.toDataTableRecordsSnapshot prefix value

[<RequireQualifiedAccess>]
type spSummarySet =
    | Standard of spSummarySet_Standard
    | Soss of spSummarySet_Soss
    member this.LastGeneration =
        match this with
        | Standard value -> value.LastGeneration
        | Soss value -> value.LastGeneration
    member this.SorterPoolSetSummarySetId =
        match this with
        | Standard value -> value.SorterPoolSetSummarySetId
        | Soss value -> value.SorterPoolSetSummarySetId

module SpSummarySet =
    let toDataTableRecords prefix = function
        | spSummarySet.Standard value -> SpSummarySet_Standard.toDataTableRecords prefix value
        | spSummarySet.Soss value -> SpSummarySet_Soss.toDataTableRecords prefix value

[<RequireQualifiedAccess>]
type sorterPoolBinsSetSeries =
    | Standard of sorterPoolBinsSetSeries_Standard
    | Soss of sorterPoolBinsSetSeries_Soss
    member this.MaxGeneration =
        match this with
        | Standard value -> value.MaxGeneration
        | Soss value -> value.MaxGeneration
    member this.SorterPoolEvalBinsSetCollectionId =
        match this with
        | Standard value -> value.SorterPoolEvalBinsSetCollectionId
        | Soss value -> value.SorterPoolEvalBinsSetCollectionId

module SorterPoolBinsSetSeries =
    let makeDataTableRecords = function
        | sorterPoolBinsSetSeries.Standard value -> SorterPoolEvalBinsSetCollection_Standard.makeDataTableRecords value
        | sorterPoolBinsSetSeries.Soss value -> SorterPoolEvalBinsSetCollection_Soss.makeDataTableRecords value

[<RequireQualifiedAccess>]
type spsh =
    | Standard of spsh_Standard
    | Soss of spsh_Soss
    member this.SaveGeneration =
        match this with
        | Standard value -> value.SaveGeneration
        | Soss value -> value.SaveGeneration
    member this.SorterPoolSetId =
        match this with
        | Standard value -> value.SorterPoolSetId
        | Soss value -> value.SorterPoolSetId

module Spsh =
    let toDataTableRecords = function
        | spsh.Standard value -> Spsh_Standard.toDataTableRecords value
        | spsh.Soss value -> Spsh_Soss.toDataTableRecords value
