namespace GeneSort.Model.Sorting.ModelParams

open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Model.Sorting


type mcseRandMutateParams = 
    private 
        { 
          msce : msce
          rngType: rngType
          indelRatesArray: indelRatesArray
          excludeSelfCe: bool }
    static member create
            (msce : msce)
            (rngType: rngType)
            (indelRatesArray: indelRatesArray)
            (excludeSelfCe: bool): mcseRandMutateParams = 
        {
            msce = msce;
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
          mssi : mssi
          rngType: rngType
          opActionRatesArray: opActionRatesArray
         }
    static member create
            (mssi: mssi)
            (rngType: rngType)
            (opActionRatesArray: opActionRatesArray): mssiRandMutateParams = 
        {
            mssi = mssi
            rngType = rngType
            opActionRatesArray = opActionRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.OpActionRatesArray with get () = this.opActionRatesArray


type msrsRandMutateParams = 
    private 
        { 
          msrs : msrs
          rngType: rngType
          opsActionRatesArray: opsActionRatesArray
         }
    static member create
            (msrs: msrs)
            (rngType: rngType)
            (opsActionRatesArray: opsActionRatesArray): msrsRandMutateParams = 
        {
            msrs = msrs
            rngType = rngType
            opsActionRatesArray = opsActionRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.OpsActionRatesArray with get () = this.opsActionRatesArray


type msuf4RandMutateParams = 
    private 
        {
          msuf4 : msuf4
          rngType: rngType
          uf4MutationRatesArray: uf4MutationRatesArray
         }
    static member create
            (msuf4 : msuf4)
            (rngType: rngType)
            (uf4MutationRatesArray: uf4MutationRatesArray): msuf4RandMutateParams = 
        {
            msuf4 = msuf4
            rngType = rngType
            uf4MutationRatesArray = uf4MutationRatesArray
        }
        
    member this.RngType with get () = this.rngType
    member this.Uf4MutationRatesArray with get () = this.uf4MutationRatesArray 


type msuf6RandMutateParams = 
    private 
        {
          msuf6 : msuf6
          uf6MutationRatesArray: uf6MutationRatesArray
          rngType: rngType
         }
    static member create
            (msuf6 : msuf6)
            (rngType: rngType)
            (uf6MutationRatesArray: uf6MutationRatesArray): msuf6RandMutateParams = 
        {
            msuf6 = msuf6
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
                (msce: msce)
                (mutationRate: float<mutationRate>)
                (insertionRate: float<mutationRate>)
                (deletionRate: float<mutationRate>)
                (rngType: rngType) 
                (excludeSelfCe: bool option) : sorterMutateParams =

        let indelRates = indelRates.create (%mutationRate, %insertionRate, %deletionRate)
        let indelRatesArray = IndelRatesArray.createUniform %msce.CeLength indelRates
        let mcseParams = mcseRandMutateParams.create msce rngType indelRatesArray (excludeSelfCe |> Option.defaultValue true)
        McseRandMutateParams mcseParams


    let makeUniformMssiMutator 
                (mssi: mssi)
                (orthoRate: float<mutationRate>)
                (paraRate: float<mutationRate>)
                (rngType: rngType) : sorterMutateParams =
                let opActionRatesArray = OpActionRatesArray.createUniform %mssi.StageLength %orthoRate %paraRate
                let mssiParams = mssiRandMutateParams.create mssi rngType opActionRatesArray
                MssiRandMutateParams mssiParams


    let makeUniformMsrsMutator 
                (msrs: msrs)
                (orthoRate: float<mutationRate>)
                (paraRate: float<mutationRate>)
                (selfSymRate: float<mutationRate>)
                (rngType: rngType) : sorterMutateParams =
                let opsActionRatesArray = OpsActionRatesArray.createUniform %msrs.StageLength %orthoRate %paraRate %selfSymRate
                let msrsParams = msrsRandMutateParams.create msrs rngType opsActionRatesArray
                MsrsRandMutateParams msrsParams

    let makeUniformMsuf4Mutator 
                (msuf4: msuf4)
                (perm_RsMutationRate: float<mutationRate>)
                (twoOrbitMutationRate: float<mutationRate>)
                (rngType: rngType) : sorterMutateParams =
                let uf4MutationRates = Uf4MutationRates.makeUniform %msuf4.SortingWidth %perm_RsMutationRate %twoOrbitMutationRate
                let uf4MutationRatesArray = Uf4MutationRatesArray.createUniform %msuf4.StageLength %msuf4.SortingWidth uf4MutationRates
                let msuf4Params = msuf4RandMutateParams.create msuf4 rngType uf4MutationRatesArray
                Msuf4RandMutateParams msuf4Params

    //let makeUniformMsuf6Mutator 
    //            (msuf6: msuf6)
    //            (mutationRate: float<mutationRate>)
    //            (rngType: rngType) : sorterMutateParams =
    //            let uf6MutationRates = uf6MutationRates.create %mutationRate
    //            let uf6MutationRatesArray = Uf6MutationRatesArray.createUniform %msuf6.StageLength uf6MutationRates
    //            let msuf6Params = msuf6RandMutateParams.create msuf6 rngType uf6MutationRatesArray
    //            Msuf6RandMutateParams msuf6Params


    let makeSorterModelMutator 
                (sorterModelMutateParams: sorterMutateParams) : sorterModelMutator = 
        match sorterModelMutateParams with
        | McseRandMutateParams prams -> 
            msceRandMutate.create prams.RngType prams.IndelRatesArray prams.ExcludeSelfCe prams.msce 
            |> sorterModelMutator.SmmMsceRandMutate
        | MssiRandMutateParams prams -> 
            mssiRandMutate.create prams.RngType prams.OpActionRatesArray prams.mssi
            |> sorterModelMutator.SmmMssiRandMutate
        | MsrsRandMutateParams prams -> 
            msrsRandMutate.create prams.RngType prams.OpsActionRatesArray prams.msrs
            |> sorterModelMutator.SmmMsrsRandMutate
        | Msuf4RandMutateParams prams -> 
            msuf4RandMutate.create prams.RngType prams.Uf4MutationRatesArray prams.msuf4
            |> sorterModelMutator.SmmMsuf4RandMutate
        | Msuf6RandMutateParams prams -> 
            msuf6RandMutate.create prams.RngType prams.Uf6MutationRatesArray prams.msuf6
            |> sorterModelMutator.SmmMsuf6RandMutate


