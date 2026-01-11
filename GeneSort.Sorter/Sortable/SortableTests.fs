
namespace GeneSort.Sorter.Sortable

open FSharp.UMX
open GeneSort.Sorter


type sortableTests = 
    | Ints of sortableIntTests
    | PackedInts of packedSortableIntTests
    | Bools of sortableBoolTests


module SortableTests = 

    let getSortableArrayType (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.SortableArrayType
        | Bools boolTest -> boolTest.SortableArrayType
        | PackedInts packedIntTest -> packedIntTest.SortableArrayType

    let getSortingWidth (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth
        | PackedInts packedIntTest -> packedIntTest.SortingWidth

    let getId (test: sortableTests) : Guid<sortableTestsId> =
        match test with
        | Ints intTest -> intTest.Id
        | Bools boolTest -> boolTest.Id
        | PackedInts packedIntTest -> packedIntTest.Id

    let getSortableCount (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.SoratbleCount
        | Bools boolTest -> boolTest.SoratbleCount
        | PackedInts packedIntTest -> packedIntTest.SoratbleCount

    let getUnsortedCount (test: sortableTests) =
        match test with
        | Ints intTest -> intTest.UnsortedCount
        | Bools boolTest -> boolTest.UnsortedCount
        | PackedInts packedIntTest -> packedIntTest.UnsortedCount

