namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


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
            (sortableDataType: sortableDataType) : sortableTests =

        match sortableTestModel with

        | MsasF msasF -> 
                match sortableDataType with
                | sortableDataType.Bools ->        
                        msasF.MakeSortableBoolTests (getSortingWidth sortableTestModel) |> sortableTests.Bools
                | sortableDataType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasO msasO ->
                match sortableDataType with
                | sortableDataType.Bools ->        
                     msasO.MakeSortableBoolTests (getSortingWidth sortableTestModel) |> sortableTests.Bools
                | sortableDataType.Ints ->
                     msasO.MakeSortableIntTests (getSortingWidth sortableTestModel) |> sortableTests.Ints

        | MsasMi msasMi ->
                match sortableDataType with
                | sortableDataType.Bools ->        
                    msasMi.MakeSortableBoolTests |> sortableTests.Bools
                | sortableDataType.Ints ->
                    msasMi.MakeSortableIntTests |> sortableTests.Ints