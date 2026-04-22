namespace GeneSort.Model.Sorting.V1.Simple.Si

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.Perm_Si
open GeneSort.Model.Sorting.V1

[<Struct; CustomEquality; NoComparison>]
type mssiRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorId>
          mssi : mssi
          rngFactory: rngFactory
          opActionRates: opActionRates // Changed from opActionRatesArray
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (opActionRates: opActionRates)
            (mssi: mssi) : mssiRandMutate =
        
        let id =
            [
                rngFactory :> obj
                mssi.Id :> obj
                opActionRates.GetHashCode() :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            mssi = mssi
            rngFactory = rngFactory
            opActionRates = opActionRates
        }
        
    member this.Id with get () = this.id
    member this.Mssi with get () = this.mssi
    member this.RngFactory with get () = this.rngFactory
    member this.CeLength with get () = this.mssi.CeLength
    member this.OpActionRates with get () = this.opActionRates
    member this.SortingWidth with get () = this.mssi.SortingWidth
    member this.StageLength with get () = this.mssi.Perm_Sis.Length

    override this.Equals(obj) = 
        match obj with
        | :? mssiRandMutate as other -> this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.mssi, this.opActionRates)

    interface IEquatable<mssiRandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId (index: int) : Guid<simpleSorterModelId> =
        CommonMutator.makeSorterModelId this.Id index

    member this.MakeSimpleSorterModelFromId (id: Guid<simpleSorterModelId>) : mssi =
        let rng = this.RngFactory.Create %id
        
        // Define mutation behaviors for Perm_Si
        let orthoMutator = fun psi -> Perm_Si.mutate (rng.NextIndex) MutationMode.Ortho psi 
        let paraMutator = fun psi ->  Perm_Si.mutate (rng.NextIndex) MutationMode.Para psi 
        
        // Mutate the array using the uniform rates module
        let mutated = OpActionRates.mutate 
                        this.OpActionRates 
                        orthoMutator 
                        paraMutator 
                        (rng.NextFloat) 
                        this.Mssi.Perm_Sis
                        
        mssi.create id this.Mssi.SortingWidth mutated

    member this.MakeSimpleSorterModelFromIndex (index: int) : mssi =
        let id = this.MakeSorterModelId index
        this.MakeSimpleSorterModelFromId id

module MssiRandMutate =
    let toString (mssiRandMutate: mssiRandMutate) : string = 
        sprintf "MssiRandMutate(RngType=%A, Width=%d, StageLength=%d, OpActionRates=%s)" 
                mssiRandMutate.RngFactory 
                (%mssiRandMutate.Mssi.SortingWidth) 
                (mssiRandMutate.StageLength)
                (mssiRandMutate.OpActionRates.toString())