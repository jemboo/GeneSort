namespace GeneSort.Model.Sorting.Sorter.Uf4
open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Core
open GeneSort.Sorting
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type msuf4RandGen = 
    private 
        { 
          id: Guid<sorterModelMakerID>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          stageLength: int<stageLength> 
          genRates: uf4GenRatesArray } 
    with
    /// Creates an Msuf4RandGen with the specified parameters.
    /// Throws an exception if rngType is invalid, width is not a power of 2, stageLength is less than 1, 
    /// or genRates array length does not match stageLength or contains invalid entries.
    /// <param name="rngFactory">The type of random number generator to use.</param>
    /// <param name="sortingWidth">The sorting width for the Msuf4 instance (must be a power of 2).</param>
    /// <param name="stageLength">The number of TwoOrbitUnfolder4 instances in the Msuf4 instance.</param>
    /// <param name="genRates">Array of generation rates for each TwoOrbitUnfolder4.</param>
    static member create 
            (rngFactory: rngFactory) 
            (sortingWidth: int<sortingWidth>)
            (stageLength: int<stageLength>) 
            (genRates: uf4GenRatesArray) 
            : msuf4RandGen =
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        else if (%sortingWidth - 1) &&& %sortingWidth <> 0 then
            failwith $"SortingWidth must be a power of 2, got {%sortingWidth}"
        else if %stageLength < 1 then
            failwith $"StageLength must be at least 1, got {%stageLength}"
        else if genRates.Length <> %stageLength then
            failwith $"genRates array length (%d{genRates.Length}) must equal stageLength ({%stageLength})"
        else if genRates.RatesArray |> Array.exists (fun gr -> gr.Order <> %sortingWidth) then
            failwith $"All genRates must have order {%sortingWidth}"
        else if genRates.RatesArray |> Array.exists (fun gr -> gr.OpsGenRatesArray.Length <> exactLog2(gr.Order / 4)) then
            failwith "twoOrbitTypeGenRatesList length must equal log2(order/4)"
        else
            let id =
                [
                    rngFactory :> obj
                    sortingWidth :> obj
                    stageLength :> obj
                    genRates :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { id = id
              rngFactory = rngFactory
              sortingWidth = sortingWidth
              stageLength = stageLength
              genRates = genRates }
              
    member this.Id with get () = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageLength / 2) |> UMX.tag<ceLength>
    member this.RngFactory with get () = this.rngFactory
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageLength with get () = this.stageLength
    member this.GenRates with get () = this.genRates

    override this.Equals(obj) = 
        match obj with
        | :? msuf4RandGen as other -> 
            this.rngFactory = other.rngFactory && 
            this.sortingWidth = other.sortingWidth &&
            this.stageLength = other.stageLength &&
            this.genRates.Equals(other.genRates)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.sortingWidth, this.stageLength, this.genRates)

    interface IEquatable<msuf4RandGen> with
        member this.Equals(other) = 
            this.rngFactory = other.rngFactory && 
            this.sortingWidth = other.sortingWidth &&
            this.stageLength = other.stageLength &&
            this.genRates.Equals(other.genRates)

    member this.MakeSorterModelId (index: int) : Guid<sorterModelID> =
        CommonMaker.makeSorterModelId this.Id index

    member this.MakeSorterModel (index: int) : msuf4 =
        let id = this.MakeSorterModelId index
        let rando = this.RngFactory.Create %id
        let sc = %this.StageLength
        let genRts = this.GenRates
        let twoOrbitUnfolder4s =
                [| for dex in 0 .. (sc - 1) ->
                    RandomUnfolderOps4.makeRandomTwoOrbitUf4 rando.NextFloat (genRts.Item(dex)) |]
        msuf4.create id this.SortingWidth twoOrbitUnfolder4s


module Msuf4RandGen =

    /// Returns a string representation of the Msuf4RandGen configuration.
    let toString (msuf4Gen: msuf4RandGen) : string =
        let genRatesStr = 
            msuf4Gen.GenRates.RatesArray 
            |> Array.mapi (fun i gr -> 
                sprintf "[%d: Ortho=%f, Para=%f, SelfRefl=%f]" 
                    i gr.SeedOpsGenRates.OrthoRate gr.SeedOpsGenRates.ParaRate gr.SeedOpsGenRates.SelfReflRate)
            |> String.concat ", "
        sprintf "Msuf4RandGen(RngType=%A, Width=%d, StageLength=%d, GenRates=%s)" 
                msuf4Gen.RngFactory 
                (%msuf4Gen.SortingWidth) 
                (%msuf4Gen.StageLength)
                genRatesStr