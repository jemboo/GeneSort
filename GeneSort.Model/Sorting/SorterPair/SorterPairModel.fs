namespace GeneSort.Model.Sorting.SorterPair
open FSharp.UMX

open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting


type sorterPairModel =
     | Msce of msce
     | Mssi of mssi
     | Msrs of msrs
     | Msuf4 of msuf4
     | Msuf6 of msuf6


module SorterPairModel =
    
    let getId (model: sorterPairModel) : Guid<sorterModelID> =
        match model with
        | Msce msce -> msce.Id
        | Mssi mssi -> mssi.Id
        | Msrs msrs -> msrs.Id
        | Msuf4 msuf4 -> msuf4.Id
        | Msuf6 msuf6 -> msuf6.Id

    let makeSorters (model: sorterPairModel) : sorter =
        match model with
        | Msce msce -> msce.MakeSorter()
        | Mssi mssi -> mssi.MakeSorter()
        | Msrs msrs -> msrs.MakeSorter()
        | Msuf4 msuf4 -> msuf4.MakeSorter()
        | Msuf6 msuf6 -> msuf6.MakeSorter()

    let getSortingWidth (model: sorterPairModel) : int<sortingWidth> =
        match model with
        | Msce msce -> msce.SortingWidth
        | Mssi mssi -> mssi.SortingWidth
        | Msrs msrs -> msrs.SortingWidth
        | Msuf4 msuf4 -> msuf4.SortingWidth
        | Msuf6 msuf6 -> msuf6.SortingWidth

    let getCeLength (model: sorterPairModel) : int<ceLength> =
        match model with
        | Msce msce -> msce.CeLength
        | Mssi mssi -> mssi.CeLength
        | Msrs msrs -> msrs.CeLength
        | Msuf4 msuf4 -> msuf4.CeLength
        | Msuf6 msuf6 -> msuf6.CeLength

    // Add a method to sorterPairModel that generates it's child sorter models.
    let isAChildOf (parentModel: sorterPairModel) (sorterId: Guid<sorterId>) : bool =
        failwith "Not implemented"

