namespace GeneSort.Model.Sorter.Uf4
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open MathUtils
open GeneSort.Model.Sorter


[<Struct; CustomEquality; NoComparison>]
type Msuf4RandMutate = 
    private 
        { rngType: rngType
          stageCount: int<stageCount>
          mutationRates: Uf4MutationRatesArray } 
    static member create 
            (rngType: rngType)
            (stageCount: int<stageCount>)
            (mutationRates: Uf4MutationRatesArray) 
            : Msuf4RandMutate =
        if rngType = Unchecked.defaultof<rngType> then
            failwith "rngType must be specified"
        else if %stageCount < 1 then
            failwith $"StageCount must be at least 1, got {%stageCount}"
        else if mutationRates.Length <> %stageCount then
            failwith $"mutationRates array length (%d{mutationRates.Length}) must equal stageCount ({%stageCount})"
        {
            rngType = rngType
            stageCount = stageCount
            mutationRates = mutationRates
        }

    static member createFromSingleRate
            (rngType: rngType)
            (stageCount: int<stageCount>)
            (rates: Uf4MutationRates) 
            : Msuf4RandMutate =
        let mutationRates = Uf4MutationRatesArray.create (Array.create (%stageCount) rates)
        Msuf4RandMutate.create rngType stageCount mutationRates

    member this.RngType with get () = this.rngType
    member this.StageCount with get () = this.stageCount
    member this.MutationRates with get () = this.mutationRates

    override this.Equals(obj) = 
        match obj with
        | :? Msuf4RandMutate as other -> 
            this.rngType = other.rngType && 
            this.stageCount = other.stageCount &&
            this.mutationRates.Equals(other.mutationRates)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.stageCount, this.mutationRates)

    interface IEquatable<Msuf4RandMutate> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.stageCount = other.stageCount &&
            this.mutationRates.Equals(other.mutationRates)

module Msuf4RandMutate =

    /// Generates a unique ID for a mutated Msuf4 instance.
    let makeId (msuf4RandMutate: Msuf4RandMutate) (msuf4: Msuf4) (index: int) : Guid<sorterModelID> =
        [ 
            msuf4RandMutate.RngType :> obj
            %msuf4RandMutate.StageCount :> obj
            msuf4RandMutate.MutationRates.RatesArray :> obj
            %msuf4.Id :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

    /// Returns a string representation of the Msuf4RandMutate configuration.
    let toString (msuf4RandMutate: Msuf4RandMutate) : string =
        let ratesStr = 
            msuf4RandMutate.MutationRates.RatesArray 
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
        sprintf "Msuf4RandMutate(RngType=%A, StageCount=%d, MutationRates=%s)" 
                msuf4RandMutate.RngType 
                (%msuf4RandMutate.StageCount)
                ratesStr

    /// Mutates an Msuf4 instance using the specified Msuf4RandMutate configuration and random number generator.
    /// <param name="randoGen">The random number generator factory.</param>
    /// <param name="msuf4RandMutate">The Msuf4RandMutate configuration.</param>
    /// <param name="msuf4">The Msuf4 instance to mutate.</param>
    /// <param name="index">The index for ID generation.</param>
    /// <returns>A mutated Msuf4 instance.</returns>
    let mutate
            (randoGen: rngType -> Guid -> IRando)
            (msuf4RandMutate: Msuf4RandMutate)
            (msuf4: Msuf4) 
            (index: int) : Msuf4 =
        if msuf4.StageCount <> %msuf4RandMutate.StageCount then
            failwith $"Stage count of Msuf4 {%msuf4.StageCount} must match Msuf4RandMutate {%msuf4RandMutate.StageCount}"
        else if %msuf4.SortingWidth < 1 then
            failwith $"Msuf4 SortingWidth must be at least 1, got {%msuf4.SortingWidth}"
        else if msuf4RandMutate.MutationRates.RatesArray |> Array.exists (fun rates -> rates.order <> %msuf4.SortingWidth) then
            failwith $"All mutationRates must have order {%msuf4.SortingWidth}"
        else if msuf4RandMutate.MutationRates.RatesArray |> Array.exists (fun rates -> rates.twoOrbitPairOpsTransitionRates.Length <> exactLog2(%msuf4.SortingWidth / 4)) then
            failwith $"All mutationRates must have twoOrbitPairOpsTransitionRates length equal to log2(sortingWidth/4)"
        let id = makeId msuf4RandMutate msuf4 index
        let rando = randoGen msuf4RandMutate.RngType %id
        let mutatedUnfolders = 
            Array.zip msuf4.TwoOrbitUnfolder4s msuf4RandMutate.MutationRates.RatesArray
            |> Array.map (fun (unfolder, mutationRates) ->
                UnfolderOps4.mutateTwoOrbitUf4 rando.NextFloat mutationRates unfolder)
        Msuf4.create id msuf4.SortingWidth mutatedUnfolders