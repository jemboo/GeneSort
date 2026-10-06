namespace GeneSort.Dispatch.V1.SortableTests

open FSharp.UMX
open GeneSort.Project.V1


type sortableTestsExecutorType = 
    | GenMerge
    | GenPrefix

module SortableTestsExecutorType =
    let toString = function
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"


module CommonSortableTests =

    let projectName = "SortableTests" |> UMX.tag<projectName>

