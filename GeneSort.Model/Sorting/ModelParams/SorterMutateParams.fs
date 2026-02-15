namespace GeneSort.Model.Sorting.ModelParams

open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6


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
          rngType: rngType
          uf6MutationRatesArray: uf6MutationRatesArray
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

module SorterMutateParams = ()