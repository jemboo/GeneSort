namespace GeneSort.Model.Sorting.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting

[<Struct; CustomEquality; NoComparison>]
type MsceRandGen = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          excludeSelfCe: bool
          ceLength: int<ceLength> } 
    with
    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>) 
            (excludeSelfCe: bool) 
            (ceLength: int<ceLength>) : MsceRandGen =
        if %ceLength < 1 then
            failwith "ceLength length must be at least 1"
        else if %sortingWidth < 1 then
            failwith "Width must be at least 1"
        else
            let id =
                [
                    rngType :> obj
                    sortingWidth :> obj
                    excludeSelfCe :> obj
                    %ceLength :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { id = id;
              rngType = rngType; 
              sortingWidth = sortingWidth; 
              excludeSelfCe = excludeSelfCe; 
              ceLength = ceLength }
    

    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.SortingWidth with get () = this.sortingWidth
    member this.ExcludeSelfCe with get () = this.excludeSelfCe
    member this.CeLength with get () = this.ceLength

    override this.Equals(obj) = 
        match obj with
        | :? MsceRandGen as other -> 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth && 
            this.excludeSelfCe = other.excludeSelfCe && 
            this.ceLength = other.ceLength
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.sortingWidth, this.excludeSelfCe, this.ceLength)

    interface IEquatable<MsceRandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth && 
            this.excludeSelfCe = other.excludeSelfCe && 
            this.ceLength = other.ceLength

    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) : Msce =
        let id = Common.makeSorterModelId this.Id index
        let rando = rngFactory this.RngType %id
        let ceCodes = 
            if this.ExcludeSelfCe then
                Ce.generateCeCodesExcludeSelf (rando.NextIndex) %this.SortingWidth
                |> Seq.take %this.ceLength
                |> Seq.toArray
            else
                Ce.generateCeCodes (rando.NextIndex) %this.SortingWidth
                |> Seq.take %this.ceLength
                |> Seq.toArray

        Msce.create
            id 
            this.SortingWidth
            ceCodes




module MsceRandGen =

    let toString (msceRandGen: MsceRandGen) : string =
        sprintf "Model_CeGen(rngType=%A, Width=%d, Length=[%d])" 
                    (msceRandGen.RngType) (%msceRandGen.SortingWidth) (%msceRandGen.ceLength)

         
