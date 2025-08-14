namespace GeneSort.Model.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open System
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Rs

      
/// Represents a configuration for mutating Msrs instances with specified mutation probabilities.
[<Struct; CustomEquality; NoComparison>]
type MsrsRandMutate = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          msrs : Msrs
          rngType: rngType
          opsActionRatesArray: OpsActionRatesArray
        } 
    static member create 
            (rngType: rngType)
            (msrs: Msrs)
            (opsActionRatesArray: OpsActionRatesArray)
             : MsrsRandMutate =
        
        if %msrs.Perm_Rss.Length <> opsActionRatesArray.Length then failwith "Perm_Rss length must match opsActionRatesArray.Length"

        let id =
            [
                rngType :> obj
                msrs :> obj
                opsActionRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        {
            id = id
            msrs = msrs
            rngType = rngType
            opsActionRatesArray = opsActionRatesArray
        }

    member this.Id with get () = this.id
    member this.Msrs with get () = this.msrs
    member this.RngType with get () = this.rngType
    member this.StageCount with get () = this.msrs.StageCount 
    member this.OpsActionRates with get () = this.opsActionRatesArray

    override this.Equals(obj) = 
        match obj with
        | :? MsrsRandMutate as other -> 
            this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.msrs, this.opsActionRatesArray)

    interface IEquatable<MsrsRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id


    /// Mutates an Msrs by applying OpsActionRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                    : Msrs =
        let id = [
                    this.Id  :> obj
                    index :> obj
                    ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>
        let rng = rngFactory this.RngType %id
        let orthoMutator = fun psi ->   Perm_RsOps.mutatePerm_Rs (rng.NextIndex) OpsActionMode.Ortho psi 
        let paraMutator = fun psi ->    Perm_RsOps.mutatePerm_Rs (rng.NextIndex) OpsActionMode.Para psi 
        let selfSymMutator = fun psi -> Perm_RsOps.mutatePerm_Rs  (rng.NextIndex) OpsActionMode.SelfRefl psi 
        let mutated = OpsActionRatesArray.mutate 
                        this.OpsActionRates 
                        orthoMutator 
                        paraMutator 
                        selfSymMutator
                        (rng.NextFloat) 
                        this.Msrs.Perm_Rss

        Msrs.create id this.Msrs.SortingWidth mutated



            
module MsrsRandMutate =

    let toString (msrsRandMutate: MsrsRandMutate) : string =
        let actionRates = msrsRandMutate.OpsActionRates.toString()
        sprintf "MsrsRandMutate(RngType=%A, Msrs=%s, OpsActionRates=%s)" 
                msrsRandMutate.RngType 
                (%msrsRandMutate.Msrs.toString())
                actionRates