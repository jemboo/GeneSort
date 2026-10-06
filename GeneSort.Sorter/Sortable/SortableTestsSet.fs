
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter


type sortableTestsSet = 
    | Ints of sortableIntTestsSet
    | Bools of sortableBoolTestsSet



module SortableTestset =

    let getSortableArrayType (testSet: sortableTestsSet) =
        match testSet with
        | Ints intTestSet -> intTestSet.SortableArrayType
        | Bools boolTestSet -> boolTestSet.SortableArrayType

    let getSortingWidth (testSet: sortableTestsSet) =
        match testSet with
        | Ints intTestSet -> intTestSet.SortingWidth
        | Bools boolTestSet -> boolTestSet.SortingWidth
        
