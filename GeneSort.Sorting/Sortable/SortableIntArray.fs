namespace GeneSort.Sorting.Sortable

open System
open System.Collections
open System.Collections.Generic
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter


[<Struct; CustomEquality; NoComparison>]
type sortableIntArray =
    private { 
        values: int[] 
        channelCount: int<channelCount>
        symbolSetSize: int<symbolSetSize>
        // Eagerly computed hash for thread-safety and struct consistency
        vHash: int 
    }

    static member create(values: int[], channelCount: int<channelCount>, symbolSetSize: int<symbolSetSize>) =
        if channelCount < 0<channelCount> then
            invalidArg "channelCount" "Channel count must be non-negative."
        if values.Length <> int channelCount then
            invalidArg "values" $"Values length ({values.Length}) must equal channel count ({int channelCount})."
        if %symbolSetSize <= 0 then
            invalidArg "symbolSetSize" "Symbol set size must be positive."
        let storedValues = Array.copy values
        let mutable h = 17
        for v in storedValues do
            h <- h * 23 + v.GetHashCode()

        { values = storedValues; channelCount = channelCount;
          symbolSetSize = symbolSetSize; vHash = h }

    override this.GetHashCode() = this.vHash

    override this.Equals(obj) =
        match obj with
        | :? sortableIntArray as other ->
            // Check hash first for fast short-circuit
            if this.vHash <> other.vHash then false
            else
                this.channelCount = other.channelCount &&
                ReadOnlySpan<int>(this.values).SequenceEqual(ReadOnlySpan<int>(other.values))
        | _ -> false

    member this.ToSortableBoolArrays() : sortableBoolArray[] =
        if this.ChannelCount <= 1<channelCount> then
            [||]
        else
            let thresholds = [| 0 .. %this.channelCount |]
            let vals = this.values
            let sw = this.ChannelCount
            thresholds 
            |> Array.map (
                fun threshold ->
                    let boolValues = vals |> Array.map (fun v -> v >= threshold )
                    sortableBoolArray.create(boolValues, sw))



    static member createFromPermutation(perm: permutation) =
        sortableIntArray.create(perm.Array, (%perm.Order |> UMX.tag<channelCount>), (%perm.Order |> UMX.tag<symbolSetSize>))

    static member createSorted(channelCount: int<channelCount>) =
        if channelCount < 0<channelCount> then
            invalidArg "channelCount" "Channel count must be non-negative."
        let values = [| 0 .. (%channelCount - 1) |]
        sortableIntArray.create(values, channelCount, %channelCount |> UMX.tag<symbolSetSize>)

    /// Gets a copy of the values array.
    member this.Values with get() = Array.copy this.values

    /// Gets a value without exposing the backing array.
    member this.Item
        with get(index: int) = this.values.[index]
    
    member this.ArrayLength with get() = this.values.Length

    /// Gets the channel count.
    member this.ChannelCount = this.channelCount

    /// Gets the symbol set size.
    member this.SymbolSetSize = this.symbolSetSize

    /// Computes the squared distance between this array and another sortableIntArray.
    member this.DistanceSquared(other: sortableIntArray) =
        if other.channelCount <> this.channelCount then
            invalidArg "other" $"Other array channel count ({int other.channelCount}) must equal this channel count ({int this.channelCount})."
        ArrayUtils.distanceSquared this.values other.values

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayUtils.isSorted this.values
    
    member this.SortByCes
                (ces: ce[])
                (useCounter: int[]) : sortableIntArray =
        let sortedValues = CeSorting.sortByWithUses ces useCounter (Array.copy this.values)
        sortableIntArray.create(sortedValues, this.ChannelCount, this.SymbolSetSize)

    member this.SortByCesWithHistory 
                (ces: ce[])
                (useCounter: int[]) : sortableIntArray[] =
        let history = CeSorting.sortByWithHistoryAndUses ces useCounter this.values
        let sw = this.ChannelCount
        let sss = this.SymbolSetSize
        history |> Array.map (fun values -> sortableIntArray.create(values, sw, sss))


    member this.ToPermutation() : permutation =
        permutation.createUnsafe (Array.copy this.values)

    interface IEnumerable<int> with
        member this.GetEnumerator() =
            (this.values :> seq<int>).GetEnumerator()

    interface IEnumerable with
        member this.GetEnumerator() =
            (this.values :> IEnumerable).GetEnumerator()




module SortableIntArray =
    // Custom comparer for sortableIntArray based only on Values

    type SortableIntArrayValueComparer() =
        interface IEqualityComparer<sortableIntArray> with
            member _.Equals(x, y) =
                // Check hash first: if they differ, they cannot be equal.
                if x.GetHashCode() <> y.GetHashCode() then false
                else
                    x.ChannelCount = y.ChannelCount &&
                    ReadOnlySpan<int>(x.Values).SequenceEqual(ReadOnlySpan<int>(y.Values))
            
            member _.GetHashCode(obj) = obj.GetHashCode()

    /// Efficiently removes duplicate test cases using SIMD-accelerated equality checks.
    let removeDuplicates (arr: sortableIntArray[]) : sortableIntArray[] =
        if arr = null || arr.Length <= 1 then arr
        else
            // Using a HashSet with our optimized comparer is O(n)
            let set = HashSet<sortableIntArray>(SortableIntArrayValueComparer())
            for i = 0 to arr.Length - 1 do
                set.Add(arr.[i]) |> ignore
            
            let result = Array.zeroCreate set.Count
            set.CopyTo(result)
            result


    let getOrbit (maxCount: int) (perm: permutation) : sortableIntArray seq =
        Permutation.powerSequence perm  
        |> CollectionUtils.takeUpToOrWhile maxCount (fun perm -> not (Permutation.isIdentity perm))
        |> Seq.map sortableIntArray.createFromPermutation


    let randomSortableIntArray (indexShuffler: int -> int) (channelCount: int<channelCount>) : sortableIntArray =
        let perm = Permutation.randomPermutation indexShuffler %channelCount
        sortableIntArray.createFromPermutation perm
      

    let getMergeTestCases 
            (channelCount: int<channelCount>)
            (mergeDimension: int<mergeDimension>)
            (mergeFillType: mergeSuffixType)
            : sortableIntArray [] =

        if %channelCount % %mergeDimension <> 0 then
            invalidArg "channelCount" (sprintf "Channel count: %d must be divisible by mergeDimension : %d " %channelCount  %mergeDimension)

        let latticeDimension = %mergeDimension |> UMX.tag<latticeDimension>
        let edgeLength = %channelCount / %mergeDimension |> UMX.tag<latticeDistance>
        let mergeLattice = MergeLattice.create latticeDimension edgeLength
        let sortableIntArrays =
            match mergeFillType with
            | NoSuffix -> 
                MergeLattice.getPermutationsStandard None mergeLattice
                |> LatticePathPermutations.toPermutations
                |> Array.map(sortableIntArray.createFromPermutation)
            | VV_1 ->
                MergeLattice.getPermutationsVV_1 None mergeLattice
                |> LatticePathPermutations.toPermutations
                |> Array.map(sortableIntArray.createFromPermutation)

        sortableIntArrays


    let toString (sia: sortableIntArray) : string =
        let valuesStr = 
            sia.Values 
            |> Array.map string 
            |> String.concat ", "
        sprintf "[%s]" valuesStr
