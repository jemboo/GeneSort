
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type sorterBoolTests =
    { id: Guid<sorterTestIsd>
      sortingWidth: int<sortingWidth>
      sortableBoolArrays: sortableBoolArray[] }

    static member create (id: Guid<sorterTestIsd>) (arrays: sortableBoolArray[]) : sorterBoolTests =
        //if Array.isEmpty arrays then
        //    invalidArg "arrays" "Arrays must not be empty."
        //let arrayType = SortableArray.getSortableArrayType arrays.[0]
        let sortingWidth = arrays.[0].SortingWidth
        //if arrays |> Array.exists (fun arr -> SortableArray.sortingWidth arr <> sortingWidth) then
        //    invalidArg "arrays" "All SortableArrays must have the same SortingWidth."
        //if arrays |> Array.exists (fun arr -> SortableArray.getSortableArrayType arr <> arrayType) then
        //    invalidArg "arrays" "All SortableArrays must have the same type (Ints or Bools)."
        { id = id; sortingWidth = sortingWidth; sortableBoolArrays = Array.copy arrays }

    override this.Equals(obj) =
        match obj with
        | :? sorterBoolTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableBoolArrays other.sortableBoolArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableBoolArrays


    member this.Count with get() = this.sortableBoolArrays.Length

    member this.Id with get() = this.id
    
    member this.SortableArrayType with get() = SortableArrayType.Bools

    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableBoolArrays with get() = this.sortableBoolArrays

    interface IEquatable<sorterBoolTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableBoolArrays other.sortableBoolArrays


module SorterBoolTest = ()
