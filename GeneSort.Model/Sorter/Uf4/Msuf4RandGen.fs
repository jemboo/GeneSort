namespace GeneSort.Model.Sorter.Uf4

open System
open FSharp.UMX
open GeneSort.Model.Sorter
open GeneSort.Core
open GeneSort.Sorter
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type Msuf4RandGen = 
    private 
        { rngType: rngType
          sortingWidth: int<sortingWidth>
          stageCount: int<stageCount> 
          genRates: Uf4GenRates array } 
    with
    /// Creates an Msuf4RandGen with the specified parameters.
    /// Throws an exception if rngType is invalid, width is not a power of 2, stageCount is less than 1, 
    /// or genRates array length does not match stageCount or contains invalid entries.
    /// <param name="rngType">The type of random number generator to use.</param>
    /// <param name="sortingWidth">The sorting width for the Msuf4 instance (must be a power of 2).</param>
    /// <param name="stageCount">The number of TwoOrbitUnfolder4 instances in the Msuf4 instance.</param>
    /// <param name="genRates">Array of generation rates for each TwoOrbitUnfolder4.</param>
    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (stageCount: int<stageCount>) 
            (genRates: Uf4GenRates array) 
            : Msuf4RandGen =
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

    override this.Equals(obj) = 
        match obj with
        | :? Msuf4RandGen as other -> 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount &&
            this.genRates = other.genRates
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.rngType, this.sortingWidth, this.stageCount, this.genRates)

    interface IEquatable<Msuf4RandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount &&
            this.genRates = other.genRates



module Msuf4RandGen =

    /// Generates a unique ID for an Msuf4 instance based on the Msuf4RandGen configuration and an index.
    let makeId (msuf4RandGen: Msuf4RandGen) (index: int) : Guid<sorterModelID> =
        [ 
            msuf4RandGen.RngType :> obj
            %msuf4RandGen.SortingWidth :> obj
            %msuf4RandGen.StageCount :> obj
            msuf4RandGen.GenRates :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

    /// Returns a string representation of the Msuf4RandGen configuration.
    let toString (msuf4Gen: Msuf4RandGen) : string =
        let genRatesStr = 
            msuf4Gen.GenRates 
            |> Array.mapi (fun i gr -> 
                sprintf "[%d: Ortho=%f, Para=%f, SelfRefl=%f]" 
                    i gr.seedOpsGenRates.OrthoRate gr.seedOpsGenRates.ParaRate gr.seedOpsGenRates.SelfReflRate)
            |> String.concat ", "
        sprintf "Msuf4RandGen(RngType=%A, Width=%d, StageCount=%d, GenRates=%s)" 
                msuf4Gen.RngType 
                (%msuf4Gen.SortingWidth) 
                (%msuf4Gen.StageCount)
                genRatesStr

    /// Generates an Msuf4 instance with uniform generation rates for all TwoOrbitUnfolder4 instances.
    /// <param name="msuf4Gen">The Msuf4RandGen configuration (genRates ignored).</param>
    /// <param name="randoGen">The random number generator factory.</param>
    /// <param name="index">The index for ID generation.</param>
    /// <returns>A randomly generated Msuf4 instance with uniform generation rates.</returns>
    let makeModel
            (msuf4Gen: Msuf4RandGen)
            (randoGen: rngType -> Guid -> IRando)
            (index: int) : Msuf4 =
        let id = makeId msuf4Gen index
        let rando = randoGen msuf4Gen.RngType %id
        let twoOrbitUnfolder4s =
            [| for dex in 0 .. (%msuf4Gen.StageCount - 1) ->
                UnfolderOps4.makeTwoOrbitUf4 rando.NextFloat msuf4Gen.GenRates.[dex] |]
        Msuf4.create id msuf4Gen.SortingWidth twoOrbitUnfolder4s