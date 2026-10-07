
namespace GeneSort.Sorting.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Core

type setOfSortableTests =
    private { sortableTests: sortableTests[] }

    static member create (tests: sortableTests[]) : setOfSortableTests =
        if Array.isEmpty tests then
            invalidArg "tests" "A set of sortable tests must not be empty."

        let first = tests.[0]
        let hasSameType test =
            match first, test with
            | Bitv512 _, Bitv512 _
            | Bools _, Bools _
            | Ints _, Ints _
            | PackedInts _, PackedInts _
            | Uint8v256 _, Uint8v256 _
            | Uint8v512 _, Uint8v512 _ -> true
            | _ -> false

        if tests |> Array.exists (hasSameType >> not) then
            invalidArg "tests" "All sortable tests in a set must have the same type."

        { sortableTests = Array.copy tests }

    member this.SortableTests = Array.copy this.sortableTests

    member this.Count = this.sortableTests.Length


module SetOfSortableTests =

    let private newId () = System.Guid.NewGuid() |> UMX.tag<sortableTestsId>

    let private partitionItems (indexPicker: int -> int) (count: int) (items: 'a[]) : 'a[][] =
        if count <= 0 then
            invalidArg "count" "Partition count must be greater than zero."
        if count > items.Length then
            invalidArg "count" "Partition count cannot exceed the number of sortable tests."

        let shuffled = Array.copy items
        for index = shuffled.Length - 1 downto 1 do
            let selectedIndex = indexPicker (index + 1)
            if selectedIndex < 0 || selectedIndex > index then
                invalidArg "indexPicker" $"Index {selectedIndex} is outside [0, {index + 1})."
            let selected = shuffled.[selectedIndex]
            shuffled.[selectedIndex] <- shuffled.[index]
            shuffled.[index] <- selected

        Array.init count (fun partitionIndex ->
            [| for index in partitionIndex .. count .. shuffled.Length - 1 do
                   yield shuffled.[index] |])

    let partition
            (indexPicker: int -> int)
            (count: int<sortableTestsCount>)
            (tests: sortableTests) : setOfSortableTests =
        let partitionCount = %count
        let makeSet makeTests =
            makeTests () |> setOfSortableTests.create

        match tests with
        | Bitv512 bitv512Tests ->
            let arrays =
                bitv512Tests.SimdSortBlocks
                |> Array.collect SortBlockBitv512.toSortableBoolArrays
            makeSet (fun () ->
                partitionItems indexPicker partitionCount arrays
                |> Array.map (fun values ->
                    SortableBitv512Tests.fromBoolArrays (newId ()) bitv512Tests.SortingWidth values
                    |> Bitv512))
        | Bools binaryTests ->
            makeSet (fun () ->
                partitionItems indexPicker partitionCount binaryTests.SortableBinaryArrays
                |> Array.map (fun values ->
                    sortableBinaryTests.create (newId ()) binaryTests.SortingWidth values
                    |> Bools))
        | Ints intTests ->
            makeSet (fun () ->
                partitionItems indexPicker partitionCount intTests.SortableIntArrays
                |> Array.map (fun values ->
                    sortableIntTests.create (newId ()) intTests.SortingWidth values
                    |> Ints))
        | PackedInts packedTests ->
            let values =
                packedTests.PackedValues
                |> Array.chunkBySize (%packedTests.SortingWidth)
            makeSet (fun () ->
                partitionItems indexPicker partitionCount values
                |> Array.map (fun arrays ->
                    arrays
                    |> Array.collect id
                    |> packedSortableIntTests.createFromPackedValues packedTests.SortingWidth
                    |> PackedInts))
        | Uint8v256 uint8v256Tests ->
            let arrays =
                uint8v256Tests.SimdSortBlocks
                |> Array.collect SortBlockUint8v256.toSortableIntArrays
            makeSet (fun () ->
                partitionItems indexPicker partitionCount arrays
                |> Array.map (fun values ->
                    SortableUint8v256Tests.fromIntArrays (newId ()) uint8v256Tests.SortingWidth values
                    |> Uint8v256))
        | Uint8v512 uint8v512Tests ->
            let arrays =
                uint8v512Tests.SimdSortBlocks
                |> Array.collect SortBlockUint8v512.toSortableIntArrays
            makeSet (fun () ->
                partitionItems indexPicker partitionCount arrays
                |> Array.map (fun values ->
                    SortableUint8v512Tests.fromIntArrays (newId ()) uint8v512Tests.SortingWidth values
                    |> Uint8v512))


    let mergeAll (setOfSTs: setOfSortableTests) : sortableTests =
        if setOfSTs.Count = 0 then
            invalidArg "setOfSTs" "Cannot merge an empty set of sortable tests."
        setOfSTs.SortableTests
        |> Array.reduce SortableTests.mergeSortableTests

                        
    //returns the merge of the tests in the setOfSTs that are flagged as true in indexFlags
    let mergeSelected (setOfSTs: setOfSortableTests) (indexFlags: bool[]) : sortableTests =
        if indexFlags.Length <> setOfSTs.Count then
            invalidArg "indexFlags" "Length of indexFlags must match the number of sortable tests in the set."
        let selectedTests =
            setOfSTs.SortableTests
            |> Array.mapi (fun i test -> (i, test))
            |> Array.choose (fun (i, test) -> if indexFlags.[i] then Some test else None)
        if Array.isEmpty selectedTests then
            invalidArg "indexFlags" "At least one sortable test must be selected."
        selectedTests
        |> Array.reduce SortableTests.mergeSortableTests


    let makeRotatingSubsets (setOfSTs: setOfSortableTests) (partitionCount: int<partitionCount>) (includedCount: int<includedCount>) =
        let rotatingFlags = ArrayUtils.makeRotatingFlags %partitionCount %includedCount
        rotatingFlags
        |> Array.map (fun flags -> mergeSelected setOfSTs flags)


        // sorterTestsPartitionCount : int<partitionCount>
        // sorterTestsGenerationsPerSegment : int<generationsPerSegment>
        // sorterTestsSegmentsProfile : string
        // sorterTestsCycleCount : int<cycleCount>