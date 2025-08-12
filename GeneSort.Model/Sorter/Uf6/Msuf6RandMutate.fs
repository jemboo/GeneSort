namespace GeneSort.Model.Sorter.Uf6
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Uf6


[<Struct; CustomEquality; NoComparison>]
type Msuf6RandMutate = 
    private 
        {
          id : Guid<sorterModelMakerID>
          msuf6 : Msuf6
          rngType: rngType
          uf6MutationRatesArray: Uf6MutationRatesArray } 
    static member create 
            (rngType: rngType)
            (msuf6 : Msuf6)
            (uf6MutationRatesArray: Uf6MutationRatesArray) 
            : Msuf6RandMutate =
        if rngType = Unchecked.defaultof<rngType> then
            failwith "rngType must be specified"
        else if uf6MutationRatesArray.Length <> %msuf6.StageCount then
            failwith $"mutationRates array length (%d{uf6MutationRatesArray.Length}) must equal stageCount ({%msuf6.StageCount})"

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
            (msuf6 : Msuf6)
            (rates: Uf6MutationRates) 
            : Msuf6RandMutate =
        let mutationRates = Uf6MutationRatesArray.create (Array.create (%msuf6.StageCount) rates)
        Msuf6RandMutate.create rngType msuf6 mutationRates

    member this.Id with get () = this.id
    member this.Msuf6 with get () = this.msuf6
    member this.RngType with get () = this.rngType
    member this.StageCount with get () = this.msuf6.StageCount
    member this.Uf6MutationRatesArray with get () = this.uf6MutationRatesArray

    override this.Equals(obj) = 
        match obj with
        | :? Msuf6RandMutate as other -> 
            this.rngType = other.rngType && 
            this.msuf6 = other.msuf6 &&
            this.uf6MutationRatesArray.Equals(other.uf6MutationRatesArray)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.msuf6, this.uf6MutationRatesArray)

    interface IEquatable<Msuf6RandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id

    interface ISorterModelMaker with
        member this.Id = this.id

        /// Mutates an Msce by applying ChromosomeRates.mutate to its ceCodes array.
        /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
        /// The ceCodes array is modified using the provided chromosomeRates, with insertions and mutations
        /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
        member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                        : ISorterModel =
            if %this.StageCount <> this.Uf6MutationRatesArray.Length then
                failwith $"Stage count of Msuf6 {%this.StageCount} must match Msuf6RandMutate length {this.Uf6MutationRatesArray.Length}"
            //else if msuf6RandMutate.MutationRates.RatesArray |> Array.exists (fun rates -> rates.twoOrbitPairOpsTransitionRates.Length <> exactLog2(%msuf6.SortingWidth / 4)) then
            //    failwith $"All mutationRates must have twoOrbitPairOpsTransitionRates length equal to log2(sortingWidth/4)"
            let id = ISorterModelMaker.makeSorterModelId this index
            let rng = rngFactory this.RngType %id
            let mutatedUnfolders = 
                Array.zip this.msuf6.TwoOrbitUnfolder6s this.Uf6MutationRatesArray.RatesArray
                |> Array.map (fun (unfolder, mutationRates) ->
                    UnfolderOps6.mutateTwoOrbitUf6 rng.NextFloat mutationRates unfolder)
            Msuf6.create id this.msuf6.SortingWidth mutatedUnfolders



module Msuf6RandMutate =

    /// Returns a string representation of the Msuf6RandMutate configuration.
    let toString (msuf6RandMutate: Msuf6RandMutate) : string =
        let ratesStr = 
            msuf6RandMutate.Uf6MutationRatesArray.RatesArray
            |> Array.mapi (fun i rates -> 
                sprintf "[%d: OrthoToPara=%f, OrthoToSelfRefl=%f, ParaToOrtho=%f, ParaToSelfRefl=%f, SelfReflToOrtho=%f, SelfReflToPara=%f]" 
                    i 
                    rates.seed6TransitionRates.Ortho1Rates.Ortho2Rate
                    rates.seed6TransitionRates.Ortho2Rates.SelfReflRate
                    rates.seed6TransitionRates.Para1Rates.Ortho1Rate
                    rates.seed6TransitionRates.Para2Rates.SelfReflRate
                    rates.seed6TransitionRates.SelfReflRates.Ortho1Rate
                    rates.seed6TransitionRates.SelfReflRates.Para1Rate)
            |> String.concat ", "
        sprintf "Msuf6RandMutate(RngType=%A, StageCount=%d, MutationRates=%s)" 
                msuf6RandMutate.RngType 
                (%msuf6RandMutate.StageCount)
                ratesStr