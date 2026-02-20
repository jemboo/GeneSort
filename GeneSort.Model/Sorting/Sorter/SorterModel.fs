namespace GeneSort.Model.Sorting
open FSharp.UMX

open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting


type sorterModel =
     | Msce of msce
     | Mssi of mssi
     | Msrs of msrs
     | Msuf4 of msuf4
     | Msuf6 of msuf6




module SorterModel =
    
    let getId (model: sorterModel) : Guid<sorterModelID> =
        match model with
        | Msce msce -> msce.Id
        | Mssi mssi -> mssi.Id
        | Msrs msrs -> msrs.Id
        | Msuf4 msuf4 -> msuf4.Id
        | Msuf6 msuf6 -> msuf6.Id

    let makeSorter (model: sorterModel) : (sorter * sortingModelTag) =
        match model with
        | Msce msce -> (msce.MakeSorter(), SortingModelTag.create %msce.Id modelTag.Single)
        | Mssi mssi -> (mssi.MakeSorter(), SortingModelTag.create %mssi.Id modelTag.Single)
        | Msrs msrs -> (msrs.MakeSorter(), SortingModelTag.create %msrs.Id modelTag.Single)
        | Msuf4 msuf4 -> (msuf4.MakeSorter(), SortingModelTag.create %msuf4.Id modelTag.Single)
        | Msuf6 msuf6 -> (msuf6.MakeSorter(), SortingModelTag.create %msuf6.Id modelTag.Single)

    let getSortingWidth (model: sorterModel) : int<sortingWidth> =
        match model with
        | Msce msce -> msce.SortingWidth
        | Mssi mssi -> mssi.SortingWidth
        | Msrs msrs -> msrs.SortingWidth
        | Msuf4 msuf4 -> msuf4.SortingWidth
        | Msuf6 msuf6 -> msuf6.SortingWidth

    let getCeLength (model: sorterModel) : int<ceLength> =
        match model with
        | Msce msce -> msce.CeLength
        | Mssi mssi -> mssi.CeLength
        | Msrs msrs -> msrs.CeLength
        | Msuf4 msuf4 -> msuf4.CeLength
        | Msuf6 msuf6 -> msuf6.CeLength

    let getStageLength (model: sorterModel) : int<stageLength> =
        match model with
        | Msce msce -> msce.StageLength
        | Mssi mssi -> mssi.StageLength
        | Msrs msrs -> msrs.StageLength
        | Msuf4 msuf4 -> msuf4.StageLength
        | Msuf6 msuf6 -> msuf6.StageLength