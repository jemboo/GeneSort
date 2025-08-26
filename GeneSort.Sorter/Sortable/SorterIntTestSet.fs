
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

type SorterIntTestSet =
    { Id: Guid<sorterTestSetId>
      sorterTests: SorterIntTest[] }

    static member create 
                    (id: Guid<sorterTestSetId>) 
                    (arrays: SorterIntTest[]) : SorterIntTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        { Id = id; sorterTests = Array.copy arrays; }

    member this.SortableArrayType with get() = SortableArrayType.Ints

    member this.SortingWidth with get() = this.sorterTests.[0].sortableArrays.[0].SortingWidth


module SorterIntTestset = ()


