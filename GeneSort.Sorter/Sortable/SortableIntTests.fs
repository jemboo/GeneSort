
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type sortableIntTests =
    { id: Guid<sortableTestsId>
      sortingWidth: int<sortingWidth>
      sortableIntArrays: sortableIntArray[] }

    static member create (id: Guid<sortableTestsId>) (arrays: sortableIntArray[]) : sortableIntTests =
        //if Array.isEmpty arrays then
        //    invalidArg "arrays" "Arrays must not be empty."
        //let arrayType = SortableArray.getSortableArrayType arrays.[0]
        let sortingWidth = arrays.[0].SortingWidth
        //if arrays |> Array.exists (fun arr -> SortableArray.sortingWidth arr <> sortingWidth) then
        //    invalidArg "arrays" "All SortableArrays must have the same SortingWidth."
        //if arrays |> Array.exists (fun arr -> SortableArray.getSortableArrayType arr <> arrayType) then
        //    invalidArg "arrays" "All SortableArrays must have the same type (Ints or Bools)."
        { id = id; sortingWidth = sortingWidth; sortableIntArrays = Array.copy arrays }

    override this.Equals(obj) =
        match obj with
        | :? sortableIntTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableIntArrays

    member this.SortableArrayType with get() = SortableArrayType.Ints

    member this.Count with get() = this.sortableIntArrays.Length

    member this.Id with get() = this.id
    
    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableIntArrays with get() = this.sortableIntArrays
    
    member this.UnsortedCount with get() = 
                    this.SortableIntArrays 
                    |> Seq.filter(fun sa -> not sa.IsSorted)
                    |> Seq.length

    interface IEquatable<sortableIntTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays


module SortableIntTest = ()
 