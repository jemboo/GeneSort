namespace GeneSort.Model.Sorting.Sorter.Rs

open FSharp.UMX
open GeneSort.Core
open System
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.Sorter.Rs

      
/// Represents a configuration for mutating Msrs instances with specified mutation probabilities.
[<Struct; CustomEquality; NoComparison>]
type msrsRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorID>
          msrs : msrs
          rngType: rngType
          opsActionRatesArray: opsActionRatesArray
        } 
    static member create 
            (rngType:rngType)
            (opsActionRatesArray: opsActionRatesArray)
            (msrs:msrs)
             : msrsRandMutate =
        
        if %msrs.Perm_Rss.Length <> opsActionRatesArray.Length then 
                failwith "Perm_Rss length must match opsActionRatesArray.Length"

        let id =
            [
                rngType :> obj
                msrs :> obj
                opsActionRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorID>

        {
            id = id
            msrs = msrs
            rngType = rngType
            opsActionRatesArray = opsActionRatesArray
        }

    member this.Id with get () = this.id
    member this.CeLength with get () = this.msrs.CeLength
    member this.Msrs with get () = this.msrs
    member this.RngType with get () = this.rngType
    member this.StageLength with get () = this.msrs.StageLength 
    member this.OpsActionRates with get () = this.opsActionRatesArray
    member this.SortingWidth with get () = this.msrs.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msrsRandMutate as other -> 
            this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.msrs, this.opsActionRatesArray)

    interface IEquatable<msrsRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id


    /// Mutates an Msrs by applying OpsActionRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                    : msrs =
        let id = CommonMutator.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
        let orthoMutator = fun psi ->   Perm_RsOps.mutatePerm_Rs (rng.NextIndex) opsActionMode.Ortho psi 
        let paraMutator = fun psi ->    Perm_RsOps.mutatePerm_Rs (rng.NextIndex) opsActionMode.Para psi 
        let selfSymMutator = fun psi -> Perm_RsOps.mutatePerm_Rs  (rng.NextIndex) opsActionMode.SelfRefl psi 
        let mutated = OpsActionRatesArray.mutate 
                        this.OpsActionRates 
                        orthoMutator 
                        paraMutator 
                        selfSymMutator
                        (rng.NextFloat) 
                        this.Msrs.Perm_Rss

        msrs.create id this.Msrs.SortingWidth mutated

            
module MsrsRandMutate =

    let toString (msrsRandMutate: msrsRandMutate) : string =
        let actionRates = msrsRandMutate.OpsActionRates.toString()
        sprintf "MsrsRandMutate(RngType=%A, Msrs=%s, OpsActionRates=%s)" 
                msrsRandMutate.RngType 
                (%msrsRandMutate.Msrs.toString())
                actionRates