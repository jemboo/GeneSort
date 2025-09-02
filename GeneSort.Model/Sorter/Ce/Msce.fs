namespace GeneSort.Model.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sorter

[<Struct; CustomEquality; NoComparison>]
type Msce = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          ceCodes: int array } 
    with
    static member create 
            (id: Guid<sorterModelID>) 
            (sortingWidth: int<sortingWidth>)
            (ceCodes: int array) : Msce =
        if ceCodes.Length < 1 then
            failwith "Must be at least 1 Ce"
        else if %sortingWidth < 1 then
            failwith "SortingWidth must be at least 1"
        else
            { id = id; sortingWidth = sortingWidth; ceCodes = ceCodes }

    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.CeCodes with get () = this.ceCodes
    member this.CeCount with get () = (this.ceCodes.Length |> UMX.tag<ceCount>)
    member this.toString() =
        sprintf "msce(Id=%A, SortingWidth=%d, SorterLength=%d)" 
                (%this.Id) 
                (%this.SortingWidth)
                (this.CeCodes.Length)

    override this.Equals(obj) = 
        match obj with
        | :? Msce as other -> 
            this.id = other.id && 
            this.sortingWidth = other.sortingWidth &&
            this.ceCodes = other.ceCodes
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.id, this.sortingWidth, this.ceCodes)

    interface IEquatable<Msce> with
        member this.Equals(other) = 
            this.id = other.id && 
            this.sortingWidth = other.sortingWidth &&
            this.ceCodes = other.ceCodes

    member this.MakeSorter() = 
        let sw = %this.sortingWidth
        let ces = this.CeCodes |> Array.map (Ce.fromIndex)
        Sorter.create (%this.Id |> UMX.tag<sorterId>) this.SortingWidth ces


module Msce =

    let toString (msce: Msce) : string =
        sprintf "msce(Id=%A, SortingWidth=%d, SorterLength=%d)" 
                (%msce.Id) 
                (%msce.SortingWidth)
                (msce.CeCodes.Length)
