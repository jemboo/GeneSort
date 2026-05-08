namespace GeneSort.Dispatch.V1.SortableTest


type executorType = 
    | Merge
    | Unknown

module ExecutorType =
    let toString = function
        | Merge -> "Merge"
        | Unknown -> "Unknown"

