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
        { 
          id: Guid<sorterModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          stageCount: int<stageLength> 
          genRates: uf4GenRatesArray } 
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
            (stageCount: int<stageLength>) 
            (genRates: uf4GenRatesArray) 
            : Msuf4RandGen =
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        else if (%sortingWidth - 1) &&& %sortingWidth <> 0 then
            failwith $"SortingWidth must be a power of 2, got {%sortingWidth}"
        else if %stageCount < 1 then
            failwith $"StageCount must be at least 1, got {%stageCount}"
        else if genRates.Length <> %stageCount then
            failwith $"genRates array length (%d{genRates.Length}) must equal stageCount ({%stageCount})"
        else if genRates.RatesArray |> Array.exists (fun gr -> gr.Order <> %sortingWidth) then
            failwith $"All genRates must have order {%sortingWidth}"
        else if genRates.RatesArray |> Array.exists (fun gr -> gr.OpsGenRatesArray.Length <> exactLog2(gr.Order / 4)) then
            failwith "twoOrbitTypeGenRatesList length must equal log2(order/4)"
        else
            let id =
                [
                    rngType :> obj
                    sortingWidth :> obj
                    stageCount :> obj
                    genRates :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { id = id
              rngType = rngType
              sortingWidth = sortingWidth
              stageCount = stageCount
              genRates = genRates }
              
    member this.Id with get () = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageCount / 2) |> UMX.tag<ceLength>
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
            this.genRates.Equals(other.genRates)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.sortingWidth, this.stageCount, this.genRates)

    interface IEquatable<Msuf4RandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount &&
            this.genRates.Equals(other.genRates)


    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) : Msuf4 =
        let id = Common.makeSorterModelId this.Id index
        let rando = rngFactory this.RngType %id
        let sc = %this.StageCount
        let genRts = this.GenRates
        let twoOrbitUnfolder4s =
                [| for dex in 0 .. (sc - 1) ->
                    RandomUnfolderOps4.makeRandomTwoOrbitUf4 rando.NextFloat (genRts.Item(dex)) |]
        Msuf4.create id this.SortingWidth twoOrbitUnfolder4s


module Msuf4RandGen =

    /// Returns a string representation of the Msuf4RandGen configuration.
    let toString (msuf4Gen: Msuf4RandGen) : string =
        let genRatesStr = 
            msuf4Gen.GenRates.RatesArray 
            |> Array.mapi (fun i gr -> 
                sprintf "[%d: Ortho=%f, Para=%f, SelfRefl=%f]" 
                    i gr.SeedOpsGenRates.OrthoRate gr.SeedOpsGenRates.ParaRate gr.SeedOpsGenRates.SelfReflRate)
            |> String.concat ", "
        sprintf "Msuf4RandGen(RngType=%A, Width=%d, StageCount=%d, GenRates=%s)" 
                msuf4Gen.RngType 
                (%msuf4Gen.SortingWidth) 
                (%msuf4Gen.StageCount)
                genRatesStr