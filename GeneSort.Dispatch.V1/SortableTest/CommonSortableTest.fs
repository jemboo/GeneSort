namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1


type sortableTestExecutorType = 
    | GenMerge
    | GenPrefix

module SortableTestExecutorType =
    let toString = function
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"


module CommonSortableTest =

    let projectName = "SortableTest" |> UMX.tag<projectName>

