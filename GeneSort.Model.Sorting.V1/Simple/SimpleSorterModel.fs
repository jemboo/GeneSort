namespace GeneSort.Model.Sorting.V1.Simple

open System
open FSharp.UMX

open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1


type simpleSorterModel =
     | Msce of msce
     | Mssi of mssi
     | Msrs of msrs
     | Msuf4 of msuf4
     | Msuf6 of msuf6




module SimpleSorterModel =
    
    let getId (model: simpleSorterModel) : Guid<simpleSorterModelId> =
        match model with
        | Msce msce -> msce.Id
        | Mssi mssi -> mssi.Id
        | Msrs msrs -> msrs.Id
        | Msuf4 msuf4 -> msuf4.Id
        | Msuf6 msuf6 -> msuf6.Id

    let makeSorter (model: simpleSorterModel) : sorter =
        match model with
        | Msce msce -> msce.MakeSorter()
        | Mssi mssi -> mssi.MakeSorter()
        | Msrs msrs -> msrs.MakeSorter()
        | Msuf4 msuf4 -> msuf4.MakeSorter()
        | Msuf6 msuf6 -> msuf6.MakeSorter()

    let getSortingWidth (model: simpleSorterModel) : int<sortingWidth> =
        match model with
        | Msce msce -> msce.SortingWidth
        | Mssi mssi -> mssi.SortingWidth
        | Msrs msrs -> msrs.SortingWidth
        | Msuf4 msuf4 -> msuf4.SortingWidth
        | Msuf6 msuf6 -> msuf6.SortingWidth

    let getCeLength (model: simpleSorterModel) : int<ceLength> =
        match model with
        | Msce msce -> msce.CeLength
        | Mssi mssi -> mssi.CeLength
        | Msrs msrs -> msrs.CeLength
        | Msuf4 msuf4 -> msuf4.CeLength
        | Msuf6 msuf6 -> msuf6.CeLength

    let getStageLength (model: simpleSorterModel) : int<stageLength> =
        match model with
        | Msce msce -> msce.StageLength
        | Mssi mssi -> mssi.StageLength
        | Msrs msrs -> msrs.StageLength
        | Msuf4 msuf4 -> msuf4.StageLength
        | Msuf6 msuf6 -> msuf6.StageLength
