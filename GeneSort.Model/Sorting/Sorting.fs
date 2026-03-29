namespace GeneSort.Model.Sorting
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair


type sorting =
     | Single of sorterModel
     | Pairs of sorterPairModel


module Sorting =

    let getModelTags (sorting:sorting) : modelTag [] =
        match sorting with
        | Single _ -> ModelTag.allSingleTags
        | Pairs _ -> ModelTag.allSplitJoinTags

    let getSortingWidth (model: sorting) : int<sortingWidth> =
        match model with
        | Single sms -> sms |> SorterModel.getSortingWidth
        | Pairs smp -> smp |> SorterPairModel.getSortingWidth


    let getStageLength (model: sorting) : int<stageLength> =
        match model with
        | Single sms -> sms |> SorterModel.getStageLength
        | Pairs smp -> smp |> SorterPairModel.getStageLength

    
    let getId (model: sorting) : Guid<sortingId> =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) |> UMX.tag<sortingId>
        | Pairs smp -> %(smp |> SorterPairModel.getId) |> UMX.tag<sortingId>


    let makeSorters (model: sorting) : sorter []  =
        match model with
        | Single sms -> sms |> SorterModel.makeSorter |> Array.singleton
        | Pairs smp ->  smp |> SorterPairModel.makeSorters


    let containsSorter (sorterId: Guid<sorterId>) (model: sorting) : bool =
        match model with
        | Single sms -> %(sms |> SorterModel.getId) = (%sorterId)
        | Pairs smp -> sorterId |> SorterPairModel.isAChildOf smp


    let containsAnySorter (sorterIds: Guid<sorterId> []) (model: sorting) : bool =
        match model with
        | Single sms -> sorterIds |> Array.exists (fun id -> %(sms |> SorterModel.getId) = (%id))
        | Pairs smp -> 
            sorterIds |> Array.exists (fun id -> id |> SorterPairModel.isAChildOf smp)


    let getSorterIdForModelTag  (model: sorting) (tag: modelTag) : Guid<sorterId> =
        match model with
        | Single sms -> SorterModel.getSorterIdForModelTag sms tag
        | Pairs smp -> SorterPairModel.getSorterIdForModelTag smp tag


    let getSorterIdsWithModelTags (model: sorting) : (Guid<sorterId> * modelTag) [] =
        match model with
        | Single sms -> SorterModel.getSorterIdsWithModelTags sms
        | Pairs smp -> SorterPairModel.getSorterIdsWithModelTags smp


    let getSorterIdsWithSortingTags (model:sorting) : (Guid<sorterId> * sortingTag) [] =
        model |> getSorterIdsWithModelTags 
              |> Array.map(fun (sid, mt) -> (sid, SortingTag.create (model |> getId) mt))