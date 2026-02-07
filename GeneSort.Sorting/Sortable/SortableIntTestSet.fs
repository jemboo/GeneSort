
namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting

type sortableIntTestSet =
    { Id: Guid<sortableTestSetId>
      sortableTests: sortableIntTest[] }

    static member create 
                    (id: Guid<sortableTestSetId>) 
                    (arrays: sortableIntTest[]) : sortableIntTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        { Id = id; sortableTests = Array.copy arrays; }

    member this.Count with get() = this.sortableTests.Length

    member this.SortableArrayType with get() = sortableDataFormat.IntArray

    member this.SortingWidth with get() = this.sortableTests.[0].sortableIntArrays.[0].SortingWidth


module SorterIntTestset = ()


