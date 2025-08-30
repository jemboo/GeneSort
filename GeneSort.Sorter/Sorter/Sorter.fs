
namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open System.Linq
open System.Collections.Generic

[<Struct; CustomEquality; NoComparison>]
type sorter =
    private { 
        sorterId: Guid<sorterId>
        sortingWidth: int<sortingWidth>
        ces: ce array
        mutable cesHash: int option // Lazily computed hash code for Ces
    }

    static member create (sorterId: Guid<sorterId>) (width: int<sortingWidth>) (ces: ce array) : sorter =
        if %sorterId = Guid.Empty then
            invalidArg "sorterId" "Sorter ID must not be empty"
        if %width < 1 then
            invalidArg "width" "Width must be at least 1"
        { sorterId = sorterId; sortingWidth = width; ces = Array.copy ces; cesHash = None }

    static member createWithNewId (width: int<sortingWidth>) (ces: ce array) : sorter =
        sorter.create (UMX.tag<sorterId> (Guid.NewGuid())) width ces

    override this.ToString() : string =
        sprintf "Sorter(Id=%A, Width=%d)" (%this.sorterId) (%this.sortingWidth)

    member this.SorterId with get() = this.sorterId
    member this.SortingWidth with get() = this.sortingWidth
    member this.Ce (dex : int) = this.ces.[dex]
    member this.ceCount with get() : int<ceCount> = this.ces.Length |> UMX.tag<ceCount>
    member this.Ces with get() = Array.copy this.ces

    override this.Equals(obj) =
        match obj with
        | :? sorter as other ->
            this.SortingWidth = other.SortingWidth &&
            Array.forall2 (=) this.ces other.ces
        | _ -> false

    override this.GetHashCode() =
        // Use cached hash if available, otherwise compute and cache
        match this.cesHash with
        | Some h -> h
        | None ->
            let mutable h = 17
            for ce in this.ces do
                h <- h * 23 + ce.GetHashCode()
            this.cesHash <- Some h
            h

    interface IEquatable<sorter> with
        member this.Equals(other) =
            this.SortingWidth = other.SortingWidth &&
            this.SorterId = other.SorterId &&
            Array.forall2 (=) this.ces other.ces


module Sorter =
    // Custom comparer for Sorter based only on Ces
    type SorterValueComparer() =
        interface IEqualityComparer<sorter> with
            member _.Equals(x, y) =
                x.SortingWidth = y.SortingWidth && // Prevent Array.forall2 crash
                Array.forall2 (=) x.ces y.ces
            member _.GetHashCode(obj) =
                // Use the struct's GetHashCode, which caches the hash of Ces
                obj.GetHashCode()

    // Function to remove duplicates based on Ces
    let removeDuplicates (arr: sorter[]) : sorter[] =
        arr.Distinct(SorterValueComparer()).ToArray()

    let create (sorterId: Guid<sorterId>) (width: int<sortingWidth>) (ces: ce array) : sorter =
        sorter.create sorterId width ces

    let createWithNewId (width: int<sortingWidth>) (ces: ce array) : sorter =
        sorter.createWithNewId width ces

    let toString (sorter: sorter) : string =
        sorter.ToString()