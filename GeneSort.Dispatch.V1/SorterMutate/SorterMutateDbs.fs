namespace GeneSort.Dispatch.V1.SorterMutate

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1

module SorterMutateDbs =
    

    module RandomStandard =

        let private sourceRun = SorterMutate.RunCfg.Standard.Msrs ()
        let projectName = sourceRun.ProjectName

        module Uniform =

            let dbName = sourceRun.DatabaseName
            let dbFolder = 
                    @$"c:\Projects\{projectName}\{%dbName}\Data" |> UMX.tag<pathToRootFolder>

            let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sorterMutateStandard)



    module RandomMerge =

        let private sourceRun = SorterMutate.RunCfg.Merge.Msce ()
        let projectName = sourceRun.ProjectName
    
        module Uniform =
            
            let dbName = sourceRun.DatabaseName
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sorterMutateMerge)



    module RandomPrefix =

        let private sourceRun = SorterMutate.RunCfg.Prefix.Msce ()
        let projectName = sourceRun.ProjectName
    
        module Uniform =
            
            let dbName = sourceRun.DatabaseName
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let db = new GeneSortDbMp(dbFolder, QueryCatalogNamesRun.sorterMutatePrefix)


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



