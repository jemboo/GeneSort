
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

type sorterBoolTestSet =

    { Id: Guid<sorterTestSetId>
      sorterTests: sorterBoolTests[] }

    static member create 
                    (id: Guid<sorterTestSetId>) 
                    (arrays: sorterBoolTests[]) : sorterBoolTestSet =
        if Array.isEmpty arrays then
            invalidArg "arrays" "Arrays must not be empty."
        let sortingWidth = arrays.[0].sortableArrays.[0]
        { Id = id; sorterTests = Array.copy arrays; }

    member this.SortableArrayType with get() = SortableArrayType.Bools

    member this.SortingWidth with get() = this.sorterTests.[0].sortableArrays.[0].SortingWidth


module SorterBoolTestSet = ()


