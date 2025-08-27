
namespace GeneSort.Sorter.Sortable


type sorterTests = 
    | Ints of sorterIntTests
    | Bools of sorterBoolTests


module SorterTests = 

    let getSortableArrayType (test: sorterTests) =
        match test with
        | Ints intTest -> intTest.SortableArrayType
        | Bools boolTest -> boolTest.SortableArrayType

    let getSortingWidth (test: sorterTests) =
        match test with
        | Ints intTest -> intTest.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth

    let getId (test: sorterTests) =
        match test with
        | Ints intTest -> intTest.Id
        | Bools boolTest -> boolTest.Id

    let getCount (test: sorterTests) =
        match test with
        | Ints intTest -> intTest.Count
        | Bools boolTest -> boolTest.Count

