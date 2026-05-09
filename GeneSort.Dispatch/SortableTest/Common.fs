namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core


type executorType = 
    | Merge
    | Unknown

module ExecutorType =
    let toString = function
        | Merge -> "Merge"
        | Unknown -> "Unknown"


module yab =

    let projectRngType = rngType.Lcg