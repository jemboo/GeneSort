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
              id : Guid<sorterModelMutatorId>
              mssi : mssi
              rngFactory: rngFactory
              opActionRates: opActionRatesArray
        } 
    static member create 
            (rngFactory: rngFactory)
            (opActionRatesArray: opActionRatesArray)
            (mssi: mssi)
            : mssiRandMutate =
        
        if %mssi.Perm_Sis.Length <> opActionRatesArray.Length then 
                failwith "Perm_Sis length must match opActionRatesArray.Length"

        let id =
            [
                rngFactory :> obj
                mssi :> obj
                opActionRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            mssi = mssi
            opActionRates = opActionRatesArray
            rngFactory = rngFactory
        }
        
    member this.Id with get () = this.id
    member this.CeLength with get () = this.mssi.CeLength
    member this.Mssi with get () = this.mssi
    member this.RngFactory with get () = this.rngFactory
    member this.StageLength with get () = this.opActionRates.Length
    member this.OpActionRates with get () = this.opActionRates
    member this.SortingWidth with get () = this.mssi.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? mssiRandMutate as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.mssi, this.opActionRates)

    interface IEquatable<mssiRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id

    member this.MakeSorterModelId (index: int) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId this.Id index

    /// Mutates an Mssi by applying OpActionRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (index: int) : mssi =
        let id = this.MakeSorterModelId index
        let rng = this.RngFactory.Create %id
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
                mssiRandMutate.RngFactory 
                (%mssiRandMutate.Mssi.SortingWidth) 
                (mssiRandMutate.StageLength)
                (mssiRandMutate.OpActionRates.ToString())