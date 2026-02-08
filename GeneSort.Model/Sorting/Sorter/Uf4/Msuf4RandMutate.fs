namespace GeneSort.Model.Sorting.Sorter.Uf4
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting


[<Struct; CustomEquality; NoComparison>]
type msuf4RandMutate = 
    private 
        {
          id : Guid<sorterModelMakerID>
          msuf4 : msuf4
          rngType: rngType
          uf4MutationRatesArray: Uf4MutationRatesArray } 
    static member create 
            (rngType: rngType)
            (msuf4 : msuf4)
            (uf4MutationRatesArray: Uf4MutationRatesArray) 
            : msuf4RandMutate =
        if rngType = Unchecked.defaultof<rngType> then
            failwith "rngType must be specified"
        else if uf4MutationRatesArray.Length <> %msuf4.StageLength then
            failwith $"mutationRates array length (%d{uf4MutationRatesArray.Length}) must equal stageLength ({%msuf4.StageLength})"

        let id =
            [
                rngType :> obj
                msuf4 :> obj
                uf4MutationRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        {
            id = id
            rngType = rngType
            msuf4 = msuf4
            uf4MutationRatesArray = uf4MutationRatesArray
        }

    static member createFromSingleRate
            (rngType: rngType)
            (msuf4 : msuf4)
            (rates: Uf4MutationRates) 
            : msuf4RandMutate =
        let mutationRates = Uf4MutationRatesArray.create (Array.create (%msuf4.StageLength) rates)
        msuf4RandMutate.create rngType msuf4 mutationRates

    member this.Id with get () = this.id
    member this.CeLength with get () = this.msuf4.CeLength
    member this.Msuf4 with get () = this.msuf4
    member this.RngType with get () = this.rngType
    member this.StageLength with get () = this.msuf4.StageLength
    member this.Uf4MutationRatesArray with get () = this.uf4MutationRatesArray

    override this.Equals(obj) = 
        match obj with
        | :? msuf4RandMutate as other -> 
            this.rngType = other.rngType && 
            this.msuf4 = other.msuf4 &&
            this.uf4MutationRatesArray.Equals(other.uf4MutationRatesArray)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.msuf4, this.uf4MutationRatesArray)

    interface IEquatable<msuf4RandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id


    /// Mutates an Msuf4 by applying Uf4MutationRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                    : msuf4 =
        if %this.StageLength <> this.Uf4MutationRatesArray.Length then
            failwith $"Stage count of Msuf4 {%this.StageLength} must match Msuf4RandMutate length {this.Uf4MutationRatesArray.Length}"
        let id = Common.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
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
                msuf4RandMutate.RngType 
                (%msuf4RandMutate.StageLength)
                ratesStr