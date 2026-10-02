namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Dispatch.V1.CommonParams


module SorterEvalDbs =
    
    let projectName = "SorterEval" |> UMX.tag<projectName>

    module Standard =

        let dbName = "Standard" |> UMX.tag<databaseName>
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>


        let makeQueryParams = QueryParamsBuilders.SorterEval.Standard.makeQueryParams projectName dbName
        let queryParamsFromRunParams = QueryParamsBuilders.SorterEval.Standard.queryParamsFromRunParams projectName dbName
        do QueryParamsBuilders.registerAll ()
        let db = new GeneSortDbMp(dbFolder, "sorter-eval.standard")



    module Merge =

        let dbName = "Merge" |> UMX.tag<databaseName>
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

        let makeQueryParams = QueryParamsBuilders.SorterEval.Merge.makeQueryParams projectName dbName
        let queryParamsFromRunParams = QueryParamsBuilders.SorterEval.Merge.queryParamsFromRunParams projectName dbName
        do QueryParamsBuilders.registerAll ()
        let db = new GeneSortDbMp(dbFolder, "sorter-eval.merge")



    module Prefix =

        let dbName = "Prefix" |> UMX.tag<databaseName>
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

        let makeQueryParams = QueryParamsBuilders.SorterEval.Prefix.makeQueryParams projectName dbName
        let queryParamsFromRunParams = QueryParamsBuilders.SorterEval.Prefix.queryParamsFromRunParams projectName dbName
        do QueryParamsBuilders.registerAll ()
        let db = new GeneSortDbMp(dbFolder, "sorter-eval.prefix")




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
                        _rngTypeLcg
                        sortingWidth 
                        simpleSorterModelType 
                        sorterEvalType
                        (outputDataType.SorterSetEval "")
        async {
             let! result = (Standard.db :> IGeneSortDb).loadAsync qp
             return  result |> Result.bind OutputData.asSorterSetEval
        }


    let getMergeSorterEvals
                    (mrgLibId: mergeLibId)
                    (repl: int<replNumber>)
                    (simpleSorterModelType: simpleSorterModelType)
                            : Async<Result<sorterSetEval, string>> =

        let qp = Merge.makeQueryParams 
                        repl 
                        _rngTypeLcg
                        mrgLibId
                        simpleSorterModelType
                        sortableDataFormat.Int8Vector512
                        sorterEvalType.V2
                        (outputDataType.SorterSetEval "")
        async {
             let! result = (Merge.db :> IGeneSortDb).loadAsync qp
             return  result |> Result.bind OutputData.asSorterSetEval
        }



    let getPrefixSorterEvals
                    (pfxLibId: prefixLibId)
                    (repl: int<replNumber>)
                    (simpleSorterModelType: simpleSorterModelType)
                            : Async<Result<sorterSetEval, string>> =

        let qp = Prefix.makeQueryParams 
                        repl
                        _rngTypeLcg
                        pfxLibId 
                        simpleSorterModelType
                        sortableDataFormat.BitVector512
                        sorterEvalType.V2
                        (outputDataType.SorterSetEval "")
        async {
             let! result = (Prefix.db :> IGeneSortDb).loadAsync qp
             return  result |> Result.bind OutputData.asSorterSetEval
        }


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.databaseName
        let run = run.createWithCatalogName spec.databaseName projectName spec.runName spec.runDescription spec.spans spec.queryCatalogName
        runHost.Create db spec run :> IRunHost
