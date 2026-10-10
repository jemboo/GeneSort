namespace GeneSort.Dispatch.V1.SortableTests

open FSharp.UMX
open GeneSort.Core
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.Sorting
open GeneSort.FileDb.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open CommonSortableTests

module SortableTestsDbs =

    module Merge =

        let dbName = "Merge" |> UMX.tag<databaseName>
        let dbFolder = $"c:\\Projects\\{projectName}\\{%dbName}\\Data"
                       |> UMX.tag<pathToRootFolder>


        let makeMergeQueryParams repl mergeLibId sortableDataFormat outputDataType =
            QueryParamsBuilders.SortableTests.Merge.makeQueryParams projectName dbName repl mergeLibId sortableDataFormat outputDataType

        let makeMergeQueryParamsFromRunParams rp odt =
            QueryParamsBuilders.SortableTests.Merge.queryParamsFromRunParams projectName dbName rp odt

        let db = new GeneSortDbMp(dbFolder, QueryCatalogNames.sortableTestMerge)


        let getMergeSortableTests
                (repl: int<replNumber>) 
                (mrgLibId: mergeLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTests, string>> =
            let qp = makeMergeQueryParams 
                            repl 
                            mrgLibId
                            sortableDataFormat 
                            (outputDataType.SortableTests "")
            (db :> IGeneSortDb).loadAsync qp
            |> Async.map (Result.bind OutputData.asSortableTests)



    module Prefix =
    
        let dbNameTest = "PrefixTest" |> UMX.tag<databaseName>
        let dbName = "Prefix" |> UMX.tag<databaseName>
        let dbFolder = $"c:\\Projects\\{projectName}\\{%dbName}\\Data"
                       |> UMX.tag<pathToRootFolder>


        let makePrefixQueryParams repl prefixLibId sortableDataFormat outputDataType =
            QueryParamsBuilders.SortableTests.Prefix.makeQueryParams projectName dbName repl prefixLibId sortableDataFormat outputDataType

        let makePrefixQueryParamsFromRunParams rp odt =
            QueryParamsBuilders.SortableTests.Prefix.queryParamsFromRunParams projectName dbName rp odt

        let db = new GeneSortDbMp(dbFolder, QueryCatalogNames.sortableTestPrefix)



        let getPrefixSortableTests
                (repl: int<replNumber>) 
                (pfxId: prefixLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTests, string>> =
            let qp = makePrefixQueryParams 
                            repl 
                            pfxId
                            sortableDataFormat 
                            (outputDataType.SortableTests "")
            (db :> IGeneSortDb).loadAsync qp
            |> Async.map (Result.bind OutputData.asSortableTests)



    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (Merge.dbName, Merge.db :> IGeneSortDb)
          (Prefix.dbName, Prefix.db :> IGeneSortDb) ]
        |> Map.ofList   

    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)



