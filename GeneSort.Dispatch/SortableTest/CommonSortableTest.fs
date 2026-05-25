namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Sorting


type executorType = 
    | Generator
    | Unknown

module ExecutorType =
    let toString = function
        | Generator -> "Generator"
        | Unknown -> "Unknown"


module CommonSortableTest =

    let projectName = "SortableTest" |> UMX.tag<projectName>
    let queryName = "SortableTest" |> UMX.tag<queryName>
    let mergeDatabaseName = "Merge" |> UMX.tag<databaseName>

    let projectRngType = rngType.Lcg
    let projectSortableDataFormat = sortableDataFormat.Int8Vector512

    let mergeDatabaseFolder = 
                            "c:\\Projects\\SortableTest\\Merge\\Data"
                            |> UMX.tag<pathToRootFolder>

