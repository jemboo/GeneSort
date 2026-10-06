namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.Sorting
open GeneSort.FileDb.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open CommonSortableTest

module SortableTestDbs =

    module Merge =

        let dbName = "Merge" |> UMX.tag<databaseName>
        let dbFolder = $"c:\\Projects\\{projectName}\\{%dbName}\\Data"
                       |> UMX.tag<pathToRootFolder>


        let makeMergeQueryParams repl mergeLibId sortableDataFormat outputDataType =
            QueryParamsBuilders.SortableTest.Merge.makeQueryParams projectName dbName repl mergeLibId sortableDataFormat outputDataType

        let makeMergeQueryParamsFromRunParams rp odt =
            QueryParamsBuilders.SortableTest.Merge.queryParamsFromRunParams projectName dbName rp odt

        let db = new GeneSortDbMp(dbFolder, "sortable-test.merge")


        let getMergeSortableTest
                (repl: int<replNumber>) 
                (mrgLibId: mergeLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTests, string>> =
            let qp = makeMergeQueryParams 
                            repl 
                            mrgLibId
                            sortableDataFormat 
                            (outputDataType.SortableTest "")
            (db :> IGeneSortDb).loadAsync qp
            |> Async.map (Result.bind OutputData.asSortableTests)



    module Prefix =

        let dbName = "Prefix" |> UMX.tag<databaseName>
        let dbFolder = $"c:\\Projects\\{projectName}\\{%dbName}\\Data"
                       |> UMX.tag<pathToRootFolder>


        let makePrefixQueryParams repl prefixLibId sortableDataFormat outputDataType =
            QueryParamsBuilders.SortableTest.Prefix.makeQueryParams projectName dbName repl prefixLibId sortableDataFormat outputDataType

        let makePrefixQueryParamsFromRunParams rp odt =
            QueryParamsBuilders.SortableTest.Prefix.queryParamsFromRunParams projectName dbName rp odt

        let db = new GeneSortDbMp(dbFolder, "sortable-test.prefix")



        let getPrefixSortableTest
                (repl: int<replNumber>) 
                (pfxId: prefixLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTests, string>> =
            let qp = makePrefixQueryParams 
                            repl 
                            pfxId
                            sortableDataFormat 
                            (outputDataType.SortableTest "")
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



