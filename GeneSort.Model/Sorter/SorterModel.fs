namespace GeneSort.Model.Sorter

open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Uf6
open GeneSort.Sorter


type SorterModel =
     | Msce of Msce
     | Mssi of Mssi
     | Msrs of Msrs
     | Msuf4 of Msuf4
     | Msuf6 of Msuf6


module SorterModel =

    let makeSorter (model: SorterModel) : sorter =
        match model with
        | Msce msce -> msce.MakeSorter()
        | Mssi mssi -> mssi.MakeSorter()
        | Msrs msrs -> msrs.MakeSorter()
        | Msuf4 msuf4 -> msuf4.MakeSorter()
        | Msuf6 msuf6 -> msuf6.MakeSorter()

    let getSortingWidth (model: SorterModel) : int<sortingWidth> =
        match model with
        | Msce msce -> msce.SortingWidth
        | Mssi mssi -> mssi.SortingWidth
        | Msrs msrs -> msrs.SortingWidth
        | Msuf4 msuf4 -> msuf4.SortingWidth
        | Msuf6 msuf6 -> msuf6.SortingWidth

    let getCeLength (model: SorterModel) : int<ceLength> =
        match model with
        | Msce msce -> msce.CeLength
        | Mssi mssi -> mssi.CeLength
        | Msrs msrs -> msrs.CeLength
        | Msuf4 msuf4 -> msuf4.CeLength
        | Msuf6 msuf6 -> msuf6.CeLength

