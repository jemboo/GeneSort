namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Project.V1


type executorType = 
    | Generator
    | Unknown

module ExecutorType =
    let toString = function
        | Generator -> "Generator"
        | Unknown -> "Unknown"


module CommonSortableTest =

    let projectName = "SortableTest" |> UMX.tag<projectName>

