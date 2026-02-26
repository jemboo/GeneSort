namespace GeneSort.Model.Sorting.Sorter.Uf6
open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Core
open GeneSort.Model.Sorting

[<Struct; CustomEquality; NoComparison>]
type msuf6RandGen = 
    private 
        { id: Guid<sorterModelMakerID>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          stageLength: int<stageLength> 
          genRates: uf6GenRatesArray }

    static member create 
            (rngFactory: rngFactory) 
            (sortingWidth: int<sortingWidth>) 
            (stageLength: int<stageLength>) 
            (genRates: uf6GenRatesArray) : msuf6RandGen =
        if %sortingWidth < 6 || %sortingWidth % 2 <> 0 then
            failwith $"SortingWidth must be at least 6 and even, got {%sortingWidth}"
        if %stageLength < 1 then
            failwith $"StageLength must be at least 1, got {%stageLength}"
        if genRates.Length <> %stageLength then
            failwith $"Uf6GenRatesArray length (%d{genRates.Length}) must equal stageLength (%d{%stageLength})"
        if genRates.RatesArray |> Array.exists (fun r -> r.Order <> %sortingWidth) then
            failwith $"All Uf6GenRates in genRates must have order {%sortingWidth}"
        let id =
            [
                "msuf6RandGen" :> obj
                rngFactory :> obj
                sortingWidth :> obj
                stageLength :> obj
                genRates :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>
        { 
          id = id
          rngFactory = rngFactory
          sortingWidth = sortingWidth
          stageLength = stageLength
          genRates = genRates 
        }

    member this.Id with get() = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageLength / 2) |> UMX.tag<ceLength>
    member this.RngFactory with get() = this.rngFactory
    member this.SortingWidth with get() = this.sortingWidth
    member this.StageLength with get() = this.stageLength
    member this.GenRates with get() = this.genRates

    member this.toString() =
        sprintf "Msuf6RandGen(Id=%A, RngType=%A, SortingWidth=%d, StageLength=%d, GenRates=%s)"
                (%this.id)
                this.RngFactory
                (%this.sortingWidth)
                (%this.stageLength)
                (this.genRates.toString())

    override this.Equals(obj) =
        match obj with
        | :? msuf6RandGen as other ->
            this.id = other.id
        | _ -> false

    override this.GetHashCode() =
        hash (this.id)

    interface IEquatable<msuf6RandGen> with
        member this.Equals(other) =
            this.id = other.id

    member this.MakeSorterModelId (index: int) : Guid<sorterModelID> =
        CommonMaker.makeSorterModelId this.Id index

    member this.MakeSorterModel (index: int) : msuf6 =
        let id = this.MakeSorterModelId index
        let rng = this.RngFactory.Create %id
        let genRatesArray = this.GenRates
        let stageLength = %this.StageLength
        let twoOrbitUnfolder6s =
            [| for dex in 0 .. (stageLength - 1) ->
                RandomUnfolderOps6.makeRandomTwoOrbitUf6
                    rng.NextFloat
                    (genRatesArray.Item(dex)) |]
        msuf6.create id this.SortingWidth twoOrbitUnfolder6s


module Msuf6RandGen =

    let toString (msuf6RandGen: msuf6RandGen) : string =
        sprintf "Msuf6RandGen(Id=%A, RngType=%A, SortingWidth=%d, StageLength=%d, GenRates=%s)"
                (%msuf6RandGen.Id)
                msuf6RandGen.RngFactory
                (%msuf6RandGen.SortingWidth)
                (%msuf6RandGen.StageLength)
                (msuf6RandGen.GenRates.toString())


    let getSorterModelIdWithTag (index: int) (msuf6RandGen:msuf6RandGen) 
                    : (Guid<sorterModelID> * modelTag) =
        (msuf6RandGen.MakeSorterModelId index, modelTag.Single)