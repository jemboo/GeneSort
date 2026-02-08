namespace GeneSort.Model.Sorting.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open System
open GeneSort.Sorting
open GeneSort.Model.Sorting

/// Represents a configuration for generating random Mssi instances with specified width and stage count.
[<Struct; CustomEquality; NoComparison>]
type MssiRandGen = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          stageLength: int<stageLength> 
        } 
    with

    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (stageLength: int<stageLength>) 
                : MssiRandGen =
        if %sortingWidth < 2 then
            failwith $"SortingWidth must be at least 2, got {%sortingWidth}"
        else if %stageLength < 1 then
            failwith $"StageLength must be at least 1, got {%stageLength}"
        else
            let id =
                [
                    rngType :> obj
                    sortingWidth :> obj
                    %stageLength :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { 
                id = id; 
                rngType = rngType; 
                sortingWidth = sortingWidth; 
                stageLength = stageLength 
            }

    member this.Id with get () = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageLength / 2) |> UMX.tag<ceLength>
    member this.RngType with get () = this.rngType
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageLength with get () = this.stageLength

    override this.Equals(obj) = 
        match obj with
        | :? MssiRandGen as other -> 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageLength = other.stageLength
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.rngType, this.sortingWidth, this.stageLength)

    interface IEquatable<MssiRandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageLength = other.stageLength


    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) 
            (index: int) : Mssi =
        let id = Common.makeSorterModelId this.Id index
        let rando = rngFactory this.RngType %id
        let perm_Sis = Perm_Si.makeRandoms (rando.NextIndex) (%this.SortingWidth) 
                        |> Seq.take (%this.StageLength)
                        |> Seq.toArray
        Mssi.create id this.SortingWidth perm_Sis



module MssiRandGen =

    /// Returns a string representation of the MssiRandGen.
    let toString (mssiRandGen: MssiRandGen) : string =
        sprintf "MssiRandGen(RngType=%A, Width=%d, StageLength=%d)" 
                mssiRandGen.RngType (%mssiRandGen.SortingWidth) (%mssiRandGen.StageLength)
