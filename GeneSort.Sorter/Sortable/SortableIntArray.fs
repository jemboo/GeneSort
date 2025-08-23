
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter


[<Struct; CustomEquality; NoComparison>]
type sortableIntArray =
    private { 
        values: int[] 
        sortingWidth: int<sortingWidth> 
        symbolSetSize: int<symbolSetSize> 
    }

    static member Create(values: int[], sortingWidth: int<sortingWidth>, symbolSetSize: int<symbolSetSize>) =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        if values.Length <> int sortingWidth then
            invalidArg "values" $"Values length ({values.Length}) must equal sorting width ({int sortingWidth})."
        if %symbolSetSize <= 0 then
            invalidArg "symbolSetSize" "Symbol set size must be positive."
        if values |> Array.exists (fun v -> v < 0 || uint64 v >= uint64 %symbolSetSize) then
            invalidArg "values" $"All values must be in [0, {uint64 %symbolSetSize})."
        { values = Array.copy values; sortingWidth = sortingWidth; symbolSetSize = symbolSetSize }

    static member CreateSorted(sortingWidth: int<sortingWidth>) =
        if sortingWidth < 0<sortingWidth> then
            invalidArg "sortingWidth" "Sorting width must be non-negative."
        let values = [| 0 .. (%sortingWidth - 1) |]
        { values = values; sortingWidth = sortingWidth; symbolSetSize = %sortingWidth |> UMX.tag<symbolSetSize> }

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

    member this.SortByCes(ces: Ce[]) (startIndex: int) (extent: int) (useCounter: int[]) =
        let sortedValues = Ce.sortBy ces startIndex extent useCounter this.values
        sortableIntArray.Create(sortedValues, this.SortingWidth, this.symbolSetSize)

    member this.SortByCesWithHistory(ces: Ce[]) (startIndex: int) (extent: int) (useCounter: int[]) =
        let history = Ce.sortByWithHistory ces startIndex extent useCounter this.values
        let sw = this.SortingWidth
        let sss = this.SymbolSetSize
        history |> Array.map (fun values -> sortableIntArray.Create(values, sw, sss))

    member this.ToSortableBoolArrays() : sortableBoolArray[] =
        if this.SortingWidth <= 1<sortingWidth> then
            [||]
        else
            let minValue = Array.min this.values
            let thresholds = 
                this.values 
                |> Array.filter (fun v -> v > minValue) 
                |> Array.distinct
                |> Array.sort

            let vals = this.Values
            let sw = this.SortingWidth
            thresholds 
            |> Array.map (
                fun threshold ->
                    let boolValues = vals |> Array.map (fun v -> v >= threshold)
                    sortableBoolArray.Create(boolValues, sw))


    override this.Equals(obj) =
        match obj with
        | :? sortableIntArray as other ->
            this.sortingWidth = other.sortingWidth &&
            this.symbolSetSize = other.symbolSetSize &&
            Array.forall2 (=) this.values other.values
        | _ -> false

    override this.GetHashCode() =
        hash (this.sortingWidth, this.symbolSetSize, hash this.values)

    interface IEquatable<sortableIntArray> with
        member this.Equals(other) =
            this.sortingWidth = other.sortingWidth &&
            this.symbolSetSize = other.symbolSetSize &&
            Array.forall2 (=) this.values other.values



module SortableIntArray =
    
    let fromPermutation (perm:Permutation) : sortableIntArray =
        sortableIntArray.Create(
                perm.Array, 
                (%perm.Order |> UMX.tag<sortingWidth>), 
                (%perm.Order |> UMX.tag<symbolSetSize>))


    let getOrbit (maxCount:int) (perm:Permutation) 
                    :sortableIntArray seq =
        Permutation.powerSequence perm  
        |> CollectionUtils.takeUpToOrWhile maxCount (fun perm -> Permutation.isIdentity perm)
        |> Seq.map(fun perm -> fromPermutation perm)



    // returns a random Permutation by shuffling the identity Permutation
    let randomSortableIntArray 
            (indexShuffler: int -> int) 
            (sortingWidth: int<sortingWidth>) 
            : sortableIntArray =
        let perm = Permutation.randomPermutation indexShuffler %sortingWidth
        fromPermutation perm


    let getIntArrayMergeCases (sortingWidth:int<sortingWidth>) =
        let halfWidth = %sortingWidth / 2
        [|
            for i = 0 to (halfWidth - 1) do
                let arrayData = 
                    [| (%sortingWidth - i) .. (%sortingWidth - 1) |] 
                    |> Array.append
                        [| 0  .. (halfWidth - 1 - i) |] 
                        |> Array.append
                            [| (halfWidth - i) .. (%sortingWidth - 1 - i) |]

                sortableIntArray.Create(arrayData, sortingWidth, (%sortingWidth |>  UMX.tag<symbolSetSize>))
        |]