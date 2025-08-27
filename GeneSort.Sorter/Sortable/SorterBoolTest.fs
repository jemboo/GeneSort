
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type sorterBoolTest =
    { id: Guid<sorterTestId>
      sortingWidth: int<sortingWidth>
      sortableArrays: sortableBoolArray[] }

    static member create (id: Guid<sorterTestId>) (arrays: sortableBoolArray[]) : sorterBoolTest =
        //if Array.isEmpty arrays then
        //    invalidArg "arrays" "Arrays must not be empty."
        //let arrayType = SortableArray.getSortableArrayType arrays.[0]
        let sortingWidth = arrays.[0].SortingWidth
        //if arrays |> Array.exists (fun arr -> SortableArray.sortingWidth arr <> sortingWidth) then
        //    invalidArg "arrays" "All SortableArrays must have the same SortingWidth."
        //if arrays |> Array.exists (fun arr -> SortableArray.getSortableArrayType arr <> arrayType) then
        //    invalidArg "arrays" "All SortableArrays must have the same type (Ints or Bools)."
        { id = id; sortingWidth = sortingWidth; sortableArrays = Array.copy arrays }

    override this.Equals(obj) =
        match obj with
        | :? sorterBoolTest as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableArrays other.sortableArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableArrays


    member this.Count with get() = this.sortableArrays.Length

    member this.Id with get() = this.id
    
    member this.SortableArrayType with get() = SortableArrayType.Bools

    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableArrays with get() : sortableBoolArray[] = Array.copy this.sortableArrays

    interface IEquatable<sorterBoolTest> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableArrays other.sortableArrays


module SorterBoolTest = ()
