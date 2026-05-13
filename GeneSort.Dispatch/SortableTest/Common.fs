namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.Sorting
open GeneSort.FileDb.V1


type executorType = 
    | Merge
    | Unknown

module ExecutorType =
    let toString = function
        | Merge -> "Merge"
        | Unknown -> "Unknown"


module Common =

    let projectName = "SortableTest" |> UMX.tag<projectName>
    let mergeDatabaseName = "Merge" |> UMX.tag<databaseName>

    let projectRngType = rngType.Lcg

    let mergeDatabaseFolder = 
                            "c:\\Projects\\SortableTest\\Merge\\Data"
                            |> UMX.tag<pathToRootFolder>

