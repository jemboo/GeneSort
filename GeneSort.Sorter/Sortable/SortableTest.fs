
namespace GeneSort.Sorter.Sortable

open FSharp.UMX
open GeneSort.Sorter


type sortableTest = 
    | Ints of sortableIntTest
    | PackedInts of packedSortableIntTests
    | Bools of sortableBoolTest


module SortableTests = 

    let getSortableArrayType (test: sortableTest) =
        match test with
        | Ints intTest -> intTest.SortableArrayType
        | Bools boolTest -> boolTest.SortableArrayType
        | PackedInts packedIntTest -> packedIntTest.SortableArrayType

    let getSortingWidth (test: sortableTest) =
        match test with
        | Ints intTest -> intTest.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth
        | PackedInts packedIntTest -> packedIntTest.SortingWidth

    let getId (test: sortableTest) : Guid<sorterTestId> =
        match test with
        | Ints intTest -> intTest.Id
        | Bools boolTest -> boolTest.Id
        | PackedInts packedIntTest -> packedIntTest.Id

    let getSortableCount (test: sortableTest) : int<sortableCount> =
        match test with
        | Ints intTest -> intTest.SoratbleCount
        | Bools boolTest -> boolTest.SoratbleCount
        | PackedInts packedIntTest -> packedIntTest.SoratbleCount

    let getUnsortedCount (test: sortableTest) =
        match test with
        | Ints intTest -> intTest.UnsortedCount
        | Bools boolTest -> boolTest.UnsortedCount
        | PackedInts packedIntTest -> packedIntTest.UnsortedCount

