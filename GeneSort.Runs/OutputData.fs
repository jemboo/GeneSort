
namespace GeneSort.Runs

open FSharp.UMX

open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs
open GeneSort.Core



type outputData =
    | RunParameters of runParameters
    | SorterSet of sorterSet
    | SortableTest of sortableTests
    | SortableTestSet of sortableTestSet
    | SorterModelSet of sorterModelSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    | Project of project
    | TextReport of dataTableFile



module OutputData =

    let asSorterSet = function
        | SorterSet ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterSet."

    let asSortableTest = function
        | SortableTest st -> Ok st
        | _ -> Error "Database returned data, but it was not a SortableTest."

    let asSorterSetEval = function
        | SorterSetEval sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterSetEval."
        
    let asRunParameters = function
        | RunParameters rp -> Ok rp
        | _ -> Error "Database returned data, but it was not RunParameters."

