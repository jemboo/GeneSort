namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1


module SorterEvalDbs =
    

    module Standard =

        let private sourceRun = SorterEval.RunCfg.Standard.Test ()
        let projectName = sourceRun.ProjectName

        let dbName = sourceRun.DatabaseName
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>


        let makeQueryParams = QueryParamsBuildersRun.SorterEval.Standard.makeQueryParams projectName dbName
        let queryParamsFromRunParams = QueryParamsBuildersRun.SorterEval.Standard.queryParamsFromRunParams projectName dbName
        let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sorterEvalStandard)



    module Merge =

        let private sourceRun = SorterEval.RunCfg.Merge.Test ()
        let projectName = sourceRun.ProjectName

        let dbName = sourceRun.DatabaseName
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

        let makeQueryParams = QueryParamsBuildersRun.SorterEval.Merge.makeQueryParams projectName dbName
        let queryParamsFromRunParams = QueryParamsBuildersRun.SorterEval.Merge.queryParamsFromRunParams projectName dbName
        let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sorterEvalMerge)



    module Prefix =

        let private sourceRun = SorterEval.RunCfg.Prefix.Width32 ()
        let projectName = sourceRun.ProjectName

        let sorterCount =
            sourceRun.SetupParameterSpans
            |> List.find (fst >> (=) ParamKeys.sorterCountKey)
            |> snd
            |> List.exactlyOne
            |> int
            |> UMX.tag<sorterCount>

        let dbName = sourceRun.DatabaseName
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

        let makeQueryParams repl rng prefixLib model dataFormat sorterCount evalType outputType =
            QueryParamsBuildersRun.SorterEval.Prefix.makeQueryParams
                projectName dbName repl rng prefixLib model dataFormat sorterCount evalType outputType
        let queryParamsFromRunParams = QueryParamsBuildersRun.SorterEval.Prefix.queryParamsFromRunParams projectName dbName
        let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sorterEvalPrefix)




    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (Standard.dbName, Standard.db :> IGeneSortDb);
          (Merge.dbName, Merge.db :> IGeneSortDb) 
          (Prefix.dbName, Prefix.db :> IGeneSortDb) ]
        |> Map.ofList


    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)



    let getStandardSorterEvals
                    (sortingWidth: int<sortingWidth>) 
                    (simpleSorterModelType: simpleSorterModelType) 
                    (sorterEvalType: sorterEvalType)
                            : Async<Result<sorterSetEval, string>> =
        let qp = Standard.makeQueryParams 
                        (0 |> UMX.tag<replNumber>) 
                        rngType.Lcg
                        sortingWidth 
                        simpleSorterModelType 
                        sorterEvalType
                        (geneSortDataType.SorterSetEval "")
        async {
             let! result = (Standard.db :> IGeneSortDb).loadAsync geneSortDataMode.Results qp
             return  result |> Result.bind OutputData.asSorterSetEval
        }


    let getMergeSorterEvals
                    (mrgLibId: mergeLibId)
                    (repl: int<replNumber>)
                    (simpleSorterModelType: simpleSorterModelType)
                            : Async<Result<sorterSetEval, string>> =

        let qp = Merge.makeQueryParams 
                        repl 
                        rngType.Lcg
                        mrgLibId
                        simpleSorterModelType
                        sortableDataFormat.Int8Vector512
                        sorterEvalType.V2
                        (geneSortDataType.SorterSetEval "")
        async {
             let! result = (Merge.db :> IGeneSortDb).loadAsync geneSortDataMode.Results qp
             return  result |> Result.bind OutputData.asSorterSetEval
        }



    let getPrefixSorterEvals
                    (pfxLibId: prefixLibId)
                    (repl: int<replNumber>)
                    (simpleSorterModelType: simpleSorterModelType)
                            : Async<Result<sorterSetEval, string>> =

        let qp = Prefix.makeQueryParams 
                        repl
                        rngType.Lcg
                        pfxLibId 
                        simpleSorterModelType
                        sortableDataFormat.BitVector512
                        Prefix.sorterCount
                        sorterEvalType.V2
                        (geneSortDataType.SorterSetEval "")
        async {
             let! result = (Prefix.db :> IGeneSortDb).loadAsync geneSortDataMode.Results qp
             return  result |> Result.bind OutputData.asSorterSetEval
        }


