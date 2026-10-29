
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting

[<Struct; CustomEquality; NoComparison>]
type sortableIntTests =
    private { id: Guid<sortableTestsId>
              channelCount: int<channelCount>
              sortableIntArrays: sortableIntArray[]
            }

    static member create 
                    (id: Guid<sortableTestsId>) 
                    (channelCount:int<channelCount>)
                    (arrays: sortableIntArray[]) : sortableIntTests =
        { 
            id = id; 
            channelCount = channelCount;
            sortableIntArrays = Array.copy arrays
        }

    static member Empty =
        let id = Guid.NewGuid() |> UMX.tag<sortableTestsId>
        sortableIntTests.create id 0<channelCount> [||]

    override this.Equals(obj) =
        match obj with
        | :? sortableIntTests as other ->
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays
        | _ -> false

    override this.GetHashCode() =
        hash this.sortableIntArrays

    member this.SortableDataFormat with get() = sortableDataFormat.IntArray

    member this.SortableCount with get() = this.sortableIntArrays.Length |> UMX.tag<sortableCount>

    member this.Id with get() = this.id
    
    member this.ChannelCount with get() = this.channelCount

    member this.SortableIntArrays with get() = this.sortableIntArrays

    interface IEquatable<sortableIntTests> with
        member this.Equals(other) =
            this.Id = other.Id && Array.forall2 (=) this.sortableIntArrays other.sortableIntArrays


module SortableIntTests = ()
 