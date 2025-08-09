namespace GeneSort.Sorter

open System


type sortableInts =
    private
        { values: int[]
          sortingWidth: int<sortingWidth>
          symbolSetSize: int }


type sortableBools =
    private { values: bool[]; order: int<sortingWidth> }


module SortableInts = ()


module SortableBools = ()