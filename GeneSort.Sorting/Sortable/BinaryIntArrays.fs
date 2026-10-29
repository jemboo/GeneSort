namespace GeneSort.Sorting.Sortable

open System
open System.Collections.Generic
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter


module BinaryIntArrays =

    let getAllBinaryIntArrays (sia:sortableIntArray) : sortableIntArray[] =
        if sia.ChannelCount <= 1<channelCount> then
            [||]
        else
            let thresholds = [| 0 .. %sia.channelCount |]
            thresholds 
            |> Array.map (
                fun threshold ->
                    let z1Vals = sia.Values |> Array.map (fun v -> if v >= threshold then 1 else 0)
                    sortableIntArray.create(z1Vals, sia.ChannelCount, 2 |> UMX.tag<symbolSetSize>))


    /// Returns all possible sortableBoolArray instances for a given channel count.
    /// <exception cref="ArgumentException">Thrown when channelCount is negative.</exception>
    let getAllBinaryIntArraysForChannelCount (channelCount: int<channelCount>) : sortableIntArray[] =
        if channelCount < 0<channelCount> then
            invalidArg "channelCount" "Channel count must be non-negative."
        let count = pown 2 (int channelCount)
        let result = Array.zeroCreate count
        for i = 0 to count - 1 do
            let z1Vals = Array.init (int channelCount) (fun j -> (i >>> j) &&& 1)
            result.[i] <- sortableIntArray.create(z1Vals, channelCount, 2 |> UMX.tag<symbolSetSize>)
        result


    let getAllSortedBinaryIntArrays (channelCount: int<channelCount>) : sortableIntArray[] =
        if channelCount < 0<channelCount> then
            invalidArg "channelCount" "Channel count must be non-negative."
        let n = int channelCount
        Array.init (n + 1) (fun k ->
            let z1Vals = Array.init n (fun i -> if i >= n - k then 1 else 0)
            sortableIntArray.create(z1Vals, channelCount, 2 |> UMX.tag<symbolSetSize>))


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
            totalWidth |> UMX.tag<channelCount>,
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