namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Model.Sorter

//[<Struct; CustomEquality; NoComparison>]
type SortableModel = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          ceCodes: int array } 


module SortableModel = ()

