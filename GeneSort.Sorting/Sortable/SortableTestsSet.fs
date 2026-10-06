
namespace GeneSort.Sorting.Sortable


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
        
