namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable



type SorterTestModel =
     | MsasF of MsasF
     | MsasO of MsasO


module SorterTestModel =

    let getSortingWidth (model: SorterTestModel) : int<sortingWidth> =
        match model with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>


    let makeSorterTest (model: SorterTestModel) (sortableArrayType:SortableArrayType) : sorterTests =
        match model with
        | MsasF msasF -> 
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                        msasF.MakeSorterTest(getSortingWidth model)
                | SortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasO msasO ->
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                     msasO.MakeSortableBoolArraySet(getSortingWidth model) |> sorterTests.Bools
                | SortableArrayType.Ints ->
                     msasO.MakeSortableIntArraySet(getSortingWidth model) |> sorterTests.Ints