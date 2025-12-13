namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable


type sortableTestModel =
     | MsasF of msasF     // MsasF = a full bool test set for a given sorting width
     | MsasO of msasO     // MsasO = generated from a seed permutation; for bool models, it's expanded from the integer permutations
     | MsasMi of msasM   // All (sorting width)/2 merge test cases


module SortableTestModel =

    let getSortingWidth (sortableTestModel: sortableTestModel): int<sortingWidth> =
        match sortableTestModel with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>
        | MsasMi msasMi -> msasMi.sortingWidth


    let makeSortableTests 
            (sortableTestModel: sortableTestModel) 
            (sortableArrayType: sortableArrayType) : sortableTests =

        match sortableTestModel with

        | MsasF msasF -> 
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                        msasF.MakeSortableBoolTests (getSortingWidth sortableTestModel) |> sortableTests.Bools
                | sortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasO msasO ->
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                     msasO.MakeSortableBoolTests (getSortingWidth sortableTestModel) |> sortableTests.Bools
                | sortableArrayType.Ints ->
                     msasO.MakeSortableIntTests (getSortingWidth sortableTestModel) |> sortableTests.Ints

        | MsasMi msasMi ->
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                    msasMi.MakeSortableBoolTests |> sortableTests.Bools
                | sortableArrayType.Ints ->
                    msasMi.MakeSortableIntTests |> sortableTests.Ints