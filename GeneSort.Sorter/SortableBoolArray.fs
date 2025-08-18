namespace GeneSort.Sorter

open System
open FSharp.UMX
open GeneSort.Core


type sortableBoolArray =
    private { 
        values: bool[] 
        order: int<sortingWidth> 
    }
    /// Creates a sortableBoolArray, throwing if values length does not match order.
    /// <exception cref="ArgumentException">Thrown when order is negative or values length does not equal order.</exception>
    static member Create(values: bool[], order: int<sortingWidth>) =
        if order < 0<sortingWidth> then
            invalidArg "order" "Order must be non-negative."
        if values.Length <> int order then
            invalidArg "values" $"Values length ({values.Length}) must equal order ({int order})."
        { values = Array.copy values; order = order }

    /// Gets the values array.
    member this.Values = this.values

    /// Gets the sorting order.
    member this.Order = this.order

    /// Computes the squared distance between this array and another sortableBoolArray, treating true as 1 and false as 0.
    /// <exception cref="ArgumentException">Thrown when the other array's order does not match this array's order.</exception>
    member this.DistanceSquared(other: sortableBoolArray) =
        if other.order <> this.order then
            invalidArg "other" $"Other array order ({int other.order}) must equal this order ({int this.order})."
        let thisNumeric = this.values |> Array.map (fun b -> if b then 1 else 0)
        let otherNumeric = other.values |> Array.map (fun b -> if b then 1 else 0)
        ArrayProperties.distanceSquared thisNumeric otherNumeric

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayProperties.isSorted this.values

    /// Returns a new sortableBoolArray with values sorted according to the comparison-exchange operations.
    /// <exception cref="ArgumentException">Thrown when ces contains indices out of bounds.</exception>
    member this.SortBy(ces: Ce[]) =
        let sortedValues =  Ce.sortBy ces this.values
        sortableBoolArray.Create(sortedValues, this.order)

    /// Returns an array of sortableIntArray, starting with the original array and followed by arrays after each comparison-exchange.
    /// <exception cref="ArgumentException">Thrown when ces contains indices out of bounds.</exception>
    member this.SortByWithHistory(ces: Ce[]) =
        let history = Ce.sortByWithHistory ces this.values
        history |> Array.map (fun values -> sortableBoolArray.Create(values, this.order))



module SortableBoolArray =

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




//module SortableBoolArray =

//    let makeAllBits (order: int<sortingWidth>) =
//        let symbolSetSize = 2uL |> UMX.tag<symbolSetSize>
//        let bitShift = order |> UMX.untag

//        { 0uL .. (1uL <<< bitShift) - 1uL }
//        |> Seq.map (ByteUtils.uint64ToBoolArray order)
//        |> Seq.map (fun arr ->
//            { sortableBoolArray.values = arr
//              order = order })


//    let makeAllForOrder (order: int<sortingWidth>) =
//        let bitShift = order |> UMX.untag

//        { 0uL .. (1uL <<< bitShift) - 1uL }
//        |> Seq.map (ByteUtils.uint64ToBoolArray order)
//        |> Seq.map (fun arr ->
//            { sortableBoolArray.values = arr
//              order = order })


//    let makeSortedStacks (orderStack: int<sortingWidth>[]) =
//        let stackedOrder = Order.add orderStack

//        CollectionOps.stackSortedBlocks orderStack
//        |> Seq.map (fun arr ->
//            { sortableBoolArray.values = arr
//              order = stackedOrder })


//    let makeRandomBits 
//            (order: int<sortingWidth>) 
//            (pctTrue: float) 
//            (randy: IRando) 
//        =
//        let arrayLength = order |> UMX.untag

//        Seq.initInfinite (fun _ ->
//            { sortableBoolArray.values = RandVars.randBits pctTrue randy arrayLength |> Seq.toArray
//              order = order })


//    let allBooleanVersions (sortableInts: sortableIntArray) =
//        let order = sortableInts |> SortableIntArray.getOrder |> UMX.untag
//        let values = sortableInts |> SortableIntArray.getValues
//        let threshHolds = values |> Set.ofArray |> Set.toArray |> Array.sort |> Array.skip 1

//        threshHolds
//        |> Seq.map (fun thresh -> Array.init order (fun dex -> if (values.[dex] >= thresh) then true else false))



//    let expandToSortableBits (sortableIntsSeq: seq<sortableIntArray>) =
//        let order = sortableIntsSeq |> Seq.head |> SortableIntArray.getOrder

//        sortableIntsSeq
//        |> Seq.map (allBooleanVersions)
//        |> Seq.concat
//        |> Seq.distinct
//        |> Seq.map (make order)
