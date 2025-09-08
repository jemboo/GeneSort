namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable



type SorterTestModel =
     | MsasF of MsasF     // MsasF = a full bool test set for a given sorting width
     | MsasO of MsasO     // MsasO = generated from a seed permutation; for bool models, it's expanded from the integer permutations
     | MsasMb of MsasMb   // All (sorting width)/2 merge test cases as bool arrays
     | MsasMi of MsasMi   // All (sorting width)/2 merge test cases as int arrays


module SorterTestModel =

    let getSortingWidth (model: SorterTestModel) : int<sortingWidth> =
        match model with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>


    let makeSorterTest (model: SorterTestModel) (sortableArrayType:SortableArrayType) : sortableTests =
        match model with
        | MsasF msasF -> 
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                        msasF.MakeSortableTests(getSortingWidth model)
                | SortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasO msasO ->
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                     msasO.MakeSortableBoolTests(getSortingWidth model) |> sortableTests.Bools
                | SortableArrayType.Ints ->
                     msasO.MakeSortableIntTests(getSortingWidth model) |> sortableTests.Ints

        | MsasMb msasMb ->
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                        msasMb.MakeSortableTests(getSortingWidth model)
                | SortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasMi msasMi ->
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                        msasMi.MakeSortableTests(getSortingWidth model)
                | SortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"