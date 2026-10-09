
namespace GeneSort.Sorting.Sortable


type sortableTestSet = 
    | Ints of sortableIntTestSet
    | Bools of sortableBoolTestSet



module SortableTestset =

    let getSortableArrayType (testSet: sortableTestSet) =
        match testSet with
        | Ints intTestSet -> intTestSet.SortableArrayType
        | Bools boolTestSet -> boolTestSet.SortableArrayType

    let getSortingWidth (testSet: sortableTestSet) =
        match testSet with
        | Ints intTestSet -> intTestSet.SortingWidth
        | Bools boolTestSet -> boolTestSet.SortingWidth
        
