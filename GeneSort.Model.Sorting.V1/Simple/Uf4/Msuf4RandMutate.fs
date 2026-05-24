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
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (uf4MutationRates: uf4MutationRates) 
            : msuf4RandMutate =

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
        }

    member this.Id with get () = this.id
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
                (index: int) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId parent.Id this.Id index

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

    member this.MakeSorterModelFromIndex 
                        (parent: msuf4) 
                        (index: int) : msuf4 =
        let id = this.MakeSorterModelId parent index
        this.MakeSorterModelFromId parent id



module Msuf4RandMutate =

    let toString (msuf4RandMutate: msuf4RandMutate) : string =
        let rates = msuf4RandMutate.Uf4MutationRates
        let ratesStr = 
            sprintf "Seed(O:%.2f, P:%.2f, S:%.2f)" 
                rates.SeedOpsTransitionRates.OrthoRates.ParaRate
                rates.SeedOpsTransitionRates.ParaRates.OrthoRate
                rates.SeedOpsTransitionRates.SelfReflRates.ParaRate

        sprintf "Msuf4RandMutate(RngType=%A, MutationRates=%s)" 
                msuf4RandMutate.RngFactory
                ratesStr