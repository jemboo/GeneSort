namespace GeneSort.Model.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open System
open GeneSort.Sorter
open GeneSort.Model.Sorter

/// Represents a configuration for generating random Mssi instances with specified width and stage count.
[<Struct; CustomEquality; NoComparison>]
type MssiRandGen = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          stageCount: int<stageCount> 
        } 
    with

    static member create 
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (stageCount: int<stageCount>) 
                : MssiRandGen =
        if %sortingWidth < 2 then
            failwith $"SortingWidth must be at least 2, got {%sortingWidth}"
        else if %stageCount < 1 then
            failwith $"StageCount must be at least 1, got {%stageCount}"
        else
            let id =
                [
                    rngType :> obj
                    sortingWidth :> obj
                    %stageCount :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { 
                id = id; 
                rngType = rngType; 
                sortingWidth = sortingWidth; 
                stageCount = stageCount 
            }

    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageCount with get () = this.stageCount

    override this.Equals(obj) = 
        match obj with
        | :? MssiRandGen as other -> 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.rngType, this.sortingWidth, this.stageCount)

    interface IEquatable<MssiRandGen> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.sortingWidth = other.sortingWidth &&
            this.stageCount = other.stageCount

    interface ISorterModelMaker with
        member this.Id = this.id
        member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) 
                (index: int) : ISorterModel =
            let id = ISorterModelMaker.makeSorterModelId this index
            let rando = rngFactory this.RngType %id
            let perm_Sis = Perm_Si.makeRandoms (rando.NextIndex) (%this.SortingWidth) 
                           |> Seq.take (%this.StageCount)
                           |> Seq.toArray
            Mssi.create id this.SortingWidth perm_Sis



module MssiRandGen =

    /// Returns a string representation of the MssiRandGen.
    let toString (mssiRandGen: MssiRandGen) : string =
        sprintf "MssiRandGen(RngType=%A, Width=%d, StageCount=%d)" 
                mssiRandGen.RngType (%mssiRandGen.SortingWidth) (%mssiRandGen.StageCount)
