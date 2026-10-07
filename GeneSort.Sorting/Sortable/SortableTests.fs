
namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting


type sortableTests = 
    | Bitv512 of sortableBitv512Tests
    | Bools of sortableBinaryTests
    | Ints of sortableIntTests
    | PackedInts of packedSortableIntTests
    | Uint8v256 of sortableUint8v256Tests
    | Uint8v512 of sortableUint8v512Tests


module SortableTests = 

    let getId (test: sortableTests) : Guid<sortableTestsId> =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.Id
        | Bools boolTest -> boolTest.Id
        | Ints intTest -> intTest.Id
        | PackedInts packedIntTest -> packedIntTest.Id
        | Uint8v256 uint8v256Test -> uint8v256Test.Id
        | Uint8v512 uint8v512Test -> uint8v512Test.Id

    let getSortableDataFormat (test: sortableTests) : sortableDataFormat =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.SortableDataFormat
        | Bools boolTest -> boolTest.SortableArrayType
        | Ints intTest -> intTest.SortableDataFormat
        | PackedInts packedIntTest -> packedIntTest.SortableDataFormat
        | Uint8v256 uint8v256Test -> uint8v256Test.SortableDataFormat
        | Uint8v512 uint8v512Test -> uint8v512Test.SortableDataFormat

    let getSortingWidth (test: sortableTests) =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.SortingWidth
        | Bools boolTest -> boolTest.SortingWidth
        | Ints intTest -> intTest.SortingWidth
        | PackedInts packedIntTest -> packedIntTest.SortingWidth
        | Uint8v256 uint8v256Test -> uint8v256Test.SortingWidth
        | Uint8v512 uint8v512Test -> uint8v512Test.SortingWidth

    let getSortableCount (test: sortableTests) : int<sortableCount> =
        match test with
        | Bitv512 bitv512Test -> bitv512Test.SortableCount
        | Bools boolTest -> boolTest.SortableCount
        | Ints intTest -> intTest.SortableCount
        | PackedInts packedIntTest -> packedIntTest.SortableCount
        | Uint8v256 uint8v256Test -> uint8v256Test.SortableCount
        | Uint8v512 uint8v512Test -> uint8v512Test.SortableCount


    let getUnsortedCount (test: sortableTests) =
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
            for i = 0 to (int %packed.SortableCount) - 1 do
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


    let mergeSortableTests (test1: sortableTests) (test2: sortableTests) : sortableTests =
        let requireMatchingSortingWidths sortingWidth1 sortingWidth2 =
            if sortingWidth1 <> sortingWidth2 then
                invalidArg "test2" "Sortable tests must have the same sorting width."

        match test1, test2 with
        | Bitv512 bitv512Test1, Bitv512 bitv512Test2 ->
            requireMatchingSortingWidths bitv512Test1.SortingWidth bitv512Test2.SortingWidth
            sortableBitv512Tests.create
                (System.Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                bitv512Test1.SortingWidth
                (Array.append bitv512Test1.SimdSortBlocks bitv512Test2.SimdSortBlocks)
            |> Bitv512
        | Bools boolTest1, Bools boolTest2 ->
            requireMatchingSortingWidths boolTest1.SortingWidth boolTest2.SortingWidth
            sortableBinaryTests.create
                (System.Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                boolTest1.SortingWidth
                (Array.append boolTest1.SortableBinaryArrays boolTest2.SortableBinaryArrays)
            |> Bools
        | Ints intTest1, Ints intTest2 ->
            requireMatchingSortingWidths intTest1.SortingWidth intTest2.SortingWidth
            sortableIntTests.create
                (System.Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                intTest1.SortingWidth
                (Array.append intTest1.SortableIntArrays intTest2.SortableIntArrays)
            |> Ints
        | PackedInts packedIntTest1, PackedInts packedIntTest2 ->
            requireMatchingSortingWidths packedIntTest1.SortingWidth packedIntTest2.SortingWidth
            packedSortableIntTests.createFromPackedValues
                packedIntTest1.SortingWidth
                (Array.append packedIntTest1.PackedValues packedIntTest2.PackedValues)
            |> PackedInts
        | Uint8v256 uint8v256Test1, Uint8v256 uint8v256Test2 ->
            requireMatchingSortingWidths uint8v256Test1.SortingWidth uint8v256Test2.SortingWidth
            sortableUint8v256Tests.create
                (System.Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                uint8v256Test1.SortingWidth
                (Array.append uint8v256Test1.SimdSortBlocks uint8v256Test2.SimdSortBlocks)
            |> Uint8v256
        | Uint8v512 uint8v512Test1, Uint8v512 uint8v512Test2 ->
            requireMatchingSortingWidths uint8v512Test1.SortingWidth uint8v512Test2.SortingWidth
            sortableUint8v512Tests.create
                (System.Guid.NewGuid() |> UMX.tag<sortableTestsId>)
                uint8v512Test1.SortingWidth
                (Array.append uint8v512Test1.SimdSortBlocks uint8v512Test2.SimdSortBlocks)
            |> Uint8v512
        | _ -> failwith "Cannot merge tests of different types."
