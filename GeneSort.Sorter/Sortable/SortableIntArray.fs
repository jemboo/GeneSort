namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open System.Linq
open System.Collections.Generic

[<Struct; CustomEquality; NoComparison>]
type sortableIntArray =
    private { 
        values: int[] 
        sortingWidth: int<sortingWidth> 
        symbolSetSize: int<symbolSetSize>
        mutable valuesHash: int option // Lazily computed hash code for Values
    }

    static member Create(values: int[], sortingWidth: int<sortingWidth>, symbolSetSize: int<symbolSetSize>) =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        if values.Length <> int sortingWidth then
            invalidArg "values" $"Values length ({values.Length}) must equal sorting width ({int sortingWidth})."
        if %symbolSetSize <= 0 then
            invalidArg "symbolSetSize" "Symbol set size must be positive."
        { values = values; sortingWidth = sortingWidth; symbolSetSize = symbolSetSize; valuesHash = None }

    static member CreateFromPermutation(perm: Permutation) =
        sortableIntArray.Create(perm.Array, (%perm.Order |> UMX.tag<sortingWidth>), (%perm.Order |> UMX.tag<symbolSetSize>))

    static member CreateSorted(sortingWidth: int<sortingWidth>) =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let values = [| 0 .. (%sortingWidth - 1) |]
        { values = values; sortingWidth = sortingWidth; symbolSetSize = %sortingWidth |> UMX.tag<symbolSetSize>; valuesHash = None }

    /// Gets the values array.
    member this.Values = this.values
    
    member this.ArrayLength with get() = this.values.Length

    /// Gets the sorting width.
    member this.SortingWidth = this.sortingWidth

    /// Gets the symbol set size.
    member this.SymbolSetSize = this.symbolSetSize

    /// Computes the squared distance between this array and another sortableIntArray.
    member this.DistanceSquared(other: sortableIntArray) =
        if other.sortingWidth <> this.sortingWidth then
            invalidArg "other" $"Other array sorting width ({int other.sortingWidth}) must equal this sorting width ({int this.sortingWidth})."
        ArrayProperties.distanceSquared this.values other.values

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayProperties.isSorted this.Values
    
    member this.SortByCes
                (ces: ce[])
                (useCounter: int[]) : sortableIntArray =
        let sortedValues = Ce.sortBy ces useCounter (Array.copy this.values)
        sortableIntArray.Create(sortedValues, this.SortingWidth, this.SymbolSetSize)

    member this.SortByCesWithHistory 
                (ces: ce[])
                (useCounter: int[]) : sortableIntArray[] =
        let history = Ce.sortByWithHistory ces useCounter this.values
        let sw = this.SortingWidth
        let sss = this.SymbolSetSize
        history |> Array.map (fun values -> sortableIntArray.Create(values, sw, sss))

    member this.ToSortableBoolArrays() : sortableBoolArray[] =
        if this.SortingWidth <= 1<sortingWidth> then
            [||]
        else
            let minValue = Array.min this.values
            //let thresholds = 
            //    this.values 
            //    |> Array.filter (fun v -> v >= minValue) 
            //    |> Array.distinct
            //    |> Array.sort
            let thresholds = [| 0 .. (%this.sortingWidth + 1) |]

            let vals = this.Values
            let sw = this.SortingWidth
            thresholds 
            |> Array.map (
                fun threshold ->
                    let boolValues = vals |> Array.map (fun v -> v >= threshold )
                    sortableBoolArray.Create(boolValues, sw))


    member this.ToPermutation() : Permutation =
        Permutation.createUnsafe this.values

    override this.Equals(obj) =
        match obj with
        | :? sortableIntArray as other ->
            this.sortingWidth = other.sortingWidth &&
            Array.forall2 (=) this.values other.values
        | _ -> false


    override this.GetHashCode() =
        // Use cached hash if available, otherwise compute and cache
        match this.valuesHash with
        | Some h -> h
        | None ->
            let mutable h = 17
            for v in this.values do
                h <- h * 23 + v.GetHashCode()
            this.valuesHash <- Some h
            h

    interface IEquatable<sortableIntArray> with
        member this.Equals(other) =
            this.sortingWidth = other.sortingWidth &&
            this.symbolSetSize = other.symbolSetSize &&
            Array.forall2 (=) this.values other.values




