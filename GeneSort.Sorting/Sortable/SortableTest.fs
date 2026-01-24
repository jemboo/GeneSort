
namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting


type sortableTest = 
    | Ints of sortableIntTest
    | PackedInts of packedSortableIntTests
    | Bools of sortableBoolTest
    | Uint8v256 of sortableUint8v256Test


module SortableTests = 

    let getSortableArrayType (test: sortableTest) =
        match test with
        | Ints intTest -> intTest.SortableArrayType
        | Bools boolTest -> boolTest.SortableArrayType
        | PackedInts packedIntTest -> packedIntTest.SortableArrayType
        | Uint8v256 uint8v256Test -> uint8v256Test.SortableArrayType

    let getSortingWidth (test: sortableTest) =
        match test with
        | Ints intTest -> intTest.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth
        | PackedInts packedIntTest -> packedIntTest.SortingWidth
        | Uint8v256 uint8v256Test -> uint8v256Test.SortingWidth

    let getId (test: sortableTest) : Guid<sorterTestId> =
        match test with
        | Ints intTest -> intTest.Id
        | Bools boolTest -> boolTest.Id
        | PackedInts packedIntTest -> packedIntTest.Id
        | Uint8v256 uint8v256Test -> uint8v256Test.Id

    let getSortableCount (test: sortableTest) : int<sortableCount> =
        match test with
        | Ints intTest -> intTest.SoratbleCount
        | Bools boolTest -> boolTest.SoratbleCount
        | PackedInts packedIntTest -> packedIntTest.SoratbleCount
        | Uint8v256 uint8v256Test -> uint8v256Test.SoratbleCount

    let getUnsortedCount (test: sortableTest) =
        match test with
        | Ints intTest -> intTest.SortableIntArrays 
                            |> Array.filter(fun sa -> not sa.IsSorted) 
                            |> Array.length 
                            |> UMX.tag<sortableCount>

        | Bools boolTest -> boolTest.SortableBoolArrays 
                            |> Array.filter(fun sa -> not sa.IsSorted) 
                            |> Array.length 
                            |> UMX.tag<sortableCount>

        | PackedInts packed -> 
            let width = %packed.SortingWidth
            let totalElements = packed.PackedValues.Length
            let values = packed.PackedValues
            let mutable unsortedCount = 0
        
            // Iterate through each test case (offset by width)
            for i = 0 to (int %packed.SoratbleCount) - 1 do
                let offset = i * width
                let mutable isSorted = true
                let mutable j = 0
            
                // Check if the current segment [offset .. offset + width - 1] is sorted
                while isSorted && j < width - 1 do
                    if values.[offset + j] > values.[offset + j + 1] then
                        isSorted <- false
                    j <- j + 1
            
                if not isSorted then
                    unsortedCount <- unsortedCount + 1
            unsortedCount |> UMX.tag<sortableCount>

        | Uint8v256 uint8v256Test -> 
                failwith "UnsortedCount not implemented for Uint8v256."

