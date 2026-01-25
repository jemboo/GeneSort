
namespace GeneSort.Sorting.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sortableBoolTestSet =

    { Id: Guid<sortableTestSetId>
      sortableTests: sortableBinaryTest[] }

    static member create 
                    (id: Guid<sortableTestSetId>) 
                    (arrays: sortableBinaryTest[]) : sortableBoolTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        { Id = id; sortableTests = Array.copy arrays; }

    member this.SortableArrayType with get() = sortableDataFormat.BoolArray

    member this.SortingWidth with get() = this.sortableTests.[0].SortableBinaryArrays.[0].SortingWidth


module SorterBoolTestSet = ()


