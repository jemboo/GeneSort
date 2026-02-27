namespace GeneSort.Model.Sorting.Sorter.Uf4
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting

[<Struct; CustomEquality; NoComparison>]
type msuf4RandMutate = 
    private 
        {
          id : Guid<sorterModelMutatorId>
          msuf4 : msuf4
          rngFactory: rngFactory
          uf4MutationRatesArray: uf4MutationRatesArray } 
    static member create 
            (rngFactory: rngFactory)
            (uf4MutationRatesArray: uf4MutationRatesArray) 
            (msuf4 : msuf4)
            : msuf4RandMutate =
        if uf4MutationRatesArray.Length <> %msuf4.StageLength then
            failwith $"mutationRates array length (%d{uf4MutationRatesArray.Length}) must equal stageLength ({%msuf4.StageLength})"

        let id =
            [
                rngFactory :> obj
                msuf4 :> obj
                uf4MutationRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            rngFactory = rngFactory
            msuf4 = msuf4
            uf4MutationRatesArray = uf4MutationRatesArray
        }

    static member createFromSingleRate
            (rngFactory: rngFactory)
            (msuf4 : msuf4)
            (rates: uf4MutationRates) 
            : msuf4RandMutate =
        let mutationRates = uf4MutationRatesArray.create (Array.create (%msuf4.StageLength) rates)
        msuf4RandMutate.create rngFactory mutationRates msuf4

    member this.Id with get () = this.id
    member this.CeLength with get () = this.msuf4.CeLength
    member this.Msuf4 with get () = this.msuf4
    member this.RngFactory with get () = this.rngFactory
    member this.StageLength with get () = this.msuf4.StageLength
    member this.Uf4MutationRatesArray with get () = this.uf4MutationRatesArray
    member this.SortingWidth with get () = this.msuf4.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msuf4RandMutate as other -> 
            this.RngFactory = other.RngFactory && 
            this.msuf4 = other.msuf4 &&
            this.uf4MutationRatesArray.Equals(other.uf4MutationRatesArray)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.RngFactory, this.msuf4, this.uf4MutationRatesArray)

    interface IEquatable<msuf4RandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id

    member this.MakeSorterModelId (index: int) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId this.Id index

    /// Mutates an Msuf4 by applying Uf4MutationRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (index: int) : msuf4 =
        if %this.StageLength <> this.Uf4MutationRatesArray.Length then
            failwith $"Stage count of Msuf4 {%this.StageLength} must match Msuf4RandMutate length {this.Uf4MutationRatesArray.Length}"
        let id = this.MakeSorterModelId index
        let rng = this.RngFactory.Create %id
        let mutatedUnfolders = 
            Array.zip this.msuf4.TwoOrbitUnfolder4s this.Uf4MutationRatesArray.RatesArray
            |> Array.map (fun (unfolder, mutationRates) ->
                RandomUnfolderOps4.mutateTwoOrbitUf4 rng.NextFloat mutationRates unfolder)
        msuf4.create id this.msuf4.SortingWidth mutatedUnfolders


module Msuf4RandMutate =

    /// Returns a string representation of the Msuf4RandMutate configuration.
    let toString (msuf4RandMutate: msuf4RandMutate) : string =
        let ratesStr = 
            msuf4RandMutate.Uf4MutationRatesArray.RatesArray
            |> Array.mapi (fun i rates -> 
                sprintf "[%d: OrthoToPara=%f, OrthoToSelfRefl=%f, ParaToOrtho=%f, ParaToSelfRefl=%f, SelfReflToOrtho=%f, SelfReflToPara=%f]" 
                    i 
                    rates.seedOpsTransitionRates.OrthoRates.ParaRate
                    rates.seedOpsTransitionRates.OrthoRates.SelfReflRate
                    rates.seedOpsTransitionRates.ParaRates.OrthoRate
                    rates.seedOpsTransitionRates.ParaRates.SelfReflRate
                    rates.seedOpsTransitionRates.SelfReflRates.OrthoRate
                    rates.seedOpsTransitionRates.SelfReflRates.ParaRate)
            |> String.concat ", "
        sprintf "Msuf4RandMutate(RngType=%A, StageLength=%d, MutationRates=%s)" 
                msuf4RandMutate.RngFactory 
                (%msuf4RandMutate.StageLength)
                ratesStr