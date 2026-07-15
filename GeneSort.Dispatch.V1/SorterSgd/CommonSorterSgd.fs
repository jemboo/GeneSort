namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.SortingOps
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Sorting
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Dispatch.V1
open System.Threading
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Sgd


type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | Gen32pfx4
    | SummaryReport
    | SnapshotReport


module SorterSgdExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | Gen32pfx4 -> "Gen32pfx4"
        | SummaryReport -> "SummaryReport"
        | SnapshotReport -> "SnapshotReport"
