namespace GeneSort.Dispatch.V1.SorterMutate


open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SortableTest



type sorterMutateExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | MutantReport
    | MutantMergeReport


module SorterMutateExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | MutantReport -> "MutantReport"
        | MutantMergeReport -> "MutantMergeReport"



module CommonSorterMutate = 

    let projectName = "SorterMutate" |> UMX.tag<projectName>
    let CollectSortableTests = true

    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    let standardSortableDataFormat = sortableDataFormat.BitVector512
    let mergeSortableDataFormat = CommonSortableTest.projectSortableDataFormat
    let projectRngType = rngType.Lcg