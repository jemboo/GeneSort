﻿namespace GeneSort.Model.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

[<Struct; CustomEquality; NoComparison>]
type MsceRandGen = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          excludeSelfCe: bool
          ceCount: int<ceCount> } 
    with
    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>) 
            (excludeSelfCe: bool) 
            (ceCount: int<ceCount>) : MsceRandGen =
        if %ceCount < 1 then
            failwith "ceCount length must be at least 1"
        else if %sortingWidth < 1 then
            failwith "Width must be at least 1"
        else
            let id =
                [
                    rngType :> obj
                    sortingWidth :> obj
                    excludeSelfCe :> obj
                    %ceCount :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { id = id;
              rngType = rngType; 
              sortingWidth = sortingWidth; 
              excludeSelfCe = excludeSelfCe; 
              ceCount = ceCount }
    

    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.SortingWidth with get () = this.sortingWidth
    member this.ExcludeSelfCe with get () = this.excludeSelfCe
    member this.CeCount with get () = this.ceCount

    override this.Equals(obj) = 
        match obj with
        | :? MsceRandGen as other -> 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth && 
            this.excludeSelfCe = other.excludeSelfCe && 
            this.ceCount = other.ceCount
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.sortingWidth, this.excludeSelfCe, this.ceCount)

    interface IEquatable<MsceRandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth && 
            this.excludeSelfCe = other.excludeSelfCe && 
            this.ceCount = other.ceCount

    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) : Msce =
        let id = [
                    this.Id  :> obj
                    index :> obj
                 ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

        let rando = rngFactory this.RngType %id
        let ceCodes = 
            if this.ExcludeSelfCe then
                Ce.generateCeCodesExcludeSelf (rando.NextIndex) %this.SortingWidth
                |> Seq.take %this.sortingWidth
                |> Seq.toArray
            else
                Ce.generateCeCodes (rando.NextIndex) %this.SortingWidth
                |> Seq.take %this.CeCount
                |> Seq.toArray

        Msce.create
            id 
            this.SortingWidth
            ceCodes




module MsceRandGen =

    let toString (msceRandGen: MsceRandGen) : string =
        sprintf "Model_CeGen(rngType=%A, Width=%d, Length=[%d])" 
                    (msceRandGen.RngType) (%msceRandGen.SortingWidth) (%msceRandGen.CeCount)

         
