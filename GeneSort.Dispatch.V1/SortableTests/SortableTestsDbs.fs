namespace GeneSort.Dispatch.V1.SortableTests

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1

module SortableTestsDbs =

    module Merge =

        let private sourceRun = SortableTests.Merge.Channels32.HiMD_32 ()
        let projectName = sourceRun.ProjectName

        let dbName = sourceRun.DatabaseName
        let dbFolder = RunHost.makeDataFolder sourceRun


        let makeMergeQueryParams repl mergeLibId sortableDataFormat outputDataType =
            QueryParamsBuildersRun.SortableTests.Merge.makeLibraryQueryParams projectName dbName repl mergeLibId sortableDataFormat outputDataType

        let makeMergeQueryParamsFromRunParams rp odt =
            QueryParamsBuildersRun.SortableTests.Merge.libraryQueryFromParameters projectName dbName rp odt

        let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sortableTestMerge, projectName)


        let getMergeSortableTests
                (repl: int<replNumber>) 
                (mrgLibId: mergeLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTests, string>> =
            let qp = makeMergeQueryParams 
                            repl 
                            mrgLibId
                            sortableDataFormat 
                            (geneSortDataType.SortableTests "")
            (db :> IGeneSortDb).loadAsync geneSortDataMode.Results qp
            |> Async.map (Result.bind OutputData.asSortableTests)



    module Prefix =

        let private sourceRun = SortableTests.Prefix.Channels32.FourStage_32 ()
        let projectName = sourceRun.ProjectName
    
        let dbName = sourceRun.DatabaseName
        let dbFolder = RunHost.makeDataFolder sourceRun


        let makePrefixQueryParams repl prefixLibId sortableDataFormat outputDataType =
            QueryParamsBuildersRun.SortableTests.Prefix.makeLibraryQueryParams projectName dbName repl prefixLibId sortableDataFormat outputDataType

        let makePrefixQueryParamsFromRunParams rp odt =
            QueryParamsBuildersRun.SortableTests.Prefix.libraryQueryFromParameters projectName dbName rp odt

        let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sortableTestPrefix, projectName)



        let getPrefixSortableTests
                (repl: int<replNumber>) 
                (pfxId: prefixLibId)
                (sortableDataFormat: sortableDataFormat): Async<Result<sortableTests, string>> =
            let qp = makePrefixQueryParams 
                            repl 
                            pfxId
                            sortableDataFormat 
                            (geneSortDataType.SortableTests "")
            (db :> IGeneSortDb).loadAsync geneSortDataMode.Results qp
            |> Async.map (Result.bind OutputData.asSortableTests)



    let databaseConfigs : Map<string<projectName> * string<databaseName>, IGeneSortDb> =
        [ ((Merge.projectName, Merge.dbName), Merge.db :> IGeneSortDb)
          ((Prefix.projectName, Prefix.dbName), Prefix.db :> IGeneSortDb) ]
        |> Map.ofList   

    let getDatabaseByName (project: string<projectName>) (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind (project, name) with
        | Some db -> db
        | None -> failwithf "Database %s/%s not found" (UMX.untag project) (UMX.untag name)



