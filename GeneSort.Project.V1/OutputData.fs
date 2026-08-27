
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
    | SorterPoolSet of sorterPoolSet
    | SorterPoolSetSummaries of sorterPoolSetSummary array
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

    let asSorterPoolSet = function
        | SorterPoolSet ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterPoolSet."

    let asSorterPoolSetSummaries = function
        | SorterPoolSetSummaries ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterPoolSetSummaries."

    let asSorterSet = function
        | SorterSet ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterSet."

    let asSorterSetEval = function
        | SorterSetEval sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterSetEval."
        
    let asSorterPoolEvalBinSetCollection = function
        | SorterPoolEvalBinsSetCollection sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterPoolEvalBinSetCollection."

    let asSorterPoolSetHistory = function
        | SorterPoolSetHistory sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterPoolSetHistory."

    let asTextReport = function
        | TextReport tr -> Ok tr
        | _ -> Error "Database returned data, but it was not a TextReport."

