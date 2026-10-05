namespace GeneSort.Dispatch.V1.SorterMutate

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.SortingLib.Sorter

module SorterMutateDbs =
    
    let projectName = "SorterMutate" |> UMX.tag<projectName>

    module RandomStandard =

        module Uniform =

            let dbName = "Rsu" |> UMX.tag<databaseName>
            let dbFolder = 
                    @$"c:\Projects\{projectName}\{%dbName}\Data" |> UMX.tag<pathToRootFolder>

            do QueryParamsBuilders.registerAll ()
            let db = new GeneSortDbMp(dbFolder, "sorter-mutate.standard")



    module RandomMerge =
    
        module Uniform =
            
            let dbName = "Rmu" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            do QueryParamsBuilders.registerAll ()
            let db = new GeneSortDbMp(dbFolder, "sorter-mutate.merge")



    module RandomPrefix =
    
        module Uniform =
            
            let dbName = "Rpu" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            do QueryParamsBuilders.registerAll ()
            let db = new GeneSortDbMp(dbFolder, "sorter-mutate.prefix")


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (RandomStandard.Uniform.dbName, RandomStandard.Uniform.db :> IGeneSortDb);
            (RandomMerge.Uniform.dbName, RandomMerge.Uniform.db :> IGeneSortDb) 
            (RandomPrefix.Uniform.dbName, RandomPrefix.Uniform.db :> IGeneSortDb) 
        ]
        |> Map.ofList


    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)


    let createRunHost (spec: runHostSpec) : runHost =
        let db = getDatabaseByName spec.databaseName
        let run = run.SimpleRun (SimpleRun.create spec.databaseName projectName spec.runName spec.runDescription spec.spans spec.queryCatalogName)
        runHost.Create db spec run