module SortableIntArray =
    // Custom comparer for sortableIntArray based only on Values
    type SortableIntArrayValueComparer() =
        interface IEqualityComparer<sortableIntArray> with
            member _.Equals(x, y) =
                x.SortingWidth = y.SortingWidth && // Ensure same length to prevent Array.forall2 crash
                Array.forall2 (=) x.Values y.Values
            member _.GetHashCode(obj) =
                // Use the struct's GetHashCode, which caches the hash of Values
                obj.GetHashCode()


    // Function to remove duplicates based on Values
    let removeDuplicates (arr: sortableIntArray[]) : sortableIntArray[] =
        arr.Distinct(SortableIntArrayValueComparer()).ToArray()


    let getOrbit (maxCount: int) (perm: Permutation) : sortableIntArray seq =
        Permutation.powerSequence perm  
        |> CollectionUtils.takeUpToOrWhile maxCount (fun perm -> not (Permutation.isIdentity perm))
        |> Seq.map sortableIntArray.CreateFromPermutation


    let randomSortableIntArray (indexShuffler: int -> int) (sortingWidth: int<sortingWidth>) : sortableIntArray =
        let perm = Permutation.randomPermutation indexShuffler %sortingWidth
        sortableIntArray.CreateFromPermutation perm


    //let getMerge2TestCases (sortingWidth: int<sortingWidth>) : sortableIntArray [] =
    //    let hw = %sortingWidth / 2
    //    let sw = %sortingWidth
    //    [|
    //        for i = 0 to hw do
    //            let ad1 = Array.append [| 0 .. (%hw - 1 - i) |] [| (sw - i) .. (sw - 1) |]
    //            let arrayData = Array.append ad1  [| (sw - hw - i) .. (sw - i - 1) |]
    //            sortableIntArray.Create(arrayData, sortingWidth, (%sortingWidth |> UMX.tag<symbolSetSize>))
    //    |]

    let getMerge2TestCases (sortingWidth: int<sortingWidth>) : sortableIntArray [] =
        let segLen = %sortingWidth / 2
        let input = [| 0 .. (%sortingWidth - 1) |]
        [|
            for gen = 0 to segLen do
                let arrayData = ArrayUtils.arrayPinch input segLen gen (0) (segLen)
                sortableIntArray.Create(arrayData, sortingWidth, (%sortingWidth |> UMX.tag<symbolSetSize>))
        |]

    let getMerge3TestCases (sortingWidth: int<sortingWidth>) : sortableIntArray [] =
        if %sortingWidth % 3 <> 0 then
            invalidArg "sortingWidth" "Sorting width must be divisible by 3 for merge3 test cases."
        let segLen = %sortingWidth / 3
        let input = [| 0 .. (%sortingWidth - 1) |]
        let outers = [| 
            for wkR = 0 to segLen do
                    ArrayUtils.arrayPinch input segLen wkR (0) (2 * segLen)
        |]
        let fullSet = 
            outers |> Array.collect (fun outer ->
                [|
                    for gen = 0 to segLen do
                        let pinched = ArrayUtils.arrayPinch outer segLen gen (segLen) (2 * segLen)
                        sortableIntArray.Create(pinched, sortingWidth, (%sortingWidth |> UMX.tag<symbolSetSize>))
                |]
            )
        fullSet
                        

    let getMerge4TestCases (sortingWidth: int<sortingWidth>) : sortableIntArray [] =
        if %sortingWidth % 4 <> 0 then
            invalidArg "sortingWidth" "Sorting width must be divisible by 4 for merge3 test cases."
        let segLen = %sortingWidth / 4
        let input = [| 0 .. (%sortingWidth - 1) |]
        let outers = [| 
            for wkR = 0 to segLen do
                    ArrayUtils.arrayPinch input segLen wkR (0) (3 * segLen)
        |]
        let mids = 
            outers |> Array.collect (fun outer ->
                [|
                    for gen = 0 to segLen do
                        ArrayUtils.arrayPinch outer segLen gen (segLen) (3 * segLen)
                |]
            )
        let fullSet = 
            mids |> Array.collect (fun outer ->
                [|
                    for gen = 0 to segLen do
                        let pinched = ArrayUtils.arrayPinch outer segLen gen (2 * segLen) (3 * segLen)
                        sortableIntArray.Create(pinched, sortingWidth, (%sortingWidth |> UMX.tag<symbolSetSize>))
                |]
            )
        fullSet
