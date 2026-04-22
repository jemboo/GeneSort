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
          msuf4 : msuf4
          rngFactory: rngFactory
          uf4MutationRates: uf4MutationRates 
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (uf4MutationRates: uf4MutationRates) 
            (msuf4 : msuf4)
            : msuf4RandMutate =

        let id =
            [
                rngFactory :> obj
                msuf4.Id :> obj
                uf4MutationRates.GetHashCode() :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            rngFactory = rngFactory
            msuf4 = msuf4
            uf4MutationRates = uf4MutationRates
        }

    member this.Id with get () = this.id
    member this.Msuf4 with get () = this.msuf4
    member this.CeLength with get () = this.msuf4.CeLength
    member this.RngFactory with get () = this.rngFactory
    member this.StageLength with get () = this.msuf4.StageLength
    member this.Uf4MutationRates with get () = this.uf4MutationRates
    member this.SortingWidth with get () = this.msuf4.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msuf4RandMutate as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.RngFactory, this.msuf4, this.uf4MutationRates)

    interface IEquatable<msuf4RandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId (index: int) : Guid<simpleSorterModelId> =
        CommonMutator.makeSorterModelId this.Id index

    member this.MakeSimpleSorterModelFromId (id: Guid<simpleSorterModelId>) : msuf4 =
        let rng = this.RngFactory.Create %id
        
        // HOIST: Pull values out of 'this' into local variables to avoid capturing the struct byref
        let unfolderArray = this.msuf4.TwoOrbitUnfolder4s 
        let rates = this.Uf4MutationRates
        let width = this.msuf4.SortingWidth

        // Now the closure captures 'unfolderArray' and 'rates' (heap or stack values), not 'this' (the byref)
        let mutatedUnfolders = 
            unfolderArray
            |> Array.map (fun unfolder ->
                RandomUnfolderOps4.mutateTwoOrbitUf4 rng.NextFloat rates unfolder)
                
        msuf4.create id width mutatedUnfolders

    member this.MakeSimpleSorterModelFromIndex (index: int) : msuf4 =
        let id = this.MakeSorterModelId index
        this.MakeSimpleSorterModelFromId id



module Msuf4RandMutate =

    let toString (msuf4RandMutate: msuf4RandMutate) : string =
        let rates = msuf4RandMutate.Uf4MutationRates
        let ratesStr = 
            sprintf "Seed(O:%.2f, P:%.2f, S:%.2f)" 
                rates.seedOpsTransitionRates.OrthoRates.ParaRate
                rates.seedOpsTransitionRates.ParaRates.OrthoRate
                rates.seedOpsTransitionRates.SelfReflRates.ParaRate

        sprintf "Msuf4RandMutate(RngType=%A, StageLength=%d, MutationRates=%s)" 
                msuf4RandMutate.RngFactory 
                (%msuf4RandMutate.StageLength)
                ratesStr