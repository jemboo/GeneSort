namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

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
open GeneSort.Eval.V1.Sgd

module Msrs24p3=
    
    let projectName = "SorterSgd.Msrs24p3" |> UMX.tag<projectName>

    module PoolSzComp =

            let dbName = "PoolSzComp" |> UMX.tag<databaseName>
            let dbFolder = 
                    @$"c:\Projects\{projectName}\{%dbName}\Data" |> UMX.tag<pathToRootFolder>