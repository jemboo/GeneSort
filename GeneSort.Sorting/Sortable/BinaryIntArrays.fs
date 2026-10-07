namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open System.Collections.Generic


module BinaryIntArrays =

    let getAllBinaryIntArrays (sia:sortableIntArray) : sortableIntArray[] =
        if sia.SortingWidth <= 1<sortingWidth> then
            [||]
        else
            let thresholds = [| 0 .. %sia.sortingWidth |]
            thresholds 
            |> Array.map (
                fun threshold ->
                    let z1Vals = sia.Values |> Array.map (fun v -> if v >= threshold then 1 else 0)
                    sortableIntArray.create(z1Vals, sia.SortingWidth, 2 |> UMX.tag<symbolSetSize>))


    /// Returns all possible sortableBoolArray instances for a given sorting width.
    /// <exception cref="ArgumentException">Thrown when sortingWidth is negative.</exception>
    let getAllBinaryIntArraysForSortingWidth (sortingWidth: int<sortingWidth>) : sortableIntArray[] =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let count = pown 2 (int sortingWidth)
        let result = Array.zeroCreate count
        for i = 0 to count - 1 do
            let z1Vals = Array.init (int sortingWidth) (fun j -> (i >>> j) &&& 1)
            result.[i] <- sortableIntArray.create(z1Vals, sortingWidth, 2 |> UMX.tag<symbolSetSize>)
        result


    let getAllSortedBinaryIntArrays (sortingWidth: int<sortingWidth>) : sortableIntArray[] =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let n = int sortingWidth
        Array.init (n + 1) (fun k ->
            let z1Vals = Array.init n (fun i -> if i >= n - k then 1 else 0)
            sortableIntArray.create(z1Vals, sortingWidth, 2 |> UMX.tag<symbolSetSize>))


    let fromLatticePoint 
            (p: GeneSort.Core.latticePoint) 
            (maxValue: int<latticeDistance>) : sortableIntArray =
    
        let dim = p.Dimension
        let mVal = %maxValue
        let totalWidth = dim * mVal
    
        // Pre-allocate the flat array. Defaults to 0, so we only need to set the 1s.
        let z1Vals = Array.zeroCreate<int> totalWidth
    
        for i = 0 to dim - 1 do
            let x = p.Coords.[i]
            let offset = i * mVal
        
            // Logic: 'y < x' reversed means the LAST 'x' elements in the block are 1.
            // If x = 0, no 1s are set.
            // If x = mVal, all elements in the block are set to 1.
            for j = 0 to x - 1 do
                // Fill from the end of the block backwards
                z1Vals.[offset + mVal - 1 - j] <- 1

        sortableIntArray.create(
            z1Vals, 
            totalWidth |> UMX.tag<sortingWidth>, 
            2 |> UMX.tag<symbolSetSize>
        )


    let fromLatticeCubeFull 
                (dim:int<latticeDimension>) 
                (maxValue:int<latticeDistance>) : sortableIntArray[] =
        let latticePoints = 
            GeneSort.Core.LatticePoint.latticeCube dim maxValue
            |> Seq.toArray

        latticePoints
        |> Array.map (fun p -> fromLatticePoint p maxValue)


    let fromLatticeCubeVV 
                (dim:int<latticeDimension>) 
                (maxValue:int<latticeDistance>) : sortableIntArray[] =
        let latticePoints = 
            GeneSort.Core.LatticePoint.latticeCube dim maxValue
            |> Seq.filter GeneSort.Core.LatticePoint.isNonDecreasing
            |> Seq.toArray

        latticePoints
        |> Array.map (fun p -> fromLatticePoint p maxValue)