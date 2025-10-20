
namespace GeneSort.Db

open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs.Params
open GeneSort.Runs


type outputDataType =
    | RunParameters
    | SorterSet
    | SortableTestSet
    | SorterModelSetMaker
    | SortableTestModelSet
    | SortableTestModelSetMaker
    | SorterSetEval
    | SorterSetEvalBins
    | Project


     
module OutputDataType =
    
    let toString (outputDataType: outputDataType) : string =
        match outputDataType with
        | RunParameters -> "RunParameters"
        | SorterSet -> "SorterSet"
        | SortableTestSet -> "SortableTestSet"
        | SorterModelSetMaker -> "SorterModelSet"
        | SortableTestModelSet -> "SortableTestModelSet"
        | SortableTestModelSetMaker -> "SortableTestModelSetMaker"
        | SorterSetEval -> "SorterSetEval"
        | SorterSetEvalBins -> "SorterSetEvalBins"
        | Project -> "Project"


type outputData =
    | RunParameters of runParameters
    | SorterSet of sorterSet
    | SortableTestSet of sortableTestSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    | Project of project


     
module OutputData =

    let getOutputDataType (outputData: outputData) : outputDataType =
        match outputData with
        | RunParameters _ -> outputDataType.RunParameters
        | SorterSet _ -> outputDataType.SorterSet
        | SortableTestSet _ -> outputDataType.SortableTestSet
        | SorterModelSetMaker _ -> outputDataType.SorterModelSetMaker
        | SortableTestModelSet _ -> outputDataType.SortableTestModelSet
        | SortableTestModelSetMaker _ -> outputDataType.SortableTestModelSetMaker
        | SorterSetEval _ -> outputDataType.SorterSetEval
        | SorterSetEvalBins _ -> outputDataType.SorterSetEvalBins
        | Project _ -> outputDataType.Project