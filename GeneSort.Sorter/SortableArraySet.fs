
namespace GeneSort.Sorter

open System
open FSharp.UMX
open GeneSort.Core

[<Struct; CustomEquality; NoComparison>]
type SortableArraySet =
    { Id: Guid<sortableSetId>
      sortingWidth: int<sortingWidth>
      srtableArrays: SortableArray[] }

    static member create (id: Guid<sortableSetId>) (arrays: SortableArray[]) : SortableArraySet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        let sortingWidth = SortableArray.sortingWidth arrays.[0]
        let arrayType = SortableArray.getSortableArrayType arrays.[0]
        if arrays |> Array.exists (fun arr -> SortableArray.sortingWidth arr <> sortingWidth) then
            invalidArg "arrays" "All SortableArrays must have the same SortingWidth."
        if arrays |> Array.exists (fun arr -> SortableArray.getSortableArrayType arr <> arrayType) then
            invalidArg "arrays" "All SortableArrays must have the same type (Ints or Bools)."
        { Id = id; sortingWidth = sortingWidth; srtableArrays = Array.copy arrays }

    override this.Equals(obj) =
        match obj with
        | :? SortableArraySet as other ->
            this.Id = other.Id && Array.forall2 (=) this.srtableArrays other.srtableArrays
        | _ -> false

    override this.GetHashCode() =
        hash (this.Id, hash this.srtableArrays)

    member this.SortableArrayType with get() = SortableArray.getSortableArrayType this.srtableArrays.[0]

    interface IEquatable<SortableArraySet> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.srtableArrays other.srtableArrays


module SortableArraySet = ()
