namespace GeneSort.Sorting.Sortable

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

[<Struct; CustomEquality; NoComparison>]
type sortableBoolArray =
    private { 
        values: bool[] 
        channelCount: int<channelCount>
        mutable valuesHash: int option // Lazily computed hash code for Values
    }

    /// Creates a sortableBoolArray, throwing if values length does not match order.
    /// <exception cref="ArgumentException">Thrown when order is negative or values length does not equal order.</exception>
    static member create(values: bool[], channelCount: int<channelCount>) =
        if channelCount < 0<channelCount> then
            invalidArg "order" "ChannelCount must be non-negative."
        if values.Length <> int channelCount then
            invalidArg "values" $"Values length ({values.Length}) must equal order ({int channelCount})."
        { values = Array.copy values; channelCount = channelCount; valuesHash = None }

    /// Gets a copy of the values array.
    member this.Values with get() = Array.copy this.values

    /// Gets a value without exposing the backing array.
    member this.Item
        with get(index: int) = this.values.[index]

    member this.ArrayLength with get() = this.values.Length

    /// Gets the sorting order.
    member this.ChannelCount with get() = this.channelCount

    /// Computes the squared distance between this array and another sortableBoolArray, treating true as 1 and false as 0.
    member this.DistanceSquared(other: sortableBoolArray) =
        if other.ChannelCount <> this.ChannelCount then
            invalidArg "other" $"Other array ChannelCount ({int other.ChannelCount}) must equal this ChannelCount ({int this.ChannelCount})."
        let thisNumeric = this.values |> Array.map (fun b -> if b then 1 else 0)
        let otherNumeric = other.values |> Array.map (fun b -> if b then 1 else 0)
        ArrayUtils.distanceSquared thisNumeric otherNumeric

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayUtils.isSorted this.values
     
    /// Mutates and creates a new sortableBoolArray by sorting with CEs. (No use counters tracked)
    member this.SortByCes (ces: ce[]) : sortableBoolArray =
        let sortedValues = CeSorting.sortBy ces (Array.copy this.values)
        sortableBoolArray.create(sortedValues, this.ChannelCount)

    override this.Equals(obj) =
        match obj with
        | :? sortableBoolArray as other ->
            this.channelCount = other.channelCount && Array.forall2 (=) this.values other.values
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
            this.channelCount = other.channelCount && Array.forall2 (=) this.values other.values

    interface IEnumerable<bool> with
        member this.GetEnumerator() =
            (this.values :> seq<bool>).GetEnumerator()

    interface IEnumerable with
        member this.GetEnumerator() =
            (this.values :> IEnumerable).GetEnumerator()


module SortableBoolArray =

    type SortableBoolArrayValueComparer() =
        interface IEqualityComparer<sortableBoolArray> with
            member _.Equals(x, y) =
                x.ChannelCount = y.ChannelCount &&
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

        sortableBoolArray.create(result, totalWidth |> UMX.tag<channelCount>)

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
    let getAllSortableBoolArrays (channelCount: int<channelCount>) : seq<sortableBoolArray> =
        if channelCount < 0<channelCount> then
            invalidArg "channelCount" "Channel count must be non-negative."
        let w = int channelCount
        let count = 1UL <<< w
        seq {
            for i = 0UL to count - 1UL do
                let boolArray = Array.init w (fun j -> (i >>> j) &&& 1UL = 1UL)
                yield sortableBoolArray.create(boolArray, channelCount)
        }

    let getAllSortedSortableBoolArrays (channelCount: int<channelCount>) : seq<sortableBoolArray> =
        if channelCount < 0<channelCount> then
            invalidArg "channelCount" "Channel count must be non-negative."
        let n = int channelCount
        seq {
            for k = 0 to n do
                let boolArray = Array.init n (fun i -> i >= n - k)
                yield sortableBoolArray.create(boolArray, channelCount)
        }

    let getMergeTestCases 
            (channelCount: int<channelCount>)
            (mergeDimension: int<mergeDimension>)
            (mergeFillType: mergeSuffixType)
            : seq<sortableBoolArray> =

        if %channelCount % %mergeDimension <> 0 then
            invalidArg "channelCount" (sprintf "Channel count: %d must be divisible by mergeDimension : %d " %channelCount %mergeDimension)

        let latticeDimension = %mergeDimension |> UMX.tag<latticeDimension>
        let edgeLength = %channelCount / %mergeDimension |> UMX.tag<latticeDistance>

        match mergeFillType with
        | NoSuffix -> fromLatticeCubeFull latticeDimension edgeLength
        | VV_1 -> fromLatticeCubeVV_1 latticeDimension edgeLength

    /// Enumerates independent comparator components separately, then combines their images.
    /// Ordering is the same as sorting all binary inputs and keeping each first occurrence.
    let getAllPossibleResultsFromCeArray
            (ces: ce[])
            (channelCount: int<channelCount>) : seq<sortableBoolArray> =
        
        if channelCount < 0<channelCount> || channelCount >= 64<channelCount> then
            invalidArg "channelCount" "Channel count must be between 0 and 63."
        let width = int channelCount
        let parents = Array.init width id
        let rec root i =
            if parents.[i] = i then i
            else
                parents.[i] <- root parents.[i]
                parents.[i]
        for comparator in ces do
            if (int comparator.HighChannel) >= width then
                invalidArg "ces" "Comparator index exceeds channel count."
            parents.[root (int comparator.HighChannel)] <- root (int comparator.LowChannel)
        let components = [| 0 .. width - 1 |] |> Array.groupBy root |> Array.map snd
        if ces.Length = 0 || components.Length <= 1 then
            getAllSortableBoolArrays channelCount
            |> Seq.map (fun arr -> arr.SortByCes ces)
            |> removeDuplicates
        else
            seq {
                let images =
                    components |> Array.map (fun channels ->
                        let localIndex = channels |> Array.mapi (fun i channel -> channel, i) |> Map.ofArray
                        let localCes =
                            ces |> Array.filter (fun comparator -> Map.containsKey (int comparator.LowChannel) localIndex)
                                |> Array.map (fun comparator -> ce.create localIndex.[(int comparator.LowChannel)] localIndex.[(int comparator.HighChannel)])
                        let seen = HashSet<sortableBoolArray>()
                        getAllSortableBoolArrays (channels.Length |> UMX.tag<channelCount>)
                        |> Seq.map (fun input -> input, input.SortByCes localCes)
                        |> Seq.choose (fun (input, output) ->
                            if seen.Add output then
                                let mutable firstInput, result = 0UL, 0UL
                                for i in 0 .. channels.Length - 1 do
                                    if input.[i] then
                                        firstInput <- firstInput ||| (1UL <<< channels.[i])
                                    if output.[i] then result <- result ||| (1UL <<< channels.[i])
                                Some (firstInput, result)
                            else None)
                        |> Seq.toArray)
                let rec combine componentIndex firstInput result = seq {
                    if componentIndex = images.Length then yield firstInput, result
                    else
                        for input, output in images.[componentIndex] do
                            yield! combine (componentIndex + 1) (firstInput ||| input) (result ||| output)
                }
                for _, result in combine 0 0UL 0UL |> Seq.sortBy fst do
                    yield sortableBoolArray.create(Array.init width (fun i -> (result >>> i) &&& 1UL <> 0UL), channelCount)
            }
