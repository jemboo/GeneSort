namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.Sorting
open GeneSort.FileDb.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open CommonSortableTest

module SortableMergeTestDb =

    let dbName = "Merge" |> UMX.tag<databaseName>
    let dbFolder = $"c:\\Projects\\{projectName}\\{%dbName}\\Data"
                   |> UMX.tag<pathToRootFolder>


    let makeMergeQueryParams 
                (repl: int<replNumber>) 
                (sortingWidth: int<sortingWidth>)
                (mergeDimension: int<mergeDimension>) 
                (mergeFillType: mergeSuffixType)
                (sortableDataFormat: sortableDataFormat) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            dbName
            (Some repl)
            outputDataType
            [| (runParameters.sortingWidthKey, string %sortingWidth); 
               (runParameters.mergeDimensionKey, string %mergeDimension);
               (runParameters.mergeSuffixTypeKey, MergeSuffixType.toString mergeFillType);
               (runParameters.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat); |]


    let makeMergeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
            let! sdf = rp.GetSortableDataFormat()
            return makeMergeQueryParams repl sw md mst sdf odt
        }


    let sortableMergeTestDb = new GeneSortDbMp(dbFolder, makeMergeQueryParamsFromRunParams)



    let getMergeSorterTestSet
                    (repl: int<replNumber>) 
                    (sortingWidth: int<sortingWidth>)
                    (mergeDimension: int<mergeDimension>) 
                    (mergeSuffixType: mergeSuffixType)
                    (sortableDataFormat: sortableDataFormat) 
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
        [ (dbName, sortableMergeTestDb :> IGeneSortDb) ]
        |> Map.ofList   

    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.DatabaseName
        let run = run.create spec.DatabaseName spec.RunName spec.RunDescription
        runHost.Create db spec run :> IRunHost

