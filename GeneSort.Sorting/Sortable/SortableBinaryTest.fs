
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting

[<Struct; CustomEquality; NoComparison>]
type sortableBinaryTest =
    private { id: Guid<sorterTestId>
              sortingWidth: int<sortingWidth>
              sortableBinaryArrays: sortableBoolArray[]
            }

    static member create 
                    (id: Guid<sorterTestId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableBoolArray[]) : sortableBinaryTest =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableBinaryArrays = Array.copy arrays
        }

    override this.Equals(obj) =
        match obj with
        | :? sortableBinaryTest as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableBinaryArrays other.sortableBinaryArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableBinaryArrays


    member this.SoratbleCount with get() = this.sortableBinaryArrays.Length  |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.SortableArrayType with get() = sortableDataFormat.BoolArray

    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableBinaryArrays with get() = this.sortableBinaryArrays

    interface IEquatable<sortableBinaryTest> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableBinaryArrays other.sortableBinaryArrays


module SortableBoolTest = ()
