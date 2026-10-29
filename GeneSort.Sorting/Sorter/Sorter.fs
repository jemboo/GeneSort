
namespace GeneSort.Sorting.Sorter

open System
open System.Collections.Generic
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

[<Struct; CustomEquality; NoComparison>]
type sorter =
    private { 
        sorterId: Guid<sorterId>
        channelCount: int<channelCount>
        ces: ce array
        mutable cesHash: int option // Lazily computed hash code for Ces
    }

    static member create (sorterId: Guid<sorterId>) (width: int<channelCount>) (ces: ce array) : sorter =
        if %sorterId = Guid.Empty then
            invalidArg "sorterId" "Sorter ID must not be empty"
        if %width < 1 then
            invalidArg "width" "Width must be at least 1"
        { sorterId = sorterId; channelCount = width; ces = Array.copy ces; cesHash = None }

    static member createWithNewId (width: int<channelCount>) (ces: ce array) : sorter =
        sorter.create (UMX.tag<sorterId> (Guid.NewGuid())) width ces

    override this.ToString() : string =
        sprintf "Sorter(Id=%A, Width=%d)" (%this.sorterId) (%this.channelCount)

    member this.SorterId with get() = this.sorterId
    member this.ChannelCount with get() = this.channelCount
    member this.Ce (dex : int) = this.ces.[dex]
    member this.ceLength with get() : int<ceLength> = this.ces.Length |> UMX.tag<ceLength>
    member this.Ces with get() = Array.copy this.ces

    override this.Equals(obj) =
        match obj with
        | :? sorter as other ->
            this.ChannelCount = other.ChannelCount &&
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
            this.ChannelCount = other.ChannelCount &&
            this.SorterId = other.SorterId &&
            Array.forall2 (=) this.ces other.ces


module Sorter =
    // Custom comparer for Sorter based only on Ces
    type SorterValueComparer() =
        interface IEqualityComparer<sorter> with
            member _.Equals(x, y) =
                x.ChannelCount = y.ChannelCount && // Prevent Array.forall2 crash
                Array.forall2 (=) x.ces y.ces
            member _.GetHashCode(obj) =
                // Use the struct's GetHashCode, which caches the hash of Ces
                obj.GetHashCode()

    /// Concatenates two sorters by appending the suffix's CEs to the prefix's CEs.
    /// The resulting sorter has the specified new ID.
    /// Throws an exception if the sorters have different channel counts.
    let concatSorters (prefix: sorter) (suffix: sorter) (newId: Guid<sorterId>) : sorter =
        if prefix.ChannelCount <> suffix.ChannelCount then
            failwith $"Cannot concatenate sorters with different channel counts. Prefix: 
                    {%prefix.ChannelCount}, Suffix: {%suffix.ChannelCount}"
        
        let concatenatedCes = Array.append prefix.Ces suffix.Ces
        
        sorter.create newId prefix.ChannelCount concatenatedCes


    // returns a dataTableRecord with the sorterId, channelCount, and ces serialized as a string
    let toDataTableRecord (sorter:sorter) : dataTableRecord =
        let ceString = sorter.Ces |> Ce.arrayToString
        
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData "SorterId" ((%sorter.SorterId).ToString())
        |> dataTableRecord.addData "ChannelCount" ((%sorter.ChannelCount).ToString())
        |> dataTableRecord.addData "Ces" ceString