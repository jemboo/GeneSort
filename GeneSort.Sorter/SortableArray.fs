
namespace GeneSort.Sorter

open System
open FSharp.UMX
open GeneSort.Core

 
type SortableArray = 
    | Ints of sortableIntArray
    | Bools of sortableBoolArray


module SortableArray =
    
    /// Creates a SortableArray from a boolean array and sorting width.
    let createBools (values: bool[]) (sortingWidth: int<sortingWidth>) : SortableArray =
        Bools (sortableBoolArray.Create(values, sortingWidth))
    
    /// Creates a SortableArray from an integer array, sorting width, and symbol set size.
    let createInts (values: int[]) (sortingWidth: int<sortingWidth>) (symbolSetSize: uint64<symbolSetSize>) : SortableArray =
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
    
    /// Returns a new SortableArray with values sorted according to the comparison-exchange operations.
    /// <exception cref="ArgumentException">Thrown when ces contains indices out of bounds.</exception>
    let sortBy (ces: Ce[]) (sortable: SortableArray) : SortableArray =
        match sortable with
        | Ints intArray ->
            let sortedValues = Ce.sortBy ces intArray.Values
            Ints (sortableIntArray.Create(sortedValues, intArray.SortingWidth, intArray.SymbolSetSize))
        | Bools boolArray ->
            let sortedValues = Ce.sortBy ces boolArray.Values
            Bools (sortableBoolArray.Create(sortedValues, boolArray.SortingWidth))
    
    /// Returns an array of SortableArray, starting with the original array and followed by arrays after each cumulative comparison-exchange.
    /// <exception cref="ArgumentException">Thrown when ces contains indices out of bounds.</exception>
    let sortByWithHistory (ces: Ce[]) (sortable: SortableArray) : SortableArray[] =
        match sortable with
        | Ints intArray ->
            let history = Ce.sortByWithHistory ces intArray.Values
            history |> Array.map (fun values -> Ints (sortableIntArray.Create(values, intArray.SortingWidth, intArray.SymbolSetSize)))
        | Bools boolArray ->
            let history = Ce.sortByWithHistory ces boolArray.Values
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