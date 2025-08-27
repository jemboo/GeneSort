
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter

[<Struct; CustomEquality; NoComparison>]
type sorterIntTests =
    { id: Guid<sorterTestId>
      sortingWidth: int<sortingWidth>
      sortableArrays: sortableIntArray[] }

    static member create (id: Guid<sorterTestId>) (arrays: sortableIntArray[]) : sorterIntTests =
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
        | :? sorterIntTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableArrays other.sortableArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableArrays

    member this.SortableArrayType with get() = SortableArrayType.Ints

    member this.Count with get() = this.sortableArrays.Length

    member this.Id with get() = this.id
    
    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableArrays with get() : sortableIntArray[] = Array.copy this.sortableArrays
     
    //member this.ApplyCes 
    //                (ces: Ce[])                 
    //                (useCounter: int[]) 
    //                (useCounterOffset: int) =
    //    let newArrays =  Array.copy this.sortableArrays
    //    sorterIntTest.create (Guid.NewGuid()) newArrays 

    interface IEquatable<sorterIntTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableArrays other.sortableArrays


module SorterIntTest = ()
 