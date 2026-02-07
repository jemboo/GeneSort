namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable


type sortableTestModel =
     | MsasF of msasF     // MsasF = a full bool test set for a given sorting width
     | MsasO of msasO     // MsasO = generated from a seed permutation; for bool models, it's expanded from the integer permutations
     | MsasMi of msasM    // All (sorting width)/2 merge test cases


module SortableTestModel =

    let getSortingWidth (sortableTestModel: sortableTestModel): int<sortingWidth> =
        match sortableTestModel with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>
        | MsasMi msasMi -> msasMi.sortingWidth


    let makeSortableTests 
            (sortableTestModel: sortableTestModel) 
            (sortableDataFormat: sortableDataFormat) : sortableTest =

        match sortableTestModel with

        | MsasF msasF -> 
                match sortableDataFormat with
                | sortableDataFormat.BoolArray ->        
                    msasF.MakeSortableBoolTest (getSortingWidth sortableTestModel) |> sortableTest.Bools
                | sortableDataFormat.IntArray ->
                    msasF.MakeSortableIntTest (getSortingWidth sortableTestModel) |> sortableTest.Ints
                | sortableDataFormat.BitVector256 ->
                    failwith "BitVector256 SortableArrayType not supported"
                | sortableDataFormat.BitVector512 ->
                    msasF.MakeSortableBitv512Test (getSortingWidth sortableTestModel) |> sortableTest.Bitv512
                | sortableDataFormat.Int8Vector256 ->
                    msasF.MakeSortableUint8v256Test (getSortingWidth sortableTestModel) |> sortableTest.Uint8v256
                | sortableDataFormat.Int8Vector512  -> 
                    msasF.MakeSortableUint8v512Test (getSortingWidth sortableTestModel) |> sortableTest.Uint8v512
                | sortableDataFormat.PackedIntArray ->
                    failwith "PackedIntArray SortableArrayType not supported"

        | MsasO msasO ->
                match sortableDataFormat with
                | sortableDataFormat.BoolArray ->        
                     msasO.MakeSortableBoolTest (getSortingWidth sortableTestModel) |> sortableTest.Bools
                | sortableDataFormat.IntArray ->
                     msasO.MakeSortableIntTest (getSortingWidth sortableTestModel) |> sortableTest.Ints
                | sortableDataFormat.Int8Vector256 ->
                     failwith "Int8Vector256 SortableArrayType not supported"
                | _ ->  
                    failwith "Unsupported SortableArrayType for MsasO"

        | MsasMi msasMi ->
                match sortableDataFormat with
                | sortableDataFormat.BoolArray ->        
                    msasMi.MakeSortableBoolTest |> sortableTest.Bools
                | sortableDataFormat.IntArray ->
                    msasMi.MakeSortableIntTest |> sortableTest.Ints
                | sortableDataFormat.Int8Vector256 -> 
                    msasMi.MakeSortableUint8v256Test |> sortableTest.Uint8v256
                | sortableDataFormat.Int8Vector512 -> 
                    msasMi.MakeSortableUint8v512Test |> sortableTest.Uint8v512
                | sortableDataFormat.BitVector512 ->
                    msasMi.MakeSortableBitv512Test |> sortableTest.Bitv512
                | _ ->  
                    failwith "Unsupported SortableArrayType for MsasMi"
                    