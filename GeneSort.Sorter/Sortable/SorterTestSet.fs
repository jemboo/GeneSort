
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter


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


    member this.SortableArrayType with get() = SortableArray.getSortableArrayType (this.sorterTests.[0]).sortableArrays.[0]

    member this.SortingWidth with get() = SortableArray.sortingWidth (this.sorterTests.[0]).sortableArrays.[0]



//type SorterTestSet = 
//    | Ints of SorterIntTestSet
//    | Bools of SorterBoolTestSet




//module SorterTestset =

//    let getSortableArrayType (testSet: SorterTestSet) =
//        match testSet with
//        | Ints intTestSet -> intTestSet.SortableArrayType
//        | Bools boolTestSet -> boolTestSet.SortableArrayType

//    let getSortingWidth (testSet: SorterTestSet) =
//        match testSet with
//        | Ints intTestSet -> intTestSet.SortingWidth
//        | Bools boolTestSet -> boolTestSet.SortingWidth
        
