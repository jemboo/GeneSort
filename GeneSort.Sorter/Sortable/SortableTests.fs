
namespace GeneSort.Sorter.Sortable

open FSharp.UMX
open GeneSort.Sorter


type sortableTests = 
    | Ints of sortableIntTests
    | Bools of sortableBoolTests


module SortableTests = 

    let getSortableArrayType (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.SortableArrayType
        | Bools boolTest -> boolTest.SortableArrayType

    let getSortingWidth (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth

    let getId (test: sortableTests) : Guid<sortableTestsId> =
        match test with
        | Ints intTest -> intTest.Id
        | Bools boolTest -> boolTest.Id

    let getCount (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.Count
        | Bools boolTest -> boolTest.Count

    let getUnsortedCount (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.UnsortedCount
        | Bools boolTest -> boolTest.UnsortedCount

