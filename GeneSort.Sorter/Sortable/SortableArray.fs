
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter

type SortableArrayType = 
    | Ints
    | Bools
 
type SortableArray = 
    | Ints of sortableIntArray
    | Bools of sortableBoolArray


module SortableArray =

    /// Gets the underlying values as an obj array.
    let getSortableArrayType (sortableArray: SortableArray) : SortableArrayType =
        match sortableArray with
        | Ints _ -> SortableArrayType.Ints
        | Bools _ -> SortableArrayType.Bools
    
    /// Creates a SortableArray from a boolean array and sorting width.
    let createBools (values: bool[]) (sortingWidth: int<sortingWidth>) : SortableArray =
        Bools (sortableBoolArray.Create(values, sortingWidth))
    
    /// Creates a SortableArray from an integer array, sorting width, and symbol set size.
    let createInts (values: int[]) (sortingWidth: int<sortingWidth>) (symbolSetSize: int<symbolSetSize>) : SortableArray =
        Ints (sortableIntArray.Create(values, sortingWidth, symbolSetSize))
    
    /// Gets the underlying values as an obj array.
    let values (sortable: SortableArray) : obj[] =
        match sortable with
        | Ints intArray -> intArray.Values |> Array.map (fun x -> x :> obj)
        | Bools boolArray -> boolArray.Values |> Array.map (fun x -> x :> obj)
    
    /// Gets the sorting width of the SortableArray.
    let sortingWidth (sortable: SortableArray) : int<sortingWidth> =
        match sortable with
        | Ints intArray -> intArray.SortingWidth
        | Bools boolArray -> boolArray.SortingWidth
    
    /// Checks if the SortableArray is sorted in non-decreasing order.
    let isSorted (sortable: SortableArray) : bool =
        match sortable with
        | Ints intArray -> intArray.IsSorted
        | Bools boolArray -> boolArray.IsSorted
    
    // sorts one sortable (this) by a sequence of ces, and returns an array
    // tracking how many times each ce was used (useCounter)
    let sortByCes 
            (ces: Ce[]) 
            (startIndex: int)
            (extent: int) 
            (useCounter: int[]) 
            (sortable: SortableArray) : SortableArray =
        match sortable with
        | Ints intArray ->
            let sortedValues = Ce.sortBy ces startIndex extent useCounter intArray.Values
            Ints (sortableIntArray.Create(sortedValues, intArray.SortingWidth, intArray.SymbolSetSize))
        | Bools boolArray ->
            let sortedValues = Ce.sortBy ces startIndex extent useCounter boolArray.Values
            Bools (sortableBoolArray.Create(sortedValues, boolArray.SortingWidth))

    // sorts one sortable (this) by a sequence of ces, and returns an array of the intermediate results (SortableArray[])
    // as well as tracking how many times each ce was used (useCounter)
    let sortByCesWithHistory 
            (ces: Ce[]) 
            (startIndex: int) 
            (extent: int) 
            (useCounter: int[]) 
            (sortable: SortableArray) : SortableArray[] =
        match sortable with
        | Ints intArray ->
            let history = Ce.sortByWithHistory ces startIndex extent useCounter intArray.Values
            history |> Array.map (fun values -> Ints (sortableIntArray.Create(values, intArray.SortingWidth, intArray.SymbolSetSize)))
        | Bools boolArray ->
            let history = Ce.sortByWithHistory ces startIndex extent useCounter boolArray.Values
            history |> Array.map (fun values -> Bools (sortableBoolArray.Create(values, boolArray.SortingWidth)))
    

    /// Computes the squared distance between two SortableArray instances of the same kind.
    /// <exception cref="ArgumentException">Thrown when the arrays are of different kinds or have different widths.</exception>
    let distanceSquared (a: SortableArray) (b: SortableArray) : int =
        match a, b with
        | Ints intA, Ints intB -> intA.DistanceSquared(intB)
        | Bools boolA, Bools boolB -> boolA.DistanceSquared(boolB)
        | _ -> invalidArg "b" "Both SortableArray instances must be of the same kind (Ints or Bools)."
    
    /// Returns all possible SortableArray instances (Bools case) for a given sorting width.
    /// <exception cref="ArgumentException">Thrown when sortingWidth is negative.</exception>
    let getAllBools (sortingWidth: int<sortingWidth>) : SortableArray[] =
        SortableBoolArray.getAllSortableBoolArrays sortingWidth |> Array.map Bools
