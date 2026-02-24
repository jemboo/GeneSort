namespace GeneSort.Model.Sorting.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open System
open GeneSort.Sorting
open GeneSort.Model.Sorting

/// Represents a configuration for generating random Mssi instances with specified width and stage count.
[<Struct; CustomEquality; NoComparison>]
type mssiRandGen = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          stageLength: int<stageLength> 
        } 
    with

    static member create 
            (rngFactory: rngFactory) 
            (sortingWidth: int<sortingWidth>)
            (stageLength: int<stageLength>) 
                : mssiRandGen =
        if %sortingWidth < 2 then
            failwith $"SortingWidth must be at least 2, got {%sortingWidth}"
        else if %stageLength < 1 then
            failwith $"StageLength must be at least 1, got {%stageLength}"
        else
            let id =
                [
                    rngFactory :> obj
                    sortingWidth :> obj
                    %stageLength :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

            { 
                id = id; 
                rngFactory = rngFactory; 
                sortingWidth = sortingWidth; 
                stageLength = stageLength 
            }

    member this.Id with get () = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageLength / 2) |> UMX.tag<ceLength>
    member this.RngFactory with get () = this.rngFactory
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageLength with get () = this.stageLength

    override this.Equals(obj) = 
        match obj with
        | :? mssiRandGen as other -> 
            this.RngFactory = other.RngFactory && 
            this.sortingWidth = other.sortingWidth &&
            this.stageLength = other.stageLength
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.RngFactory, this.sortingWidth, this.stageLength)

    interface IEquatable<mssiRandGen> with
        member this.Equals(other) = 
            this.RngFactory = other.RngFactory && 
            this.sortingWidth = other.sortingWidth &&
            this.stageLength = other.stageLength


    member this.MakeSorterModel
            (index: int) : mssi =
        let id = CommonMaker.makeSorterModelId this.Id index
        let rando = this.RngFactory.Create %id
        let perm_Sis = Perm_Si.makeRandoms (rando.NextIndex) (%this.SortingWidth) 
                        |> Seq.take (%this.StageLength)
                        |> Seq.toArray
        mssi.create id this.SortingWidth perm_Sis



module MssiRandGen =

    /// Returns a string representation of the MssiRandGen.
    let toString (mssiRandGen: mssiRandGen) : string =
        sprintf "MssiRandGen(RngType=%A, Width=%d, StageLength=%d)" 
                mssiRandGen.RngFactory (%mssiRandGen.SortingWidth) (%mssiRandGen.StageLength)
