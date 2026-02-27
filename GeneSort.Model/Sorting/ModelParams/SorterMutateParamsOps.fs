namespace GeneSort.Model.Sorting.ModelParams

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Model.Sorting

module SorterMutateParamsOps = 
    
    let makeSortingModelMutatorFromSorterModel
                (sorterModel: sorterModel)
                (sorterModelMutateParams: sorterModelMutateParams) : sortingModelMutator = 
        match sorterModel, sorterModelMutateParams with
        | Msce msce, McseRandMutateParams prams -> 
            // Validate that ceLength matches indelRatesArray length
            if %msce.CeLength <> prams.IndelRatesArray.Length then
                failwith (sprintf "Msce ceLength (%d) must match IndelRatesArray length (%d)" 
                            %msce.CeLength prams.IndelRatesArray.Length)
            msceRandMutate.create prams.RngFactory prams.IndelRatesArray prams.ExcludeSelfCe msce 
            |> sorterModelMutator.SmmMsceRandMutate |> sortingModelMutator.Single

        
        | Mssi mssi, MssiRandMutateParams prams -> 
            // Validate that stageLength matches opActionRatesArray length
            if %mssi.StageLength <> prams.OpActionRatesArray.Length then
                failwith (sprintf "Mssi stageLength (%d) must match OpActionRatesArray length (%d)" 
                            %mssi.StageLength prams.OpActionRatesArray.Length)
            mssiRandMutate.create prams.RngFactory prams.OpActionRatesArray mssi
            |> sorterModelMutator.SmmMssiRandMutate |> sortingModelMutator.Single
    
        
        | Msrs msrs, MsrsRandMutateParams prams -> 
            // Validate that stageLength matches opsActionRatesArray length
            if %msrs.StageLength <> prams.OpsActionRatesArray.Length then
                failwith (sprintf "Msrs stageLength (%d) must match OpsActionRatesArray length (%d)" 
                            %msrs.StageLength prams.OpsActionRatesArray.Length)
            msrsRandMutate.create prams.RngFactory prams.OpsActionRatesArray msrs
            |> sorterModelMutator.SmmMsrsRandMutate |> sortingModelMutator.Single

        
        | Msuf4 msuf4, Msuf4RandMutateParams prams -> 
            // Validate that stageLength matches uf4MutationRatesArray length
            if %msuf4.StageLength <> prams.Uf4MutationRatesArray.Length then
                failwith (sprintf "Msuf4 stageLength (%d) must match Uf4MutationRatesArray length (%d)" 
                            %msuf4.StageLength prams.Uf4MutationRatesArray.Length)
            msuf4RandMutate.create prams.RngFactory prams.Uf4MutationRatesArray msuf4
            |> sorterModelMutator.SmmMsuf4RandMutate |> sortingModelMutator.Single
    
        
        | Msuf6 msuf6, Msuf6RandMutateParams prams -> 
            // Validate that stageLength matches uf6MutationRatesArray length
            if %msuf6.StageLength <> prams.Uf6MutationRatesArray.Length then
                failwith (sprintf "Msuf6 stageLength (%d) must match Uf6MutationRatesArray length (%d)" 
                            %msuf6.StageLength prams.Uf6MutationRatesArray.Length)
            msuf6RandMutate.create prams.RngFactory prams.Uf6MutationRatesArray msuf6
            |> sorterModelMutator.SmmMsuf6RandMutate |> sortingModelMutator.Single

        
        | _ -> failwith "Mismatched sorterModel and sorterMutateParams types"



    let makeSortingModelSetMutatorFromSortingModel
                (sorting: sorting)
                (sorterModelMutateParams: sorterModelMutateParams) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingModelSetMutator = 

        if %count <= 0 then
            failwith "Count must be greater than 0"
        if %firstIndex < 0 then
            failwith "FirstIndex must be non-negative"

        match sorting with
        | sorting.Single sorterModel ->
            let sortingModelMutator = 
                makeSortingModelMutatorFromSorterModel sorterModel sorterModelMutateParams
            sortingModelSetMutator.create sortingModelMutator firstIndex count

        | sorting.Pair _ ->
                failwith "Mutation of Pair sortingModels not yet supported"


