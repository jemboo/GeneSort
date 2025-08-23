
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

[<Struct; CustomEquality; NoComparison>]
type SorterTestSet =
    { Id: Guid<sorterTestSetId>
      sorterTests: SorterTest[] }

    static member create 
                    (id: Guid<sorterTestSetId>) 
                    (arrays: SorterTest[]) : SorterTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        let sortingWidth = SortableArray.sortingWidth arrays.[0].sortableArrays.[0]
        { Id = id; sorterTests = Array.copy arrays; }

    override this.Equals(obj) =
        match obj with
        | :? SorterTestSet as other ->
            this.Id = other.Id
        | _ -> false

    override this.GetHashCode() =
        hash (this.Id, hash this.sorterTests)

    member this.SortableArrayType with get() = SortableArray.getSortableArrayType (this.sorterTests.[0]).sortableArrays.[0]

    member this.SortingWidth with get() = SortableArray.sortingWidth (this.sorterTests.[0]).sortableArrays.[0]

    interface IEquatable<SorterTestSet> with
        member this.Equals(other) =
            this.Id = other.Id
module SorterTestset = ()


