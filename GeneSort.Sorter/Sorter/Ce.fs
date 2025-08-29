
namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

// Type definitions
[<Struct; CustomEquality; NoComparison>]
type ce = private { low: int; hi: int } with

    static member create (lv: int) (hv: int) : ce =
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
        | :? ce as other -> this.low = other.low && this.hi = other.hi
        | _ -> false
    override this.GetHashCode() = 
        hash (this.low, this.hi)
    interface IEquatable<ce> with
        member this.Equals(other) = 
            this.low = other.low && this.hi = other.hi


// Core module for Ce operations
module Ce =

    let toString (ce: ce) : string =
        sprintf "(%d, %d)" ce.Low ce.Hi


   // mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortBy< ^a when ^a: comparison> 
                (ces: ce[]) 
                (useCounter: int[])
                (values: ^a[]) : ^a[] =

        for i = 0 to ces.Length - 1 do
            let ce = ces.[i]
            if values.[ce.Low] > values.[ce.Hi] then
                let temp = values.[ce.Low]
                values.[ce.Low] <- values.[ce.Hi]
                values.[ce.Hi] <- temp
                useCounter.[i] <- useCounter.[i] + 1
        values
         

   // mutates in placeby a sequence of ces, returning an array of the final and
   // intermediate results (values[][]) 
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortByWithHistory< ^a when ^a: comparison> 
                (ces: ce[]) 
                (useCounter: int[])
                (values: ^a[]) : ^a[][] =
        let result = Array.init (ces.Length + 1) (fun _ -> Array.copy values)
        for i = 0 to ces.Length - 1 do
            let ce = ces.[i]
            result.[i + 1] <- Array.copy result.[i]
            if result.[i + 1].[ce.Low] > result.[i + 1].[ce.Hi] then
                let temp = result.[i + 1].[ce.Low]
                result.[i + 1].[ce.Low] <- result.[i + 1].[ce.Hi]
                result.[i + 1].[ce.Hi] <- temp
                useCounter.[i] <- useCounter.[i] + 1
        result


    let maxIndexForWdith (width: int) : int =
        width*(width - 1) / 2

    let toIndex (ce: ce) : int =
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
            ce.create (pfloor - 1) (pfloor - 1)
        else
            let lo = (float dex) - (float (pfloor * (pfloor + 1))) / 2.0 |> int
            let hi = (int pfloor)
            ce.create (lo) (hi)

    let reflect 
            (sortingWidth: int<sortingWidth>) 
            (cer: ce) 
        : ce =
            ce.create 
                (cer.Hi |> GeneSort.Core.Combinatorics.reflect %sortingWidth) 
                (cer.Low |>  GeneSort.Core.Combinatorics.reflect %sortingWidth)


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
            ce.create (ceTemp.Low) (ceTemp.Hi + 1) |> toIndex
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
                ce.create (ceTemp.Low) (ceTemp.Hi + 1) |> toIndex
        }


    /// returns random Ce's to make Sorters, including Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCes (indexPicker: int -> int) 
                    (width:int) : ce seq =
        generateCeCodes indexPicker width |> Seq.map(fromIndex)


    /// returns random Ce's to make Sorters, excluding Ce's where low=hi
    /// <param name="width">The Sorter width</param>
    let generateCesExcludeSelf 
                    (indexPicker: int -> int) 
                    (width:int) : ce seq =
        generateCeCodesExcludeSelf indexPicker width |> Seq.map(fromIndex)


    let fromTwoOrbit (twoOrbit:TwoOrbit) :ce = 
            ce.create (twoOrbit.First) (twoOrbit.Second)
