
namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Sorting.Sortable
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd

type outputData =
    | Run of run
    | RunParameters of runParameters
    | SortableTest of sortableTest
    | SorterRunResult of sorterRunResult
    | SorterSet of sorterSet
    | SorterSetEval of sorterSetEval
    | TextReport of dataTableReport



module OutputData =

//    let asSortingSet = function
//        | SortingSet sms -> Ok sms
//        | _ -> Error "Database returned data, but it was not a SortingSet."
    let asSorterRunResult = function
        | SorterRunResult ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterRunResult."

    let asSorterSet = function
        | SorterSet ss -> Ok ss
        | _ -> Error "Database returned data, but it was not a SorterSet."

    let asSortableTest = function
        | SortableTest st -> Ok st
        | _ -> Error "Database returned data, but it was not a SortableTest."

    let asSorterSetEval = function
        | SorterSetEval sse -> Ok sse
        | _ -> Error "Database returned data, but it was not a SorterSetEval."
        
//    let asRunParameters = function
//        | RunParameters rp -> Ok rp
//        | _ -> Error "Database returned data, but it was not RunParameters."

    let asSorterEvalBins = function
        | SorterEvalBins sseb -> Ok sseb
        | _ -> Error "Database returned data, but it was not SorterEvalBins."

//    let asMutationSegmentEvalBinsSet = function
//        | MutationSegmentEvalBinsSet msebs -> Ok msebs
//        | _ -> Error "Database returned data, but it was not a MutationSegmentEvalBinsSet."

//    let asProject = function
//        | Project p -> Ok p
//        | _ -> Error "Database returned data, but it was not a Project."

//    let asSortableTestSet = function
//        | SortableTestSet sts -> Ok sts
//        | _ -> Error "Database returned data, but it was not a SortableTestSet."

//    let asSortingSetGen = function
//        | SortingSetGen ssg -> Ok ssg
//        | _ -> Error "Database returned data, but it was not a SortingSetGen."

//    let asSortableTestModelSet = function
//        | SortableTestModelSet stms -> Ok stms
//        | _ -> Error "Database returned data, but it was not a SortableTestModelSet."

//    let asSortableTestModelSetGen = function
//        | SortableTestModelSetGen stmsm -> Ok stmsm
//        | _ -> Error "Database returned data, but it was not a SortableTestModelSetGen."

//    let asTextReport = function
//        | TextReport tr -> Ok tr
//        | _ -> Error "Database returned data, but it was not a TextReport."


