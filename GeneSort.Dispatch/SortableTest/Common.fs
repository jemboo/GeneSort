namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1


type executorType = 
    | Merge
    | Unknown

module ExecutorType =
    let toString = function
        | Merge -> "Merge"
        | Unknown -> "Unknown"

