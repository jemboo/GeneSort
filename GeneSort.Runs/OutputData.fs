
namespace GeneSort.Runs

open FSharp.UMX

open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs
open GeneSort.Core



type outputData =
    | RunParameters of runParameters
    | SorterSet of sorterSet
    | SortableTest of sortableTest
    | SortableTestSet of sortableTestSet
    | SortingModelSet of sortingModelSet
    | SortingModelSetMaker of sortingModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    | Project of project
    | TextReport of dataTableReport



module OutputData =

    let asSortingModelSet = function
        | SortingModelSet sms -> Ok sms
        | _ -> Error "Database returned data, but it was not a SortingModelSet."

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

