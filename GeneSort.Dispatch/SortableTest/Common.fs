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


module yab =

    let projectName = "SortableTest" |> UMX.tag<projectName>
    let mergeDatabaseName = "Merge" |> UMX.tag<databaseName>

    let projectRngType = rngType.Lcg

    let databaseFolder = "c:\\Projects\\SortableTest\\Merge\\Data"
                         |> UMX.tag<pathToRootFolder>

    let makeQueryParams 
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeFillType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            projectName
            repl 
            outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeFillType 
                    |> Option.map MergeSuffixType.toString |> UmxExt.stringOptionToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat 
                    |> Option.map SortableDataFormat.toString |> UmxExt.stringOptionToString ); |]


    let makeQueryParamsFromRunParams 
                (runParams: runParameters) 
                (outputDataType: outputDataType) : queryParams =
            makeQueryParams 
                    (runParams.GetRepl()) 
                    (runParams.GetSortingWidth()) 
                    (runParams.GetMergeDimension())
                    (runParams.GetMergeSuffixType()) 
                    (runParams.GetSortableDataFormat()) 
                    outputDataType


    let sortableTestDb = new GeneSortDbMp(databaseFolder, makeQueryParamsFromRunParams)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (mergeDatabaseName, sortableTestDb :> IGeneSortDb) ]
        |> Map.ofList   

    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)

