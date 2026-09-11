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


        let makeMergeQueryParams 
                    (repl: int<replNumber>) 
                    (mrgLibId: mergeLibId)
                    (sortableDataFormat: sortableDataFormat) 
                    (outputDataType: outputDataType) : queryParams =

            queryParams.create 
                dbName projectName
                (Some repl)
                None
                outputDataType
                [| 
                   (runParameters.mergeLibIdKey, MergeLibId.toString mrgLibId);
                   (runParameters.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat); 
                |]


        let makeMergeQueryParamsFromRunParams 
                        (rp: runParameters) 
                        (odt: outputDataType) : queryParams option =
            maybe {
                let! repl = rp.GetRepl()
                let! mrgLibId = rp.GetMergeLibId()
                let! sdf = rp.GetSortableDataFormat()
                return makeMergeQueryParams repl mrgLibId sdf odt
            }


        let db = new GeneSortDbMp(dbFolder, makeMergeQueryParamsFromRunParams)


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


        let makePrefixQueryParams 
                    (repl: int<replNumber>) 
                    (pfxId: prefixLibId)
                    (sortableDataFormat: sortableDataFormat) 
                    (outputDataType: outputDataType) : queryParams =

            queryParams.create
                dbName projectName
                (Some repl)
                None
                outputDataType
                [| 
                   (runParameters.prefixLibIdKey, PrefixLibId.toString pfxId);
                   (runParameters.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat); 
                |]


        let makePrefixQueryParamsFromRunParams 
                        (rp: runParameters) 
                        (odt: outputDataType) : queryParams option =
            maybe {
                let! repl = rp.GetRepl()
                let! pfxId = rp.GetPrefixLibId()
                let! sdf = rp.GetSortableDataFormat()
                return makePrefixQueryParams repl pfxId sdf odt
            }


        let db = new GeneSortDbMp(dbFolder, makePrefixQueryParamsFromRunParams)



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


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.databaseName
        let run = run.create spec.databaseName projectName spec.runName spec.runDescription
        runHost.Create db spec run :> IRunHost

