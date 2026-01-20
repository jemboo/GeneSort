
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type sortableIntTest =
    { id: Guid<sortableTestsId>
      sortingWidth: int<sortingWidth>
      sortableIntArrays: sortableIntArray[] 
      unsortedCount: Lazy<int>
    }

    static member create 
                    (id: Guid<sortableTestsId>) 
                    (sortingWidth:int<sortingWidth>)
                    (arrays: sortableIntArray[]) : sortableIntTest =
        { 
            id = id; 
            sortingWidth = sortingWidth; 
            sortableIntArrays = Array.copy arrays 
            unsortedCount = Lazy<int>(fun () -> arrays |> Seq.filter(fun sa -> not sa.IsSorted) |> Seq.length)
        }

    static member Empty =
        let id = Guid.NewGuid() |> UMX.tag<sortableTestsId>
        sortableIntTest.create id 0<sortingWidth> [||]

    override this.Equals(obj) =
        match obj with
        | :? sortableIntTest as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableIntArrays

    member this.SortableArrayType with get() = sortableDataType.Ints

    member this.SoratbleCount with get() = this.sortableIntArrays.Length |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.SortingWidth with get() = this.sortingWidth

    member this.SortableIntArrays with get() = this.sortableIntArrays
    
    member this.UnsortedCount with get() = this.unsortedCount.Value |> UMX.tag<sortableCount>

    interface IEquatable<sortableIntTest> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays


module SortableIntTest = ()
 