namespace GeneSort.Model.Sorting.V1.Simple.Uf6

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.V1

[<Struct; CustomEquality; NoComparison>]
type msuf6RandMutate = 
    private 
        {
          id : Guid<sorterModelMutatorId>
          msuf6 : msuf6
          rngFactory: rngFactory
          uf6MutationRates: uf6MutationRates 
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (uf6MutationRates: uf6MutationRates) 
            (msuf6 : msuf6)
            : msuf6RandMutate =

        let id =
            [
                rngFactory :> obj
                msuf6.Id :> obj
                uf6MutationRates.GetHashCode() :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            rngFactory = rngFactory
            msuf6 = msuf6
            uf6MutationRates = uf6MutationRates
        }

    member this.CeLength with get () = this.msuf6.CeLength
    member this.Id with get () = this.id
    member this.Msuf6 with get () = this.msuf6
    member this.RngFactory with get () = this.rngFactory
    member this.StageLength with get () = this.msuf6.StageLength
    member this.Uf6MutationRates with get () = this.uf6MutationRates
    member this.SortingWidth with get () = this.msuf6.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msuf6RandMutate as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.RngFactory, this.msuf6, this.uf6MutationRates)

    interface IEquatable<msuf6RandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId (index: int) : Guid<simpleSorterModelId> =
        CommonMutator.makeSorterModelId this.Id index

    member this.MakeSimpleSorterModelFromId (id: Guid<simpleSorterModelId>) : msuf6 =
        let rng = this.RngFactory.Create %id
        
        let unfolderArray = this.msuf6.TwoOrbitUnfolder6s
        let rates = this.uf6MutationRates
        let width = this.msuf6.SortingWidth

        let mutatedUnfolders = 
            unfolderArray
            |> Array.map (fun unfolder ->
                RandomUnfolderOps6.mutateTwoOrbitUf6 rng.NextFloat rates unfolder)
                
        msuf6.create id width mutatedUnfolders

    member this.MakeSimpleSorterModelFromIndex (index: int) : msuf6 =
        let id = this.MakeSorterModelId index
        this.MakeSimpleSorterModelFromId id


module Msuf6RandMutate =

    let toString (msuf6RandMutate: msuf6RandMutate) : string =
        let rates = msuf6RandMutate.Uf6MutationRates
        // Simplified reporting since we are using uniform rates
        let seedStr = sprintf "Seed6(O1:%.2f, P1:%.2f)" 
                        rates.Seed6TransitionRates.Ortho1Rates.Ortho2Rate
                        rates.Seed6TransitionRates.Para1Rates.Ortho1Rate

        sprintf "Msuf6RandMutate(RngType=%A, StageLength=%d, MutationRates=%s)" 
                msuf6RandMutate.RngFactory 
                (%msuf6RandMutate.StageLength)
                seedStr