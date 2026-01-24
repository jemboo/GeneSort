
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting

[<Struct; CustomEquality; NoComparison>]
type sortableBoolTest =
    private { id: Guid<sorterTestId>
              sortingWidth: int<sortingWidth>
              sortableBoolArrays: sortableBoolArray[]
            }

    static member create 
                    (id: Guid<sorterTestId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableBoolArray[]) : sortableBoolTest =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableBoolArrays = Array.copy arrays
        }

    override this.Equals(obj) =
        match obj with
        | :? sortableBoolTest as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableBoolArrays other.sortableBoolArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableBoolArrays


    member this.SoratbleCount with get() = this.sortableBoolArrays.Length  |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.SortableArrayType with get() = sortableDataType.Bools

    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableBoolArrays with get() = this.sortableBoolArrays

    interface IEquatable<sortableBoolTest> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableBoolArrays other.sortableBoolArrays


module SortableBoolTest = ()
