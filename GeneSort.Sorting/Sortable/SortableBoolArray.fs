namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open System.Linq
open System.Collections.Generic

[<Struct; CustomEquality; NoComparison>]
type sortableBoolArray =
    private { 
        values: bool[] 
        sortingWidth: int<sortingWidth>
        mutable valuesHash: int option // Lazily computed hash code for Values
    }

    /// Creates a sortableBoolArray, throwing if values length does not match order.
    /// <exception cref="ArgumentException">Thrown when order is negative or values length does not equal order.</exception>
    static member create(values: bool[], sortingWidth: int<sortingWidth>) =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "order" "SortingWidth must be non-negative."
        if values.Length <> int sortingWidth then
            invalidArg "values" $"Values length ({values.Length}) must equal order ({int sortingWidth})."
        { values = Array.copy values; sortingWidth = sortingWidth; valuesHash = None }

    /// Gets the values array.
    member this.Values with get() = this.values

    member this.ArrayLength with get() = this.values.Length

    /// Gets the sorting order.
    member this.SortingWidth with get() = this.sortingWidth

    /// Computes the squared distance between this array and another sortableBoolArray, treating true as 1 and false as 0.
    member this.DistanceSquared(other: sortableBoolArray) =
        if other.SortingWidth <> this.SortingWidth then
            invalidArg "other" $"Other array SortingWidth ({int other.SortingWidth}) must equal this SortingWidth ({int this.SortingWidth})."
        let thisNumeric = this.values |> Array.map (fun b -> if b then 1 else 0)
        let otherNumeric = other.values |> Array.map (fun b -> if b then 1 else 0)
        ArrayUtils.distanceSquared thisNumeric otherNumeric

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayUtils.isSorted this.values
     
    /// Mutates and creates a new sortableBoolArray by sorting with CEs. (No use counters tracked)
    member this.SortByCes (ces: ce[]) : sortableBoolArray =
        let sortedValues = Ce.sortBy ces (Array.copy this.values)
        sortableBoolArray.create(sortedValues, this.SortingWidth)

    override this.Equals(obj) =
        match obj with
        | :? sortableBoolArray as other ->
            this.sortingWidth = other.sortingWidth && Array.forall2 (=) this.values other.values
        | _ -> false

    override this.GetHashCode() =
        match this.valuesHash with
        | Some h -> h
        | None ->
            let mutable h = 17
            for v in this.values do
                h <- h * 23 + v.GetHashCode()
            this.valuesHash <- Some h
            h

    interface IEquatable<sortableBoolArray> with
        member this.Equals(other) =
            this.sortingWidth = other.sortingWidth && Array.forall2 (=) this.values other.values


module SortableBoolArray =

    type SortableBoolArrayValueComparer() =
        interface IEqualityComparer<sortableBoolArray> with
            member _.Equals(x, y) =
                x.SortingWidth = y.SortingWidth && 
                Array.forall2 (=) x.Values y.Values
            member _.GetHashCode(obj) =
                obj.GetHashCode()

    let removeDuplicates (arr: seq<sortableBoolArray>) : seq<sortableBoolArray> =
        arr.Distinct(SortableBoolArrayValueComparer())

    let fromLatticePoint 
            (p: GeneSort.Core.latticePoint) 
            (maxValue: int<latticeDistance>) : sortableBoolArray =
    
        let dim = p.Dimension
        let mVal = %maxValue
        let totalWidth = dim * mVal
        let result = Array.zeroCreate<bool> totalWidth
    
        for i = 0 to dim - 1 do
            let x = p.Coords.[i]
            let offset = i * mVal
            for j = 0 to mVal - 1 do
                if (mVal - 1 - j) < x then
                    result.[offset + j] <- true

        sortableBoolArray.create(result, totalWidth |> UMX.tag<sortingWidth>)

    let fromLatticeCubeFull 
                (dim:int<latticeDimension>) 
                (maxValue:int<latticeDistance>) : seq<sortableBoolArray> =
        LatticePoint.latticeCube dim maxValue
        |> Seq.map (fun p -> fromLatticePoint p maxValue)

    let fromLatticeCubeVV_1 
                (dim:int<latticeDimension>) 
                (maxValue:int<latticeDistance>) : seq<sortableBoolArray> =
        LatticePoint.latticeCube dim maxValue
        |> Seq.filter GeneSort.Core.LatticePoint.isNonDecreasing
        |> Seq.map (fun p -> fromLatticePoint p maxValue)

    /// Streams all possible combinations on-demand using 64-bit integer limits.
    let getAllSortableBoolArrays (sortingWidth: int<sortingWidth>) : seq<sortableBoolArray> =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let w = int sortingWidth
        let count = 1UL <<< w
        seq {
            for i = 0UL to count - 1UL do
                let boolArray = Array.init w (fun j -> (i >>> j) &&& 1UL = 1UL)
                yield sortableBoolArray.create(boolArray, sortingWidth)
        }

    let getAllSortedSortableBoolArrays (sortingWidth: int<sortingWidth>) : seq<sortableBoolArray> =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let n = int sortingWidth
        seq {
            for k = 0 to n do
                let boolArray = Array.init n (fun i -> i >= n - k)
                yield sortableBoolArray.create(boolArray, sortingWidth)
        }

    let getMergeTestCases 
            (sortingWidth: int<sortingWidth>) 
            (mergeDimension: int<mergeDimension>)
            (mergeFillType: mergeSuffixType)
            : seq<sortableBoolArray> =

        if %sortingWidth % %mergeDimension <> 0 then
            invalidArg "sortingWidth" (sprintf "Sorting width: %d must be divisible by mergeDimension : %d " %sortingWidth %mergeDimension)

        let latticeDimension = %mergeDimension |> UMX.tag<latticeDimension>
        let edgeLength = %sortingWidth / %mergeDimension |> UMX.tag<latticeDistance>

        match mergeFillType with
        | NoSuffix -> fromLatticeCubeFull latticeDimension edgeLength
        | VV_1 -> fromLatticeCubeVV_1 latticeDimension edgeLength

    /// Streams values on-demand, filters out duplicates lazily, and drops history/counters entirely.
    let getAllPossibleResultsFromCeArray
            (ces: ce[])
            (sortingWidth: int<sortingWidth>) : seq<sortableBoolArray> =
        
        let initialArrays = getAllSortableBoolArrays sortingWidth
        
        initialArrays
        |> Seq.map (fun arr -> arr.SortByCes ces)
        |> removeDuplicates