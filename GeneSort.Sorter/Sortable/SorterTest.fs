
namespace GeneSort.Sorter.Sortable

type sorterTest = 
    | Ints of sorterIntTest
    | Bools of sorterBoolTest


module SorterTest = 

    let getSortableArrayType (test: sorterTest) =
        match test with
        | Ints intTest -> intTest.SortableArrayType
        | Bools boolTest -> boolTest.SortableArrayType

    let getSortingWidth (test: sorterTest) =
        match test with
        | Ints intTest -> intTest.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth

    let getId (test: sorterTest) =
        match test with
        | Ints intTest -> intTest.Id
        | Bools boolTest -> boolTest.Id

    let getCount (test: sorterTest) =
        match test with
        | Ints intTest -> intTest.Count
        | Bools boolTest -> boolTest.Count

