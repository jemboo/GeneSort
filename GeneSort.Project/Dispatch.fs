
namespace GeneSort.Project

open System
open System.IO

open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Runs.Params
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs


open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.SortingOps.Mp
open GeneSort.Model.Sorter.Uf6
open System.Threading
open OutputData





module Dispatch =

    let Run 
            (projectName:string) 
            (runParamSet: runParameters [])
            (maxDegreeOfParallelism: int) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string>) =

        //match projectName with
        //| "Ce" -> Ce.runAll runParamSet maxDegreeOfParallelism cts progress
        //| "Si" -> Si.runAll runParamSet maxDegreeOfParallelism cts progress
        //| "Uf4" -> Uf4.runAll runParamSet maxDegreeOfParallelism cts progress
        //| _ -> failwithf "Unknown project name: %s" projectName

        ()

