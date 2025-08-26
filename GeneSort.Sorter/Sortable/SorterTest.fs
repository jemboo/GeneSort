
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

//[<Struct; CustomEquality; NoComparison>]
type SorterTest =
    { id: Guid<sorterTestId>
      sortingWidth: int<sortingWidth>
      sortableArrays: SortableArray[] }

    static member create (id: Guid<sorterTestId>) (arrays: SortableArray[]) : SorterTest =
        //if Array.isEmpty arrays then
        //    invalidArg "arrays" "Arrays must not be empty."
        //let arrayType = SortableArray.getSortableArrayType arrays.[0]
        let sortingWidth = SortableArray.sortingWidth arrays.[0]
        //if arrays |> Array.exists (fun arr -> SortableArray.sortingWidth arr <> sortingWidth) then
        //    invalidArg "arrays" "All SortableArrays must have the same SortingWidth."
        //if arrays |> Array.exists (fun arr -> SortableArray.getSortableArrayType arr <> arrayType) then
        //    invalidArg "arrays" "All SortableArrays must have the same type (Ints or Bools)."
        { id = id; sortingWidth = sortingWidth; sortableArrays = Array.copy arrays }


    member this.SortableArrayType with get() = SortableArray.getSortableArrayType this.sortableArrays.[0]

    member this.Count with get() = this.sortableArrays.Length

    member this.Id with get() = this.id

module SorterTest = ()
