
namespace GeneSort.Runs

open FSharp.UMX

open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs.Params
open GeneSort.Runs
open GeneSort.Core



type outputData =
    | RunParameters of runParameters
    | SorterSet of sorterSet
    | SortableTestSet of sortableTestSet
    | SorterModelSet of sorterModelSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    | Project of project
    | TextReport of dataTableFile
