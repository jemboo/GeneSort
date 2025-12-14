
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type sortableIntTests =
    { id: Guid<sortableTestsId>
      sortingWidth: int<sortingWidth>
      sortableIntArrays: sortableIntArray[] 
      unsortedCount: Lazy<int>
    }

    static member create 
                    (id: Guid<sortableTestsId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableIntArray[]) : sortableIntTests =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableIntArrays = Array.copy arrays 
            unsortedCount = Lazy<int>(fun () -> arrays |> Seq.filter(fun sa -> not sa.IsSorted) |> Seq.length)
        }

    override this.Equals(obj) =
        match obj with
        | :? sortableIntTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableIntArrays

    member this.SortableArrayType with get() = sortableArrayDataType.Ints

    member this.Count with get() = this.sortableIntArrays.Length

    member this.Id with get() = this.id
    
    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableIntArrays with get() = this.sortableIntArrays
    
    member this.UnsortedCount with get() = this.unsortedCount.Value

    interface IEquatable<sortableIntTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays


module SortableIntTest = ()
 