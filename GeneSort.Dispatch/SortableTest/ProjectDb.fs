namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.Sorting
open GeneSort.FileDb.V1
open GeneSort.Sorting.Sortable


module ProjectDb =

    let makeMergeQueryParams 
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeFillType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            Common.projectName
            repl 
            outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeFillType 
                    |> Option.map MergeSuffixType.toString |> UmxExt.stringOptionToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat 
                    |> Option.map SortableDataFormat.toString |> UmxExt.stringOptionToString ); |]


    let makeMergeQueryParamsFromRunParams 
                (runParams: runParameters) 
                (outputDataType: outputDataType) : queryParams =
            makeMergeQueryParams 
                    (runParams.GetRepl()) 
                    (runParams.GetSortingWidth()) 
                    (runParams.GetMergeDimension())
                    (runParams.GetMergeSuffixType()) 
                    (runParams.GetSortableDataFormat()) 
                    outputDataType


    let sortableMergeTestDb = new GeneSortDbMp(Common.mergeDatabaseFolder, makeMergeQueryParamsFromRunParams)

    let getMergeSorterTestSet
                        (repl: int<replNumber> option) 
                        (sortingWidth: int<sortingWidth> option)
                        (mergeDimension: int<mergeDimension> option) 
                        (mergeSuffixType: mergeSuffixType option)
                        (sortableDataFormat: sortableDataFormat option) 
                             : Async<Result<sortableTest, string>> =
        let qp = makeMergeQueryParams 
                        repl 
                        sortingWidth 
                        mergeDimension 
                        mergeSuffixType 
                        sortableDataFormat 
                        (outputDataType.SortableTest "")
        (sortableMergeTestDb :> IGeneSortDb).loadAsync qp
        |> Async.map (Result.bind OutputData.asSortableTest)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (Common.mergeDatabaseName, sortableMergeTestDb :> IGeneSortDb) ]
        |> Map.ofList   

    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)

