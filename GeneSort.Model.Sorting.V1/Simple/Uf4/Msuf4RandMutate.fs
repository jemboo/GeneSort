namespace GeneSort.Model.Sorting.V1.Simple.Uf4

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.V1

[<Struct; CustomEquality; NoComparison>]
type msuf4RandMutate = 
    private 
        {
          id : Guid<sorterModelMutatorId>
          rngFactory: rngFactory
          uf4MutationRates: uf4MutationRates 
          mutVariant: mutatorVariant
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (uf4MutationRates: uf4MutationRates) 
            (mutVariant: mutatorVariant) : msuf4RandMutate =

        let id =
            [
                box "msuf4RandMutate"
                box rngFactory
                box (uf4MutationRates.GetHashCode())
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            rngFactory = rngFactory
            uf4MutationRates = uf4MutationRates
            mutVariant = mutVariant
        }

    member this.Id with get () = this.id
    member this.MutatorVariant = this.mutVariant
    member this.RngFactory with get () = this.rngFactory
    member this.Uf4MutationRates with get () = this.uf4MutationRates

    override this.Equals(obj) = 
        match obj with
        | :? msuf4RandMutate as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.Id)

    interface IEquatable<msuf4RandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId 
                (parent: msuf4) 
                (index: int<mutationIndex>) 
                (modd: int<mutationMod>) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId parent.Id this.Id index modd

    member this.MakeSorterModelFromId 
                        (parent: msuf4) 
                        (id: Guid<sorterModelId>) : msuf4 =
        let rng = this.RngFactory.Create %id
        
        // Pull values out of 'this' into local variables to avoid capturing the struct byref
        let unfolderArray = parent.TwoOrbitUnfolder4s 
        let rates = this.Uf4MutationRates
        let width = parent.SortingWidth

        let mutatedUnfolders = 
            unfolderArray
            |> Array.map (fun unfolder ->
                RandomUnfolderOps4.mutateTwoOrbitUf4 rng.NextFloat rates unfolder)
                
        msuf4.create id width mutatedUnfolders

    member this.MakeSorterModelFromIndexAndMod 
                        (parent: msuf4) 
                        (index: int<mutationIndex>) 
                        (modd: int<mutationMod>) : msuf4 =
        let id = this.MakeSorterModelId parent index modd
        this.MakeSorterModelFromId parent id 