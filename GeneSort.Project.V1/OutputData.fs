
namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting.Sortable

type outputData =
    | Run of run
    | RunParameters of runParameters
    | SortableTest of sortableTest
    | SorterEvalBins of sorterEvalBins


//type outputData =
//    | MutationSegmentEvalBinsSet of mutationSegmentEvalBinsSet
//    | Project of project
//    | RunParameters of runParameters
//    | SorterSet of sorterSet
//    | SortableTest of sortableTest
//    | SortableTestSet of sortableTestSet
//    | SortingSet of sortingSet
//    | SortingSetGen of sortingGenSegment
//    | SortableTestModelSet of sortableTestModelSet
//    | SortableTestModelSetGen of sortableTestModelSetGen
//    | SorterSetEval of sorterSetEval
//    | SorterEvalBins of sorterEvalBins
//    | TextReport of dataTableReport



//module OutputData =

//    let asSortingSet = function
//        | SortingSet sms -> Ok sms
//        | _ -> Error "Database returned data, but it was not a SortingSet."

//    let asSorterSet = function
//        | SorterSet ss -> Ok ss
//        | _ -> Error "Database returned data, but it was not a SorterSet."

//    let asSortableTest = function
//        | SortableTest st -> Ok st
//        | _ -> Error "Database returned data, but it was not a SortableTest."

//    let asSorterSetEval = function
//        | SorterSetEval sse -> Ok sse
//        | _ -> Error "Database returned data, but it was not a SorterSetEval."
        
//    let asRunParameters = function
//        | RunParameters rp -> Ok rp
//        | _ -> Error "Database returned data, but it was not RunParameters."

//    let asSorterEvalBins = function
//        | SorterEvalBins sseb -> Ok sseb
//        | _ -> Error "Database returned data, but it was not SorterEvalBins."

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


