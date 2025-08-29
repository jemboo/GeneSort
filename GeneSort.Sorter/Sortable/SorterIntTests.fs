
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter

[<Struct; CustomEquality; NoComparison>]
type sorterIntTests =
    { id: Guid<sorterTestIsd>
      sortingWidth: int<sortingWidth>
      sortableIntArrays: sortableIntArray[] }

    static member create (id: Guid<sorterTestIsd>) (arrays: sortableIntArray[]) : sorterIntTests =
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
        | :? sorterIntTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableIntArrays

    member this.SortableArrayType with get() = SortableArrayType.Ints

    member this.Count with get() = this.sortableIntArrays.Length

    member this.Id with get() = this.id
    
    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableIntArrays with get() = this.sortableIntArrays
     
    member this.ApplyCes 
                    (removeDuplicates: bool)
                    (ces: ce[]) 
                    (useCounter: int[])    : sorterIntTests =
          let newArrays =
                this.sortableIntArrays 
                |> Array.map(fun sia -> sia.SortByCes ces useCounter )
                |> (fun arr -> if removeDuplicates then SortableIntArray.removeDuplicates arr else arr)

          sorterIntTests.create (Guid.NewGuid() |> UMX.tag<sorterTestIsd>) newArrays 

     
    interface IEquatable<sorterIntTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays


module SorterIntTest = ()
 