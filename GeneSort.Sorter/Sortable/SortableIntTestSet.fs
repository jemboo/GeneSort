
namespace GeneSort.Sorter.Sortable

open FSharp.UMX
open GeneSort.Sorter

type sortableIntTestSet =
    { Id: Guid<sortableTestSetId>
      sortableTests: sortableIntTests[] }

    static member create 
                    (id: Guid<sortableTestSetId>) 
                    (arrays: sortableIntTests[]) : sortableIntTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        { Id = id; sortableTests = Array.copy arrays; }

    member this.Count with get() = this.sortableTests.Length

    member this.SortableArrayType with get() = sortableDataType.Ints

    member this.SortingWidth with get() = this.sortableTests.[0].sortableIntArrays.[0].SortingWidth


module SorterIntTestset = ()


