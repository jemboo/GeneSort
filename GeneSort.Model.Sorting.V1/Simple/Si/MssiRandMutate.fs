namespace GeneSort.Model.Sorting.V1.Simple.Si

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.PermSi
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sorter

[<Struct; CustomEquality; NoComparison>]
type mssiRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorId>
          rngFactory: rngFactory
          opActionRates: opActionRates
          mutVariant: mutatorVariant
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (opActionRates: opActionRates) 
            (mutVariant: mutatorVariant) : mssiRandMutate =
        
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
            mutVariant = mutVariant
        }
        
    member this.Id with get() = this.id
    member this.MutatorVariant = this.mutVariant
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
                        (index: int<mutationIndex>) 
                        (modd: int<mutationMod>) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId parent.Id this.Id index modd

    member this.MakeSorterModelFromId 
                        (parent:mssi) 
                        (id: Guid<sorterModelId>) :mssi =
        let rng = this.RngFactory.Create %id
        
        // Define mutation behaviors for PermSi
        let orthoMutator = fun psi -> PermSi.mutate (rng.NextIndex) MutationMode.Ortho psi 
        let paraMutator = fun psi ->  PermSi.mutate (rng.NextIndex) MutationMode.Para psi 
        
        let evalo (psi:permSi) : int =
            let ces = Ce.fromPermSi psi
            Ce.countReflectiveOrReflected parent.SortingWidth ces
            |> UMX.untag


        // Mutate the array using the uniform rates module
        let mutated =
            match this.MutatorVariant with
            | mutatorVariant.V1 -> 
                OpActionRates.mutateV1 
                    this.OpActionRates 
                    orthoMutator 
                    paraMutator 
                    (rng.NextFloat)
                    parent.Perm_Sis

            | mutatorVariant.V2 ->
                OpActionRates.mutateV2 
                                this.OpActionRates 
                                orthoMutator 
                                paraMutator 
                                (rng.NextFloat)
                                evalo
                                parent.Perm_Sis

            | mutatorVariant.V3 ->
                failwith "V3 mutator variant not implemented for mssiRandMutate"
                        
        mssi.create id parent.SortingWidth mutated

    member this.MakeSorterModelFromIndexAndMod 
                                (parent:mssi) 
                                (index: int<mutationIndex>) 
                                (modd: int<mutationMod>) : mssi =
        let id = this.MakeSorterModelId parent index modd
        this.MakeSorterModelFromId parent id
