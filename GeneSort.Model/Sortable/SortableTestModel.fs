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
            (sortableDataType: sortableDataType) : sortableTest =

        match sortableTestModel with

        | MsasF msasF -> 
                match sortableDataType with
                | sortableDataType.Bools ->        
                        msasF.MakeSortableBoolTest (getSortingWidth sortableTestModel) |> sortableTest.Bools
                | sortableDataType.Ints ->
                    failwith "Ints SortableArrayType not supported"
                | sortableDataType.Int8Vector256 ->
                    failwith "Int8Vector256 SortableArrayType not supported"

        | MsasO msasO ->
                match sortableDataType with
                | sortableDataType.Bools ->        
                     msasO.MakeSortableBoolTest (getSortingWidth sortableTestModel) |> sortableTest.Bools
                | sortableDataType.Ints ->
                     msasO.MakeSortableIntTest (getSortingWidth sortableTestModel) |> sortableTest.Ints
                | sortableDataType.Int8Vector256 ->
                     failwith "Int8Vector256 SortableArrayType not supported"

        | MsasMi msasMi ->
                match sortableDataType with
                | sortableDataType.Bools ->        
                    msasMi.MakeSortableBoolTest |> sortableTest.Bools
                | sortableDataType.Ints ->
                    msasMi.MakeSortableIntTest |> sortableTest.Ints
                | sortableDataType.Int8Vector256 -> 
                    msasMi.MakeSortableUint8v256Test |> sortableTest.Uint8v256