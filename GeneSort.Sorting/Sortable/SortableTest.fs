
namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting


type sortableTest = 
    | Bitv512 of sortableBitv512Test
    | Bools of sortableBinaryTest
    | Ints of sortableIntTest
    | PackedInts of packedSortableIntTests
    | Uint8v256 of sortableUint8v256Test
    | Uint8v512 of sortableUint8v512Test


module SortableTests = 

    let getSortableDataFormat (test: sortableTest) : sortableDataFormat =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.SortableDataFormat
        | Bools boolTest -> boolTest.SortableArrayType
        | Ints intTest -> intTest.SortableDataFormat
        | PackedInts packedIntTest -> packedIntTest.SortableDataFormat
        | Uint8v256 uint8v256Test -> uint8v256Test.SortableDataFormat
        | Uint8v512 uint8v512Test -> uint8v512Test.SortableDataFormat

    let getSortingWidth (test: sortableTest) =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth
        | Ints intTest -> intTest.SortingWidth
        | PackedInts packedIntTest -> packedIntTest.SortingWidth
        | Uint8v256 uint8v256Test -> uint8v256Test.SortingWidth
        | Uint8v512 uint8v512Test -> uint8v512Test.SortingWidth

    let getId (test: sortableTest) : Guid<sorterTestId> =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.Id
        | Bools boolTest -> boolTest.Id
        | Ints intTest -> intTest.Id
        | PackedInts packedIntTest -> packedIntTest.Id
        | Uint8v256 uint8v256Test -> uint8v256Test.Id
        | Uint8v512 uint8v512Test -> uint8v512Test.Id

    let getSortableCount (test: sortableTest) : int<sortableCount> =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.SoratbleCount
        | Bools boolTest -> boolTest.SoratbleCount
        | Ints intTest -> intTest.SoratbleCount
        | PackedInts packedIntTest -> packedIntTest.SoratbleCount
        | Uint8v256 uint8v256Test -> uint8v256Test.SoratbleCount
        | Uint8v512 uint8v512Test -> uint8v512Test.SoratbleCount


    let getUnsortedCount (test: sortableTest) =
        match test with
        | Bitv512 bitv512Test -> 
                failwith "UnsortedCount not implemented for Bitv512."
        | Bools boolTest -> boolTest.SortableBinaryArrays 
                            |> Array.filter(fun sa -> not sa.IsSorted) 
                            |> Array.length 
                            |> UMX.tag<sortableCount>

        | Ints intTest -> intTest.SortableIntArrays 
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

        | Uint8v512 uint8v512Test ->
                failwith "UnsortedCount not implemented for Uint8v512."

