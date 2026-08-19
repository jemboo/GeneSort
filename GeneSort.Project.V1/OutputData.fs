
namespace GeneSort.Project.V1

open GeneSort.Sorting.Sortable
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1.Sgd


type outputData =
    | Run of run
    | RunParameters of runParameters
    | SortableTest of sortableTest
    | SorterRunResult of sorterRunResult
    | SorterSet of sorterSet
    | SorterSetEval of sorterSetEval
    | SorterPoolEvalBinsSetCollection of sorterPoolEvalBinsSetCollection
    | SorterPoolSetHistory of sorterPoolSetHistory
    | TextReport of dataTableReport



module OutputData =

    let asRun = function
        | Run msebs -> Ok msebs
        | _ -> Error "Database returned data, but it was not a Run."

    let asRunParameters = function
        | RunParameters rp -> Ok rp
        | _ -> Error "Database returned data, but it was not RunParameters."

    let asSortableTest = function
        | SortableTest st -> Ok st
        | _ -> Error "Database returned data, but it was not a SortableTest."

    let asSorterRunResult = function
        | SorterRunResult ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterRunResult."

    let asSorterSet = function
        | SorterSet ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterSet."

    let asSorterSetEval = function
        | SorterSetEval sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterSetEval."
        
    let asSorterPoolEvalBinSetCollection = function
        | SorterPoolEvalBinsSetCollection sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterPoolEvalBinSetCollection."

    let asSorterPoolSetHistoryCollection = function
        | SorterPoolSetHistory sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterPoolSetHistoryCollection."

    let asTextReport = function
        | TextReport tr -> Ok tr
        | _ -> Error "Database returned data, but it was not a TextReport."

