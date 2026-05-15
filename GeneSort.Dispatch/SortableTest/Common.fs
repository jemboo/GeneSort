namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.FileDb.V1


type executorType = 
    | Merge_Gen
    | Unknown

module ExecutorType =
    let toString = function
        | Merge_Gen -> "Merge_Gen"
        | Unknown -> "Unknown"


module Common =

    let projectName = "SortableTest" |> UMX.tag<projectName>
    let mergeDatabaseName = "Merge" |> UMX.tag<databaseName>

    let projectRngType = rngType.Lcg

    let mergeDatabaseFolder = 
                            "c:\\Projects\\SortableTest\\Merge\\Data"
                            |> UMX.tag<pathToRootFolder>

