namespace GeneSort.Model.Sorter.Uf6

open System
open FSharp.UMX
open EvoMergeSort.Core
open EvoMergeSort.Sorter
open GeneSort.Model.Sorter
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type Msuf6RandGen = 
    private 
        { rngType: rngType
          sortingWidth: int<sortingWidth>
          stageCount: int<stageCount> 
          genRates: Uf4GenRates array } 
    with
    /// Creates an Msuf6RandGen with the specified parameters.
    /// Throws an exception if rngType is invalid, width is not a power of 2, stageCount is less than 1, 
    /// or genRates array length does not match stageCount or contains invalid entries.
    /// <param name="rngType">The type of random number generator to use.</param>
    /// <param name="sortingWidth">The sorting width for the Msuf6 instance (must be a power of 2).</param>
    /// <param name="stageCount">The number of TwoOrbitUnfolder4 instances in the Msuf6 instance.</param>
    /// <param name="genRates">Array of generation rates for each TwoOrbitUnfolder4.</param>
    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (stageCount: int<stageCount>) 
            (genRates: Uf4GenRates array) 
            : Msuf6RandGen =
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        else if (%sortingWidth - 1) &&& %sortingWidth <> 0 then
            failwith $"SortingWidth must be a power of 2, got {%sortingWidth}"
        else if %stageCount < 1 then
            failwith $"StageCount must be at least 1, got {%stageCount}"
        else if genRates.Length <> %stageCount then
            failwith $"genRates array length (%d{genRates.Length}) must equal stageCount ({%stageCount})"
        else if genRates |> Array.exists (fun gr -> gr.order <> %sortingWidth) then
            failwith $"All genRates must have order {%sortingWidth}"
        else if genRates |> Array.exists (fun gr -> gr.seedGenRatesUf4.Ortho + gr.seedGenRatesUf4.Para + gr.seedGenRatesUf4.SelfRefl > 1.0) then
            failwith "Sum of seedGenRates must not exceed 1.0"
        else if genRates |> Array.exists (fun gr -> gr.opsGenRatesList.Length <> exactLog2(gr.order / 4)) then
            failwith "twoOrbitTypeGenRatesList length must equal log2(order/4)"
        else
            { rngType = rngType
              sortingWidth = sortingWidth
              stageCount = stageCount
              genRates = genRates }

    member this.RngType with get () = this.rngType
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageCount with get () = this.stageCount
    member this.GenRates with get () = this.genRates
    member this.toString() =
        let genRatesStr = 
            this.genRates 
            |> Array.mapi (fun i gr -> 
                sprintf "[%d: Ortho=%f, Para=%f, SelfRefl=%f]" 
                    i gr.seedGenRatesUf4.Ortho gr.seedGenRatesUf4.Para gr.seedGenRatesUf4.SelfRefl)
            |> String.concat ", "
        sprintf "Msuf6RandGen(RngType=%A, Width=%d, StageCount=%d, GenRates=%s)" 
                this.RngType 
                (%this.SortingWidth) 
                (%this.StageCount)
                genRatesStr


    override this.Equals(obj) = 
        match obj with
        | :? Msuf6RandGen as other -> 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount &&
            this.genRates = other.genRates
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.rngType, this.sortingWidth, this.stageCount, this.genRates)

    interface IEquatable<Msuf6RandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount &&
            this.genRates = other.genRates

module Msuf6RandGen =

    /// Generates a unique ID for an Msuf6 instance based on the Msuf6RandGen configuration and an index.
    let makeId (msuf6RandGen: Msuf6RandGen) (index: int) : Guid<sorterModelID> =
        [ 
            msuf6RandGen.RngType :> obj
            %msuf6RandGen.SortingWidth :> obj
            %msuf6RandGen.StageCount :> obj
            msuf6RandGen.GenRates :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

    /// Returns a string representation of the Msuf6RandGen configuration.
    let toString (msuf6Gen: Msuf6RandGen) : string =
        let genRatesStr = 
            msuf6Gen.GenRates 
            |> Array.mapi (fun i gr -> 
                sprintf "[%d: Ortho=%f, Para=%f, SelfRefl=%f]" 
                    i gr.seedGenRatesUf4.Ortho gr.seedGenRatesUf4.Para gr.seedGenRatesUf4.SelfRefl)
            |> String.concat ", "
        sprintf "Msuf6RandGen(RngType=%A, Width=%d, StageCount=%d, GenRates=%s)" 
                msuf6Gen.RngType 
                (%msuf6Gen.SortingWidth) 
                (%msuf6Gen.StageCount)
                genRatesStr

    /// Generates an Msuf6 instance with uniform generation rates for all TwoOrbitUnfolder4 instances.
    /// <param name="msuf6Gen">The Msuf6RandGen configuration (genRates ignored).</param>
    /// <param name="randoGen">The random number generator factory.</param>
    /// <param name="index">The index for ID generation.</param>
    /// <returns>A randomly generated Msuf6 instance with uniform generation rates.</returns>
    let makeModelUniform
            (msuf6Gen: Msuf6RandGen)
            (randoGen: rngType -> Guid -> IRando)
            (index: int) : Msuf6 =
        let id = makeId msuf6Gen index
        let rando = randoGen msuf6Gen.RngType %id
        let twoOrbitUnfolder4s =
            [| for _ in 0 .. (%msuf6Gen.StageCount - 1) ->
                let genRates = Uf6GenRates.makeUniform (%msuf6Gen.SortingWidth)
                TwoOrbitUf6Ops.makeTwoOrbitUf6 rando.NextFloat genRates |]
        Msuf6.create id msuf6Gen.SortingWidth twoOrbitUnfolder4s

    ///// Generates an Msuf6 instance with uniform generation rates for all but the last TwoOrbitUnfolder4,
    ///// which is biased towards the specified TwoOrbitType.
    ///// <param name="msuf6Gen">The Msuf6RandGen configuration (genRates ignored).</param>
    ///// <param name="randoGen">The random number generator factory.</param>
    ///// <param name="index">The index for ID generation.</param>
    ///// <param name="twoOrbitType">The TwoOrbitType to bias towards in the last stage.</param>
    ///// <param name="baseAmt">The base probability for generation rates.</param>
    ///// <param name="biasAmt">The bias amount to apply.</param>
    ///// <returns>A randomly generated Msuf6 instance with biased generation rates for the last stage.</returns>
    //let makeModelBiased
    //        (msuf6Gen: Msuf6RandGen)
    //        (randoGen: rngType -> Guid -> IRando)
    //        (index: int)
    //        (twoOrbitType: TwoOrbitType)
    //        (baseAmt: float)
    //        (biasAmt: float) : Msuf6 =
    //    let id = makeId msuf6Gen index
    //    let rando = randoGen msuf6Gen.RngType %id
    //    let twoOrbitUnfolder4s =
    //        [| for i in 0 .. (%msuf6Gen.StageCount - 1) ->
    //            let genRates = Uf6GenRates. 
    //                                (%msuf6Gen.SortingWidth) baseAmt twoOrbitType biasAmt
    //            TwoOrbitUf6Ops.makeTwoOrbitUf6 rando.NextFloat genRates |]

    //    Msuf6.create id msuf6Gen.SortingWidth twoOrbitUnfolder4s