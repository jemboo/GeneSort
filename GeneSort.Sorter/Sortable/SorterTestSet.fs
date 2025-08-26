
namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter


type sorterTestSet = 
    | Ints of sorterIntTestSet
    | Bools of sorterBoolTestSet



module SorterTestset =

    let getSortableArrayType (testSet: sorterTestSet) =
        match testSet with
        | Ints intTestSet -> intTestSet.SortableArrayType
        | Bools boolTestSet -> boolTestSet.SortableArrayType

    let getSortingWidth (testSet: sorterTestSet) =
        match testSet with
        | Ints intTestSet -> intTestSet.SortingWidth
        | Bools boolTestSet -> boolTestSet.SortingWidth
        
