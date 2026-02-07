
namespace GeneSort.Component.Sortable

open System
open FSharp.UMX
open GeneSort.Component

[<Struct; CustomEquality; NoComparison>]
type sortableIntTest =
    private { id: Guid<sorterTestId>
              sortingWidth: int<sortingWidth>
              sortableIntArrays: sortableIntArray[]
            }

    static member create 
                    (id: Guid<sorterTestId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableIntArray[]) : sortableIntTest =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableIntArrays = Array.copy arrays
        }

    static member Empty =
        let id = Guid.NewGuid() |> UMX.tag<sorterTestId>
        sortableIntTest.create id 0<sortingWidth> [||]

    override this.Equals(obj) =
        match obj with
        | :? sortableIntTest as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableIntArrays

    member this.SortableDataFormat with get() = sortableDataFormat.IntArray

    member this.SoratbleCount with get() = this.sortableIntArrays.Length |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableIntArrays with get() = this.sortableIntArrays

    interface IEquatable<sortableIntTest> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays


module SortableIntTest = ()
 