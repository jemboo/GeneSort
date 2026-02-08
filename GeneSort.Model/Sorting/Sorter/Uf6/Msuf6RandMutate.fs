namespace GeneSort.Model.Sorter.Uf6
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Uf6


[<Struct; CustomEquality; NoComparison>]
type msuf6RandMutate = 
    private 
        {
          id : Guid<sorterModelMakerID>
          msuf6 : msuf6
          rngType: rngType
          uf6MutationRatesArray: uf6MutationRatesArray } 
    static member create 
            (rngType: rngType)
            (msuf6 : msuf6)
            (uf6MutationRatesArray: uf6MutationRatesArray) 
            : msuf6RandMutate =
        if rngType = Unchecked.defaultof<rngType> then
            failwith "rngType must be specified"
        else if uf6MutationRatesArray.Length <> %msuf6.StageLength then
            failwith $"mutationRates array length (%d{uf6MutationRatesArray.Length}) must equal stageLength ({%msuf6.StageLength})"

        let id =
            [
                rngType :> obj
                msuf6 :> obj
                uf6MutationRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        {
            id = id
            rngType = rngType
            msuf6 = msuf6
            uf6MutationRatesArray = uf6MutationRatesArray
        }

    static member createFromSingleRate
            (rngType: rngType)
            (msuf6 : msuf6)
            (rates: uf6MutationRates) 
            : msuf6RandMutate =
        let mutationRates = uf6MutationRatesArray.create (Array.create (%msuf6.StageLength) rates)
        msuf6RandMutate.create rngType msuf6 mutationRates

    member this.Id with get () = this.id
    member this.CeLength with get () = this.msuf6.CeLength
    member this.Msuf6 with get () = this.msuf6
    member this.RngType with get () = this.rngType
    member this.StageLength with get () = this.msuf6.StageLength
    member this.Uf6MutationRatesArray with get () = this.uf6MutationRatesArray

    override this.Equals(obj) = 
        match obj with
        | :? msuf6RandMutate as other -> 
            this.rngType = other.rngType && 
            this.msuf6 = other.msuf6 &&
            this.uf6MutationRatesArray.Equals(other.uf6MutationRatesArray)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.msuf6, this.uf6MutationRatesArray)

    interface IEquatable<msuf6RandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id


    /// Mutates an Msuf6 by applying Uf6MutationRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                    : msuf6 =
        if %this.StageLength <> this.Uf6MutationRatesArray.Length then
            failwith $"Stage count of Msuf6 {%this.StageLength} must match Msuf6RandMutate length {this.Uf6MutationRatesArray.Length}"
        let id = Common.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
        let mutatedUnfolders = 
            Array.zip this.msuf6.TwoOrbitUnfolder6s this.Uf6MutationRatesArray.RatesArray
            |> Array.map (fun (unfolder, mutationRates) ->
                RandomUnfolderOps6.mutateTwoOrbitUf6 rng.NextFloat mutationRates unfolder)
        msuf6.create id this.msuf6.SortingWidth mutatedUnfolders



module Msuf6RandMutate =

    /// Returns a string representation of the Msuf6RandMutate configuration.
    let toString (msuf6RandMutate: msuf6RandMutate) : string =
        let ratesStr = 
            msuf6RandMutate.Uf6MutationRatesArray.RatesArray
            |> Array.mapi (fun i rates -> 
                sprintf "[%d: OrthoToPara=%f, OrthoToSelfRefl=%f, ParaToOrtho=%f, ParaToSelfRefl=%f, SelfReflToOrtho=%f, SelfReflToPara=%f]" 
                    i 
                    rates.Seed6TransitionRates.Ortho1Rates.Ortho2Rate
                    rates.Seed6TransitionRates.Ortho2Rates.SelfReflRate
                    rates.Seed6TransitionRates.Para1Rates.Ortho1Rate
                    rates.Seed6TransitionRates.Para2Rates.SelfReflRate
                    rates.Seed6TransitionRates.SelfReflRates.Ortho1Rate
                    rates.Seed6TransitionRates.SelfReflRates.Para1Rate)
            |> String.concat ", "
        sprintf "Msuf6RandMutate(RngType=%A, StageLength=%d, MutationRates=%s)" 
                msuf6RandMutate.RngType 
                (%msuf6RandMutate.StageLength)
                ratesStr