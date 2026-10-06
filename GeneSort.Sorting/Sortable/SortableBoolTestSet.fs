
namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting

type sortableBoolTestSet =

    { Id: Guid<sortableTestsSetId>
      sortableTests: sortableBinaryTest[] }

    static member create 
                    (id: Guid<sortableTestsSetId>) 
                    (arrays: sortableBinaryTest[]) : sortableBoolTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        { Id = id; sortableTests = Array.copy arrays; }

    member this.SortableArrayType with get() = sortableDataFormat.BoolArray

    member this.SortingWidth with get() = this.sortableTests.[0].SortableBinaryArrays.[0].SortingWidth


module SorterBoolTestSet = ()


