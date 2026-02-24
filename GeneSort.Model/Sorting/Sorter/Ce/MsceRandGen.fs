namespace GeneSort.Model.Sorting.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting

[<Struct; CustomEquality; NoComparison>]
type msceRandGen = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          excludeSelfCe: bool
          ceLength: int<ceLength> } 
    with
    static member create 
            (rngFactory: rngFactory) 
            (sortingWidth: int<sortingWidth>) 
            (excludeSelfCe: bool) 
            (ceLength: int<ceLength>) : msceRandGen =
        if %ceLength < 1 then
            failwith "ceLength length must be at least 1"
        else if %sortingWidth < 1 then
            failwith "Width must be at least 1"
        else
            let id =
                [
                    rngFactory :> obj
                    sortingWidth :> obj
                    excludeSelfCe :> obj
                    %ceLength :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { id = id;
              rngFactory = rngFactory; 
              sortingWidth = sortingWidth; 
              excludeSelfCe = excludeSelfCe; 
              ceLength = ceLength }
    

    member this.Id with get () = this.id
    member this.RngFactory with get () = this.rngFactory
    member this.SortingWidth with get () = this.sortingWidth
    member this.ExcludeSelfCe with get () = this.excludeSelfCe
    member this.CeLength with get () = this.ceLength

    override this.Equals(obj) = 
        match obj with
        | :? msceRandGen as other -> 
            this.rngFactory = other.rngFactory && 
            this.sortingWidth = other.sortingWidth && 
            this.excludeSelfCe = other.excludeSelfCe && 
            this.ceLength = other.ceLength
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.sortingWidth, this.excludeSelfCe, this.ceLength)

    interface IEquatable<msceRandGen> with
        member this.Equals(other) = 
            this.rngFactory = other.rngFactory && 
            this.sortingWidth = other.sortingWidth && 
            this.excludeSelfCe = other.excludeSelfCe && 
            this.ceLength = other.ceLength

    member this.MakeSorterModel (index: int) : msce =
        let id = CommonMaker.makeSorterModelId this.Id index
        let rando = this.RngFactory.Create %id
        let ceCodes = 
            if this.ExcludeSelfCe then
                Ce.generateCeCodesExcludeSelf (rando.NextIndex) %this.SortingWidth
                |> Seq.take %this.ceLength
                |> Seq.toArray
            else
                Ce.generateCeCodes (rando.NextIndex) %this.SortingWidth
                |> Seq.take %this.ceLength
                |> Seq.toArray

        msce.create
            id 
            this.SortingWidth
            ceCodes




module MsceRandGen =

    let toString (msceRandGen: msceRandGen) : string =
        sprintf "Model_CeGen(rngType=%A, Width=%d, Length=[%d])" 
                    (msceRandGen.RngFactory) (%msceRandGen.SortingWidth) (%msceRandGen.ceLength)

         
