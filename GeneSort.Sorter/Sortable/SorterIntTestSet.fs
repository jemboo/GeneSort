
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

type sorterIntTestSet =
    { Id: Guid<sorterTestSetId>
      sorterTests: sorterIntTest[] }

    static member create 
                    (id: Guid<sorterTestSetId>) 
                    (arrays: sorterIntTest[]) : sorterIntTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        { Id = id; sorterTests = Array.copy arrays; }

    member this.Count with get() = this.sorterTests.Length

    member this.SortableArrayType with get() = SortableArrayType.Ints

    member this.SortingWidth with get() = this.sorterTests.[0].sortableArrays.[0].SortingWidth


module SorterIntTestset = ()


