
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting

[<Struct; CustomEquality; NoComparison>]
type sortableBinaryTests =
    private { id: Guid<sortableTestsId>
              sortingWidth: int<sortingWidth>
              sortableBinaryArrays: sortableBoolArray[]
            }

    static member create 
                    (id: Guid<sortableTestsId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableBoolArray[]) : sortableBinaryTests =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableBinaryArrays = Array.copy arrays
        }

    static member Empty =
        let id = Guid.NewGuid() |> UMX.tag<sortableTestsId>
        sortableBinaryTests.create id 0<sortingWidth> [||]

    override this.Equals(obj) =
        match obj with
        | :? sortableBinaryTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableBinaryArrays other.sortableBinaryArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableBinaryArrays


    member this.SortableCount with get() = this.sortableBinaryArrays.Length  |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.SortableArrayType with get() = sortableDataFormat.BoolArray

    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableBinaryArrays with get() = this.sortableBinaryArrays

    interface IEquatable<sortableBinaryTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableBinaryArrays other.sortableBinaryArrays


module SortableBoolTests = ()
