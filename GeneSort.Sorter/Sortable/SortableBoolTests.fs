
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type sortableBoolTests =
    { id: Guid<sortableTestsId>
      sortingWidth: int<sortingWidth>
      sortableBoolArrays: sortableBoolArray[]
      unsortedCount: Lazy<int>
    }

    static member create 
                    (id: Guid<sortableTestsId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableBoolArray[]) : sortableBoolTests =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableBoolArrays = Array.copy arrays
            unsortedCount = Lazy<int>(fun () -> arrays |> Seq.filter(fun sa -> not sa.IsSorted) |> Seq.length)
        }

    override this.Equals(obj) =
        match obj with
        | :? sortableBoolTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableBoolArrays other.sortableBoolArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableBoolArrays


    member this.SoratbleCount with get() = this.sortableBoolArrays.Length  |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.SortableArrayType with get() = sortableDataType.Bools

    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableBoolArrays with get() = this.sortableBoolArrays

    member this.UnsortedCount with get() = this.unsortedCount.Value |> UMX.tag<sortableCount>

    interface IEquatable<sortableBoolTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableBoolArrays other.sortableBoolArrays


module SortableBoolTest = ()
