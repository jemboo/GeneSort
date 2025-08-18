namespace GeneSort.Sorter

open System
open FSharp.UMX
open GeneSort.Core


/// A record representing an integer array with a specified sorting width and symbol set size.
type sortableIntArray =
    private { 
        values: int[] 
        sortingWidth: int<sortingWidth> 
        symbolSetSize: uint64<symbolSetSize> 
    }

    static member Create(values: int[], sortingWidth: int<sortingWidth>, symbolSetSize: uint64<symbolSetSize>) =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        if values.Length <> int sortingWidth then
            invalidArg "values" $"Values length ({values.Length}) must equal sorting width ({int sortingWidth})."
        if %symbolSetSize <= 0UL then
            invalidArg "symbolSetSize" "Symbol set size must be positive."
        if values |> Array.exists (fun v -> v < 0 || uint64 v >= uint64 %symbolSetSize) then
            invalidArg "values" $"All values must be in [0, {uint64 %symbolSetSize})."
        { values = Array.copy values; sortingWidth = sortingWidth; symbolSetSize = symbolSetSize }

    /// Gets the values array.
    member this.Values = this.values

    /// Gets the sorting width.
    member this.SortingWidth = this.sortingWidth

    /// Gets the symbol set size.
    member this.SymbolSetSize = this.symbolSetSize

    /// Computes the squared distance between this array and another sortableIntArray.
    /// <exception cref="ArgumentException">Thrown when the other array's sortingWidth does not match this array's sortingWidth.</exception>
    member this.DistanceSquared(other: sortableIntArray) =
        if other.sortingWidth <> this.sortingWidth then
            invalidArg "other" $"Other array sorting width ({int other.sortingWidth}) must equal this sorting width ({int this.sortingWidth})."
        ArrayProperties.distanceSquared this.values other.values

    /// Checks if the values array is sorted in non-decreasing order.
    member this.IsSorted = ArrayProperties.isSorted this.values

    /// Returns a new sortableIntArray with values sorted according to the comparison-exchange operations.
    /// <exception cref="ArgumentException">Thrown when ces contains indices out of bounds.</exception>
    member this.SortBy(ces: Ce[]) =
        let sortedValues = Ce.sortBy ces this.values
        sortableIntArray.Create(sortedValues, this.sortingWidth, this.symbolSetSize)

    /// Returns an array of sortableIntArray, starting with the original array and followed by arrays after each comparison-exchange.
    /// <exception cref="ArgumentException">Thrown when ces contains indices out of bounds.</exception>
    member this.SortByWithHistory(ces: Ce[]) =
        let history = Ce.sortByWithHistory ces this.values
        history |> Array.map (fun values -> sortableIntArray.Create(values, this.sortingWidth, this.symbolSetSize))



































//module SortableIntArray =

//    let Identity (order: int<sortingWidth>) (symbolCount: uint64<symbolSetSize>) =
//        let ordV = (order |> UMX.untag)
//        let sc = symbolCount |> UMX.untag |> int

//        if (ordV <> sc) then
//            sprintf "order %d and symbolcount %d don't match (*83)" ordV sc |> Error
//        else
//            { sortableIntArray.values = [| 0 .. ordV - 1 |]
//              sortingWidth = order
//              symbolSetSize = symbolCount }
//            |> Ok


//    let makeOrbits (maxCount: int<sortableCount> option) (perm: permutation) =
//        let order = perm |> Permutation.getOrder
//        let symbolSetSize = order |> UMX.untag |> uint64 |> UMX.tag<symbolSetSize>
//        let intOpt = maxCount |> Option.map UMX.untag

//        Permutation.powers intOpt perm
//        |> Seq.map (Permutation.getArray)
//        |> Seq.map (fun arr ->
//            { sortableIntArray.values = arr
//              sortingWidth = order
//              symbolSetSize = symbolSetSize })


//    // test set for the merge sort (merge two sorted sets of order/2)
//    let makeMergeSortTestWithInts (order: int<order>) =
//        let hov = (order |> UMX.untag ) / 2
//        let symbolSetSize = order |> UMX.untag |> uint64 |> UMX.tag<symbolSetSize>
//        [|0 .. hov|]
//        |> Array.map (
//            fun dex ->
//                { sortableIntArray.values = Permutation.stackedSource order dex 
//                  sortingWidth = order
//                  symbolSetSize = symbolSetSize }
//            )


//    let makeRandomPermutation (order: int<sortingWidth>) (randy: IRando) =
//        let symbolSetSize = order |> UMX.untag |> uint64 |> UMX.tag<symbolSetSize>

//        { sortableIntArray.values = RandVars.randomPermutation randy order
//          sortingWidth = order
//          symbolSetSize = symbolSetSize }


//    let makeRandomSymbol (order: int<sortingWidth>) (symbolSetSize: uint64<symbolSetSize>) (randy: IRando) =
//        let arrayLength = order |> UMX.untag
//        let intArray = RandVars.randSymbols symbolSetSize randy arrayLength |> Seq.toArray

//        { sortableIntArray.values = intArray
//          sortingWidth = order
//          symbolSetSize = symbolSetSize }


//    let makeRandomSymbols 
//                (order: int<sortingWidth>) 
//                (symbolSetSize: uint64<symbolSetSize>) 
//                (rnd: IRando) 
//        =
//        seq {
//            while true do
//                yield makeRandomSymbol order symbolSetSize rnd
//        }


