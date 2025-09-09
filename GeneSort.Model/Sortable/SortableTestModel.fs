﻿namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable



type sortableTestModel =
     | MsasF of MsasF     // MsasF = a full bool test set for a given sorting width
     | MsasO of MsasO     // MsasO = generated from a seed permutation; for bool models, it's expanded from the integer permutations
     | MsasMb of MsasMb   // All (sorting width)/2 merge test cases as bool arrays
     | MsasMi of MsasMi   // All (sorting width)/2 merge test cases as int arrays


module SortableTestModel =

    let getSortingWidth (sorterTestModel: sortableTestModel) : int<sortingWidth> =
        match sorterTestModel with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>
        | MsasMb msasMb -> msasMb.sortingWidth
        | MsasMi msasMi -> msasMi.sortingWidth


    let makeSortableTests (sortableTestModel: sortableTestModel) (sortableArrayType:sortableArrayType) : sortableTests =
        match sortableTestModel with
        | MsasF msasF -> 
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                        msasF.MakeSortableTests(getSortingWidth sortableTestModel)
                | sortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasO msasO ->
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                     msasO.MakeSortableBoolTests(getSortingWidth sortableTestModel) |> sortableTests.Bools
                | sortableArrayType.Ints ->
                     msasO.MakeSortableIntTests(getSortingWidth sortableTestModel) |> sortableTests.Ints

        | MsasMb msasMb ->
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                        msasMb.MakeSortableTests(getSortingWidth sortableTestModel)
                | sortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasMi msasMi ->
                match sortableArrayType with
                | sortableArrayType.Bools ->        
                    failwith "Bools SortableArrayType not supported"
                | sortableArrayType.Ints ->
                        msasMi.MakeSortableTests(getSortingWidth sortableTestModel)



    let makeSortableTestsForMerge 
                (sortableArrayType:sortableArrayType) 
                (sortingWidth : int<sortingWidth>): sortableTests =

        match sortableArrayType with
        | sortableArrayType.Bools ->     
                let msasMb = MsasMb.create sortingWidth
                msasMb.MakeSortableTests(sortingWidth)

        | sortableArrayType.Ints ->
                let msasMi = MsasMi.create sortingWidth
                msasMi.MakeSortableTests(sortingWidth)
