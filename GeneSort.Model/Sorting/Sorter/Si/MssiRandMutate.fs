namespace GeneSort.Model.Sorting.Sorter.Si

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.Perm_Si
open GeneSort.Model.Sorting


/// Represents a configuration for mutating Mssi instances with specified mutation probabilities.
[<Struct; CustomEquality; NoComparison>]
type mssiRandMutate = 
    private 
        { 
              id : Guid<sorterModelMutatorID>
              mssi : mssi
              rngType: rngType
              opActionRates: opActionRatesArray
        } 
    static member create 
            (rngType: rngType)
            (mssi: mssi)
            (opActionRatesArray: opActionRatesArray)
            : mssiRandMutate =
        
        if %mssi.Perm_Sis.Length <> opActionRatesArray.Length then 
                failwith "Perm_Sis length must match opActionRatesArray.Length"

        let id =
            [
                rngType :> obj
                mssi :> obj
                opActionRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorID>

        {
            id = id
            mssi = mssi
            opActionRates = opActionRatesArray
            rngType = rngType
        }
        
    member this.Id with get () = this.id
    member this.CeLength with get () = this.mssi.CeLength
    member this.Mssi with get () = this.mssi
    member this.RngType with get () = this.rngType
    member this.StageLength with get () = this.opActionRates.Length
    member this.OpActionRates with get () = this.opActionRates
    member this.SortingWidth with get () = this.mssi.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? mssiRandMutate as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.mssi, this.opActionRates)

    interface IEquatable<mssiRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id


    /// Mutates an Mssi by applying OpActionRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) : mssi =
        let id = CommonMutator.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
        let orthoMutator = fun psi ->  Perm_Si.mutate (rng.NextIndex) MutationMode.Ortho psi 
        let paraMutator = fun psi ->   Perm_Si.mutate (rng.NextIndex) MutationMode.Para psi 
        let mutated = OpActionRatesArray.mutate 
                        this.OpActionRates 
                        orthoMutator 
                        paraMutator 
                        (rng.NextFloat) 
                        this.Mssi.Perm_Sis
        mssi.create id this.Mssi.SortingWidth mutated



module MssiRandMutate =

    let toString (mssiRandMutate: mssiRandMutate) : string = 
        sprintf "MssiRandMutate(RngType=%A, Width=%d, StageLength=%d, OpActionRates=%s)" 
                mssiRandMutate.RngType 
                (%mssiRandMutate.Mssi.SortingWidth) 
                (mssiRandMutate.StageLength)
                (mssiRandMutate.OpActionRates.ToString())