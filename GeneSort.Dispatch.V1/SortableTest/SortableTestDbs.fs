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

        do QueryParamsBuilders.registerAll ()
        let db = new GeneSortDbMp(dbFolder, "sortable-test.merge")


        let getMergeSorterTestSet
                (repl: int<replNumber>) 
                (mrgLibId: mergeLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTest, string>> =
            let qp = makeMergeQueryParams 
                            repl 
                            mrgLibId
                            sortableDataFormat 
                            (outputDataType.SortableTest "")
            (db :> IGeneSortDb).loadAsync qp
            |> Async.map (Result.bind OutputData.asSortableTest)



    module Prefix =

        let dbName = "Prefix" |> UMX.tag<databaseName>
        let dbFolder = $"c:\\Projects\\{projectName}\\{%dbName}\\Data"
                       |> UMX.tag<pathToRootFolder>


        let makePrefixQueryParams repl prefixLibId sortableDataFormat outputDataType =
            QueryParamsBuilders.SortableTest.Prefix.makeQueryParams projectName dbName repl prefixLibId sortableDataFormat outputDataType

        let makePrefixQueryParamsFromRunParams rp odt =
            QueryParamsBuilders.SortableTest.Prefix.queryParamsFromRunParams projectName dbName rp odt

        do QueryParamsBuilders.registerAll ()
        let db = new GeneSortDbMp(dbFolder, "sortable-test.prefix")



        let getPrefixSorterTestSet
                (repl: int<replNumber>) 
                (pfxId: prefixLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTest, string>> =
            let qp = makePrefixQueryParams 
                            repl 
                            pfxId
                            sortableDataFormat 
                            (outputDataType.SortableTest "")
            (db :> IGeneSortDb).loadAsync qp
            |> Async.map (Result.bind OutputData.asSortableTest)



    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (Merge.dbName, Merge.db :> IGeneSortDb)
          (Prefix.dbName, Prefix.db :> IGeneSortDb) ]
        |> Map.ofList   

    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)


    let createRunHost (spec: runHostSpec) : runHost =
        let db = getDatabaseByName spec.databaseName
        let run = run.SimpleRun (SimpleRun.create spec.databaseName projectName spec.runName spec.runDescription spec.spans spec.queryCatalogName)
        runHost.Create db spec run

