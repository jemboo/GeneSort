﻿namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
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
    static member Create(values: bool[], sortingWidth: int<sortingWidth>) =
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
        ArrayProperties.distanceSquared thisNumeric otherNumeric

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayProperties.isSorted this.values
     
    member this.SortByCes
                (ces: Ce[]) 
                (useCounter: int[]) : sortableBoolArray =
        let sortedValues = Ce.sortBy ces useCounter (Array.copy this.values)
        sortableBoolArray.Create(sortedValues, this.SortingWidth)

    member this.SortByCesWithHistory 
                (ces: Ce[])
                (useCounter: int[]) : sortableBoolArray[] =
        let history = Ce.sortByWithHistory ces useCounter this.values
        let sw = this.SortingWidth
        history |> Array.map (fun values -> sortableBoolArray.Create(values, sw))

    override this.Equals(obj) =
        match obj with
        | :? sortableBoolArray as other ->
            this.sortingWidth = other.sortingWidth && Array.forall2 (=) this.values other.values
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

    interface IEquatable<sortableBoolArray> with
        member this.Equals(other) =
            this.sortingWidth = other.sortingWidth && Array.forall2 (=) this.values other.values

module SortableBoolArray =
    // Custom comparer for sortableBoolArray based only on Values
    type SortableBoolArrayValueComparer() =
        interface IEqualityComparer<sortableBoolArray> with
            member _.Equals(x, y) =
                x.SortingWidth = y.SortingWidth && // Prevent Array.forall2 crash
                Array.forall2 (=) x.Values y.Values
            member _.GetHashCode(obj) =
                // Use the struct's GetHashCode, which caches the hash of Values
                obj.GetHashCode()

    // Function to remove duplicates based on Values
    let removeDuplicates (arr: sortableBoolArray[]) : sortableBoolArray[] =
        arr.Distinct(SortableBoolArrayValueComparer()).ToArray()

    /// Returns all possible sortableBoolArray instances for a given sorting width.
    /// <exception cref="ArgumentException">Thrown when sortingWidth is negative.</exception>
    let getAllSortableBoolArrays (sortingWidth: int<sortingWidth>) : sortableBoolArray[] =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let count = pown 2 (int sortingWidth)
        let result = Array.zeroCreate count
        for i = 0 to count - 1 do
            let boolArray = Array.init (int sortingWidth) (fun j -> (i >>> j) &&& 1 = 1)
            result.[i] <- sortableBoolArray.Create(boolArray, sortingWidth)
        result

    let getAllSortedSortableBoolArrays (sortingWidth: int<sortingWidth>) : sortableBoolArray[] =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let n = int sortingWidth
        Array.init (n + 1) (fun k ->
            let boolArray = Array.init n (fun i -> i >= n - k)
            sortableBoolArray.Create(boolArray, sortingWidth))

    let getMergeSortTestCases (sortingWidth: int<sortingWidth>) : sortableBoolArray[] =
        if %sortingWidth % 2 <> 0 then
            invalidArg "sortingWidth" "Sorting width must be even."
        let n = int sortingWidth
        let m = n / 2
        let sortedHalfArrays = getAllSortedSortableBoolArrays (m * 1<sortingWidth>)
        let result = ResizeArray<sortableBoolArray>()
        for left in sortedHalfArrays do
            for right in sortedHalfArrays do
                let boolArray = Array.concat [left.Values; right.Values]
                let sba = sortableBoolArray.Create(boolArray, sortingWidth)
                result.Add(sba)
        result.ToArray()