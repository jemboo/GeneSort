
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type SorterTest =
    { Id: Guid<sorterTestId>
      sortingWidth: int<sortingWidth>
      sortableArrays: SortableArray[] }

    static member create (id: Guid<sorterTestId>) (arrays: SortableArray[]) : SorterTest =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        let sortingWidth = SortableArray.sortingWidth arrays.[0]
        let arrayType = SortableArray.getSortableArrayType arrays.[0]
        if arrays |> Array.exists (fun arr -> SortableArray.sortingWidth arr <> sortingWidth) then
            invalidArg "arrays" "All SortableArrays must have the same SortingWidth."
        if arrays |> Array.exists (fun arr -> SortableArray.getSortableArrayType arr <> arrayType) then
            invalidArg "arrays" "All SortableArrays must have the same type (Ints or Bools)."
        { Id = id; sortingWidth = sortingWidth; sortableArrays = Array.copy arrays }

    override this.Equals(obj) =
        match obj with
        | :? SorterTest as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableArrays other.sortableArrays
        | _ -> false

    override this.GetHashCode() =
        hash (this.Id, hash this.sortableArrays)

    member this.SortableArrayType with get() = SortableArray.getSortableArrayType this.sortableArrays.[0]

    interface IEquatable<SorterTest> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableArrays other.sortableArrays


module SortableArraySet = ()
