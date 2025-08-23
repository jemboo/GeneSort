namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter



type SorterTestModel =
     | MsasF of MsasF
     | MsasO of MsasO


module SorterTestModel =

    let getSortingWidth (model: SorterTestModel) : int<sortingWidth> =
        match model with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>


    let makeSortableArraySet (model: SorterTestModel) (sortableArrayType:SortableArrayType) : SortableArraySet =
        match model with
        | MsasF msasF -> 
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                        msasF.MakeSortableArraySet(getSortingWidth model)
                | SortableArrayType.Ints ->
                    failwith "Ints SortableArrayType not supported"

        | MsasO msasO ->
                match sortableArrayType with
                | SortableArrayType.Bools ->        
                     msasO.MakeSortableBoolArraySet(getSortingWidth model)
                | SortableArrayType.Ints ->
                     msasO.MakeSortableIntArraySet(getSortingWidth model)