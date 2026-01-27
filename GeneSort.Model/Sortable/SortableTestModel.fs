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
            (sortableDataType: sortableDataFormat) : sortableTest =

        match sortableTestModel with

        | MsasF msasF -> 
                match sortableDataType with
                | sortableDataFormat.BoolArray ->        
                        msasF.MakeSortableBoolTest (getSortingWidth sortableTestModel) |> sortableTest.Bools
                | sortableDataFormat.IntArray ->
                    failwith "Ints SortableArrayType not supported"
                | sortableDataFormat.Int8Vector256 ->
                    failwith "Int8Vector256 SortableArrayType not supported"
                | _ -> 
                    failwith "Unsupported SortableArrayType for MsasF"

        | MsasO msasO ->
                match sortableDataType with
                | sortableDataFormat.BoolArray ->        
                     msasO.MakeSortableBoolTest (getSortingWidth sortableTestModel) |> sortableTest.Bools
                | sortableDataFormat.IntArray ->
                     msasO.MakeSortableIntTest (getSortingWidth sortableTestModel) |> sortableTest.Ints
                | sortableDataFormat.Int8Vector256 ->
                     failwith "Int8Vector256 SortableArrayType not supported"
                | _ ->  
                    failwith "Unsupported SortableArrayType for MsasO"

        | MsasMi msasMi ->
                match sortableDataType with
                | sortableDataFormat.BoolArray ->        
                    msasMi.MakeSortableBoolTest |> sortableTest.Bools
                | sortableDataFormat.IntArray ->
                    msasMi.MakeSortableIntTest |> sortableTest.Ints
                | sortableDataFormat.Int8Vector256 -> 
                    msasMi.MakeSortableUint8v256Test |> sortableTest.Uint8v256
                | sortableDataFormat.Int8Vector512 -> 
                    msasMi.MakeSortableUint8v512Test |> sortableTest.Uint8v512 
                | _ ->  
                    failwith "Unsupported SortableArrayType for MsasMi"
                    