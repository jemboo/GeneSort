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
          rngFactory: rngFactory
          opActionRates: opActionRates // Changed from opActionRatesArray
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (opActionRates: opActionRates) :mssiRandMutate =
        
        let id =
            [
                box "mssiRandMutate"
                box rngFactory
                box (opActionRates.GetHashCode())
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            rngFactory = rngFactory
            opActionRates = opActionRates
        }
        
    member this.Id with get() = this.id
    member this.RngFactory with get() = this.rngFactory
    member this.OpActionRates with get() = this.opActionRates

    override this.Equals(obj) = 
        match obj with
        | :? mssiRandMutate as other -> this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.Id)

    interface IEquatable<mssiRandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId 
                        (parent: mssi) 
                        (index: int) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId parent.Id this.Id index

    member this.MakeSorterModelFromId 
                        (parent:mssi) 
                        (id: Guid<sorterModelId>) :mssi =
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
                        parent.Perm_Sis
                        
        mssi.create id parent.SortingWidth mutated

    member this.MakeSorterModelFromIndex 
                                (parent:mssi) 
                                (index: int) : mssi =
        let id = this.MakeSorterModelId parent index
        this.MakeSorterModelFromId parent id


module MssiRandMutate =
    let toString (mssiRandMutate: mssiRandMutate) : string = 
        sprintf "MssiRandMutate(RngType=%A, OpActionRates=%s)" 
                mssiRandMutate.RngFactory
                (mssiRandMutate.OpActionRates.toString())