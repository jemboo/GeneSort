namespace GeneSort.Model.Sorting.V1.Simple.Uf4

open System
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Sorting
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type msuf4RandGen = 
    private 
        { 
          id: Guid<sorterModelGenId>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          stageLength: int<stageLength> 
          genRates: uf4GenRates // Changed from uf4GenRatesArray
        } 
    with
    static member create 
            (rngFactory: rngFactory) 
            (sortingWidth: int<sortingWidth>)
            (stageLength: int<stageLength>) 
            (genRates: uf4GenRates) 
            : msuf4RandGen =
        
        // Validation logic
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        else if (%sortingWidth - 1) &&& %sortingWidth <> 0 then
            failwith $"SortingWidth must be a power of 2, got {%sortingWidth}"
        else if %stageLength < 1 then
            failwith $"StageLength must be at least 1, got {%stageLength}"
        else if genRates.Order <> %sortingWidth then
            failwith $"genRates order (%d{genRates.Order}) must match sortingWidth ({%sortingWidth})"
        else
            let id =
                [
                    "msuf4RandGen" :> obj
                    rngFactory :> obj
                    sortingWidth :> obj
                    stageLength :> obj
                    genRates.GetHashCode() :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelGenId>

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
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.sortingWidth, this.stageLength, this.genRates)

    interface IEquatable<msuf4RandGen> with
        member this.Equals(other) = this.id = other.id

    member this.MakeSorterModelId (index: int) : Guid<sorterModelId> =
        CommonGen.makeSorterModelId this.Id index

    member this.MakeSorterModelFromId (id: Guid<sorterModelId>) : msuf4 =
        let rando = this.RngFactory.Create %id
        let sc = %this.stageLength
        let rates = this.genRates
        let width = this.sortingWidth

        let twoOrbitUnfolder4s =
            Array.init sc (fun _ -> 
                RandomUnfolderOps4.makeRandomTwoOrbitUf4 rando.NextFloat rates)

        msuf4.create id width twoOrbitUnfolder4s


    member this.MakeSorterModelFromIndex (index: int) : msuf4 =
        let id = this.MakeSorterModelId index
        this.MakeSorterModelFromId id



module Msuf4RandGen =

    let toString (msuf4Gen: msuf4RandGen) : string =
        let gr = msuf4Gen.GenRates
        let ratesStr = 
            sprintf "[Uniform: Ortho=%.2f, Para=%.2f, SelfRefl=%.2f]" 
                gr.SeedOpsGenRates.OrthoRate 
                gr.SeedOpsGenRates.ParaRate 
                gr.SeedOpsGenRates.SelfReflRate
                
        sprintf "Msuf4RandGen(RngType=%A, Width=%d, StageLength=%d, GenRates=%s)" 
                msuf4Gen.RngFactory 
                (%msuf4Gen.SortingWidth) 
                (%msuf4Gen.StageLength)
                ratesStr
