
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

type sortableBoolTestSet =

    { Id: Guid<sortableTestSetId>
      sortableTests: sortableBoolTests[] }

    static member create 
                    (id: Guid<sortableTestSetId>) 
                    (arrays: sortableBoolTests[]) : sortableBoolTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        let sortingWidth = arrays.[0].sortableBoolArrays.[0]
        { Id = id; sortableTests = Array.copy arrays; }

    member this.SortableArrayType with get() = sortableArrayType.Bools

    member this.SortingWidth with get() = this.sortableTests.[0].sortableBoolArrays.[0].SortingWidth


module SorterBoolTestSet = ()


