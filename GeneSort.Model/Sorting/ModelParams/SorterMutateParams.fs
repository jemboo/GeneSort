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


type mcseRandMutateParams = 
    private 
        { 
          rngType: rngType
          indelRatesArray: indelRatesArray
          excludeSelfCe: bool }
    static member create
            (rngType: rngType)
            (indelRatesArray: indelRatesArray)
            (excludeSelfCe: bool): mcseRandMutateParams = 
        {
            rngType = rngType
            indelRatesArray = indelRatesArray
            excludeSelfCe = excludeSelfCe
        }
        
    member this.RngType with get () = this.rngType
    member this.IndelRatesArray with get () = this.indelRatesArray
    member this.ExcludeSelfCe with get () = this.excludeSelfCe


type mssiRandMutateParams = 
    private 
        { 
          rngType: rngType
          opActionRatesArray: opActionRatesArray
         }
    static member create
            (rngType: rngType)
            (opActionRatesArray: opActionRatesArray): mssiRandMutateParams = 
        {
            rngType = rngType
            opActionRatesArray = opActionRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.OpActionRatesArray with get () = this.opActionRatesArray


type msrsRandMutateParams = 
    private 
        { 
          rngType: rngType
          opsActionRatesArray: opsActionRatesArray
         }
    static member create
            (rngType: rngType)
            (opsActionRatesArray: opsActionRatesArray): msrsRandMutateParams = 
        {
            rngType = rngType
            opsActionRatesArray = opsActionRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.OpsActionRatesArray with get () = this.opsActionRatesArray


type msuf4RandMutateParams = 
    private 
        {
          rngType: rngType
          uf4MutationRatesArray: uf4MutationRatesArray
         }
    static member create
            (rngType: rngType)
            (uf4MutationRatesArray: uf4MutationRatesArray): msuf4RandMutateParams = 
        {
            rngType = rngType
            uf4MutationRatesArray = uf4MutationRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.Uf4MutationRatesArray with get () = this.uf4MutationRatesArray 


type msuf6RandMutateParams = 
    private 
        {
          uf6MutationRatesArray: uf6MutationRatesArray
          rngType: rngType
         }
    static member create
            (rngType: rngType)
            (uf6MutationRatesArray: uf6MutationRatesArray): msuf6RandMutateParams = 
        {
            rngType = rngType
            uf6MutationRatesArray = uf6MutationRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.Uf6MutationRatesArray with get () = this.uf6MutationRatesArray


type sorterMutateParams = 
    | McseRandMutateParams of mcseRandMutateParams
    | MssiRandMutateParams of mssiRandMutateParams
    | MsrsRandMutateParams of msrsRandMutateParams
    | Msuf4RandMutateParams of msuf4RandMutateParams
    | Msuf6RandMutateParams of msuf6RandMutateParams


module SorterMutateParams = 

    let makeUniformMsceMutator 
                (ceLength: int<ceLength>)
                (mutationRate: float<mutationRate>)
                (insertionRate: float<mutationRate>)
                (deletionRate: float<mutationRate>)
                (rngType: rngType) 
                (excludeSelfCe: bool option) : sorterMutateParams =

        let indelRates = indelRates.create (%mutationRate, %insertionRate, %deletionRate)
        let indelRatesArray = IndelRatesArray.createUniform %ceLength indelRates
        let mcseParams = mcseRandMutateParams.create rngType indelRatesArray (excludeSelfCe |> Option.defaultValue true)
        McseRandMutateParams mcseParams


    let makeUniformMssiMutator 
                (stageLength: int<stageLength>)
                (orthoRate: float<mutationRate>)
                (paraRate: float<mutationRate>)
                (rngType: rngType) : sorterMutateParams =
                let opActionRatesArray = OpActionRatesArray.createUniform %stageLength %orthoRate %paraRate
                let mssiParams = mssiRandMutateParams.create rngType opActionRatesArray
                MssiRandMutateParams mssiParams


    let makeUniformMsrsMutator 
                (stageLength: int<stageLength>)
                (orthoRate: float<mutationRate>)
                (paraRate: float<mutationRate>)
                (selfSymRate: float<mutationRate>)
                (rngType: rngType) : sorterMutateParams =
                let opsActionRatesArray = OpsActionRatesArray.createUniform %stageLength %orthoRate %paraRate %selfSymRate
                let msrsParams = msrsRandMutateParams.create rngType opsActionRatesArray
                MsrsRandMutateParams msrsParams

    let makeUniformMsuf4Mutator 
                (stageLength: int<stageLength>)
                (sortingWidth: int<sortingWidth>)
                (perm_RsMutationRate: float<mutationRate>)
                (twoOrbitMutationRate: float<mutationRate>)
                (rngType: rngType) : sorterMutateParams =
                let uf4MutationRates = Uf4MutationRates.makeUniform %sortingWidth %perm_RsMutationRate %twoOrbitMutationRate
                let uf4MutationRatesArray = Uf4MutationRatesArray.createUniform %stageLength %sortingWidth uf4MutationRates
                let msuf4Params = msuf4RandMutateParams.create rngType uf4MutationRatesArray
                Msuf4RandMutateParams msuf4Params

    //let makeUniformMsuf6Mutator 
    //            (stageLength: int<stageLength>)
    //            (mutationRate: float<mutationRate>)
    //            (rngType: rngType) : sorterMutateParams =
    //            let uf6MutationRates = uf6MutationRates.create %mutationRate
    //            let uf6MutationRatesArray = Uf6MutationRatesArray.createUniform %stageLength uf6MutationRates
    //            let msuf6Params = msuf6RandMutateParams.create rngType uf6MutationRatesArray
    //            Msuf6RandMutateParams msuf6Params


    let makeSortingModelMutatorFromSorterModel
                (sorterModel: sorterModel)
                (sorterModelMutateParams: sorterMutateParams) : sortingModelMutator = 
        match sorterModel, sorterModelMutateParams with
        | Msce msce, McseRandMutateParams prams -> 
            // Validate that ceLength matches indelRatesArray length
            if %msce.CeLength <> prams.IndelRatesArray.Length then
                failwith (sprintf "Msce ceLength (%d) must match IndelRatesArray length (%d)" 
                            %msce.CeLength prams.IndelRatesArray.Length)
            msceRandMutate.create prams.RngType prams.IndelRatesArray prams.ExcludeSelfCe msce 
            |> sorterModelMutator.SmmMsceRandMutate |> sortingModelMutator.Single
        
        | Mssi mssi, MssiRandMutateParams prams -> 
            // Validate that stageLength matches opActionRatesArray length
            if %mssi.StageLength <> prams.OpActionRatesArray.Length then
                failwith (sprintf "Mssi stageLength (%d) must match OpActionRatesArray length (%d)" 
                            %mssi.StageLength prams.OpActionRatesArray.Length)
            mssiRandMutate.create prams.RngType prams.OpActionRatesArray mssi
            |> sorterModelMutator.SmmMssiRandMutate |> sortingModelMutator.Single
        
        | Msrs msrs, MsrsRandMutateParams prams -> 
            // Validate that stageLength matches opsActionRatesArray length
            if %msrs.StageLength <> prams.OpsActionRatesArray.Length then
                failwith (sprintf "Msrs stageLength (%d) must match OpsActionRatesArray length (%d)" 
                            %msrs.StageLength prams.OpsActionRatesArray.Length)
            msrsRandMutate.create prams.RngType prams.OpsActionRatesArray msrs
            |> sorterModelMutator.SmmMsrsRandMutate |> sortingModelMutator.Single
        
        | Msuf4 msuf4, Msuf4RandMutateParams prams -> 
            // Validate that stageLength matches uf4MutationRatesArray length
            if %msuf4.StageLength <> prams.Uf4MutationRatesArray.Length then
                failwith (sprintf "Msuf4 stageLength (%d) must match Uf4MutationRatesArray length (%d)" 
                            %msuf4.StageLength prams.Uf4MutationRatesArray.Length)
            // Validate that sortingWidth matches (if Uf4MutationRatesArray has a width property)
            // Assuming Uf4MutationRatesArray has a way to get sortingWidth, add check here if needed
            msuf4RandMutate.create prams.RngType prams.Uf4MutationRatesArray msuf4
            |> sorterModelMutator.SmmMsuf4RandMutate |> sortingModelMutator.Single
        
        | Msuf6 msuf6, Msuf6RandMutateParams prams -> 
            // Validate that stageLength matches uf6MutationRatesArray length
            if %msuf6.StageLength <> prams.Uf6MutationRatesArray.Length then
                failwith (sprintf "Msuf6 stageLength (%d) must match Uf6MutationRatesArray length (%d)" 
                            %msuf6.StageLength prams.Uf6MutationRatesArray.Length)
            msuf6RandMutate.create prams.RngType prams.Uf6MutationRatesArray msuf6
            |> sorterModelMutator.SmmMsuf6RandMutate |> sortingModelMutator.Single
        
        | _ -> failwith "Mismatched sorterModel and sorterMutateParams types"



    let makeSortingModelSetMutatorFromSorterModel
                (sorterModel: sorterModel)
                (sorterModelMutateParams: sorterMutateParams) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingModelSetMutator =
    
        let sortingModelMutator = 
            makeSortingModelMutatorFromSorterModel sorterModel sorterModelMutateParams
    
        sortingModelSetMutator.create sortingModelMutator firstIndex count


    // Creates an array of sortingModelSets of size sortingModelSet.Count, each of which contains (count) mutated sortingModels. 
    let mutateSortingModelSetWithSorterMutateParams
                (rngFactory: rngType -> Guid -> IRando)
                (sms: sortingModelSet)
                (sorterModelMutateParams: sorterMutateParams) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sortingModelSet[] =
    
        if %count <= 0 then
            failwith "Count must be greater than 0"
        if %firstIndex < 0 then
            failwith "FirstIndex must be non-negative"
    
        // For each sortingModel in the input set, create a sortingModelSet of mutations
        sms.SortingModels
        |> Array.map (fun sortingModel ->
            match sortingModel with
            | sortingModel.Single sorterModel ->
                // Create a mutator for this specific sorterModel
                let mutator = 
                    makeSortingModelSetMutatorFromSorterModel 
                        sorterModel 
                        sorterModelMutateParams
                        firstIndex 
                        count
            
                // Generate the mutated models using the rngFactory
                mutator.MakeSortingModelSet rngFactory
            
            | sortingModel.Pair _ ->
                failwith "Mutation of Pair sortingModels not yet supported"
        )



