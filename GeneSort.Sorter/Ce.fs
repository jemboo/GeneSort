namespace GeneSort.Sorter

open System
open FSharp.UMX
open GeneSort.Core

// Type definitions
[<Struct; CustomEquality; NoComparison>]
type Ce = private { low: int; hi: int } with

    static member create (lv: int) (hv: int) : Ce =
        if lv < 0 || hv < 0 then
            failwith "Indices must be non-negative"
        else if lv < hv then
            { low = lv; hi = hv }
        else
            { low = hv; hi = lv }

    /// Gets the first TwoOrbit.
    member this.Low with get () = this.low

    /// Gets the second TwoOrbit (if present).
    member this.Hi with get () = this.hi

    override this.Equals(obj) = 
        match obj with
        | :? Ce as other -> this.low = other.low && this.hi = other.hi
        | _ -> false
    override this.GetHashCode() = 
        hash (this.low, this.hi)
    interface IEquatable<Ce> with
        member this.Equals(other) = 
            this.low = other.low && this.hi = other.hi

// Core module for Ce operations
module Ce =

    let toString (ce: Ce) : string =
        sprintf "(%d, %d)" ce.Low ce.Hi

    let maxIndexForWdith (width: int) : int =
        width*(width - 1) / 2

    let toIndex (ce: Ce) : int =
        let i = ce.Low
        let j = ce.Hi
        (j * (j + 1)) / 2 + i

    let fromIndex (dex:int) = 
        if dex < 0 then
            failwith "Index must be non-negative"
        else
        let indexFlt = (dex |> float) + 1.0
        let p = (sqrt (1.0 + 8.0 * indexFlt) - 1.0) / 2.0
        let pfloor = int p
        if (p = pfloor) then 
            Ce.create (pfloor - 1) (pfloor - 1)
        else
            let lo = (float dex) - (float (pfloor * (pfloor + 1))) / 2.0 |> int
            let hi = (int pfloor)
            Ce.create (lo) (hi)

    let reflect 
            (sortingWidth: int<sortingWidth>) 
            (ce: Ce) 
        : Ce =
        Ce.create 
                (ce.Hi |> GeneSort.Core.Combinatorics.reflect %sortingWidth) 
                (ce.Low |>  GeneSort.Core.Combinatorics.reflect %sortingWidth)


    let generateCeCode (excludeSelfCe:bool)
                       (width:int) 
                       (indexPicker: int -> int) : int =
        if width < 1 then
            failwith "Width must be at least 1"
        let ww = if excludeSelfCe then width else width + 1
        let indexMax = maxIndexForWdith ww
        let dex = indexPicker indexMax
        if excludeSelfCe then
            let ceTemp = fromIndex dex
            Ce.create (ceTemp.Low) (ceTemp.Hi + 1) |> toIndex
        else
            dex


    /// returns random Ce's to make Sorters, including Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCeCodes (indexPicker: int -> int) 
                        (width:int) : int seq =
        if width < 1 then
            failwith "Width must be at least 1"
        let indexMax = maxIndexForWdith width
        seq {
            while true do
                indexPicker indexMax
        }


    /// returns random Ce's to make Sorters, excluding Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCeCodesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : int seq =
        if width < 2 then
            failwith "Width must be at least 2"
        let indexMax = maxIndexForWdith (width - 1)
        seq {
            while true do
                let ceTemp = indexPicker indexMax |> fromIndex
                Ce.create (ceTemp.Low) (ceTemp.Hi + 1) |> toIndex
        }


    /// returns random Ce's to make Sorters, including Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCes (indexPicker: int -> int) 
                    (width:int) : Ce seq =
        generateCeCodes indexPicker width |> Seq.map(fromIndex)


    /// returns random Ce's to make Sorters, excluding Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : Ce seq =
        generateCeCodesExcludeSelf indexPicker width |> Seq.map(fromIndex)


    let fromTwoOrbit (twoOrbit:TwoOrbit) :Ce = 
            Ce.create (twoOrbit.First) (twoOrbit.Second)
