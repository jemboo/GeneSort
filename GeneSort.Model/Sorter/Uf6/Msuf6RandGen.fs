namespace GeneSort.Model.Sorter.Uf6
open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core
open GeneSort.Model.Sorter

[<Struct; CustomEquality; NoComparison>]
type Msuf6RandGen = 
    private 
        { id: Guid<sorterModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          stageCount: int<stageCount> 
          genRates: Uf6GenRatesArray }

    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>) 
            (stageCount: int<stageCount>) 
            (genRates: Uf6GenRatesArray) : Msuf6RandGen =
        if %sortingWidth < 6 || %sortingWidth % 2 <> 0 then
            failwith $"SortingWidth must be at least 6 and even, got {%sortingWidth}"
        if %stageCount < 1 then
            failwith $"StageCount must be at least 1, got {%stageCount}"
        if genRates.Length <> %stageCount then
            failwith $"Uf6GenRatesArray length (%d{genRates.Length}) must equal stageCount (%d{%stageCount})"
        if genRates.RatesArray |> Array.exists (fun r -> r.order <> %sortingWidth) then
            failwith $"All Uf6GenRates in genRates must have order {%sortingWidth}"
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

    member this.Id with get() = this.id
    member this.RngType with get() = this.rngType
    member this.SortingWidth with get() = this.sortingWidth
    member this.StageCount with get() = this.stageCount
    member this.GenRates with get() = this.genRates

    member this.toString() =
        sprintf "Msuf6RandGen(Id=%A, RngType=%A, SortingWidth=%d, StageCount=%d, GenRates=%s)"
                (%this.id)
                this.rngType
                (%this.sortingWidth)
                (%this.stageCount)
                (this.genRates.toString())

    override this.Equals(obj) =
        match obj with
        | :? Msuf6RandGen as other ->
            this.id = other.id
        | _ -> false

    override this.GetHashCode() =
        hash (this.id)

    interface IEquatable<Msuf6RandGen> with
        member this.Equals(other) =
            this.id = other.id

    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) : Msuf6 =
        let id = Common.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
        let genRatesArray = this.GenRates
        let stageCount = %this.StageCount
        let twoOrbitUnfolder6s =
            [| for dex in 0 .. (stageCount - 1) ->
                RandomUnfolderOps6.makeRandomTwoOrbitUf6
                    rng.NextFloat
                    (genRatesArray.Item(dex)) |]
        Msuf6.create id this.SortingWidth twoOrbitUnfolder6s


module Msuf6RandGen =

    let toString (msuf6RandGen: Msuf6RandGen) : string =
        sprintf "Msuf6RandGen(Id=%A, RngType=%A, SortingWidth=%d, StageCount=%d, GenRates=%s)"
                (%msuf6RandGen.Id)
                msuf6RandGen.RngType
                (%msuf6RandGen.SortingWidth)
                (%msuf6RandGen.StageCount)
                (msuf6RandGen.GenRates.toString())