namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open SwFull
open SwMerege
open CeCount
open StageCount

type sortingSuccess = 
    | Record
    | P900
    | P999


module SortingSuccess =

   let getCeCountForFull (sortingSuccess:sortingSuccess) 
                (sortingWidth:int<sortingWidth>) : int<ceCount> =
       match sortingSuccess with
        | sortingSuccess.Record -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getRecordCeCountForFull swFull
        | sortingSuccess.P900 -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getP900CeCountForFull swFull
        | sortingSuccess.P999 -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getP999CeCountForFull swFull


   let getCeCountForMerge (sortingSuccess:sortingSuccess) 
            (sortingWidth:int<sortingWidth>) : int<ceCount> = 
       match sortingSuccess with
        | sortingSuccess.Record -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getRecordCeCountForMerge swMerege
        | sortingSuccess.P900 -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getP900CeCountForMerge swMerege
        | sortingSuccess.P999 -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getP999CeCountForMerge swMerege


   let getStageCountForFull (sortingSuccess:sortingSuccess) 
            (sortingWidth:int<sortingWidth>) : int<stageCount> =
       match sortingSuccess with
        | sortingSuccess.Record -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getRecordStageCountForFull swFull
        | sortingSuccess.P900 -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getP900StageCountForMerge swMerege
        | sortingSuccess.P999 -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getP999StageCountForMerge swMerege


   let getStageCountForMerge (sortingSuccess:sortingSuccess)
            (sortingWidth:int<sortingWidth>) : int<stageCount> =
       match sortingSuccess with
        | sortingSuccess.Record -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getRecordStageCountForMerge swMerege
        | sortingSuccess.P900 -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getP900StageCountForMerge swMerege
        | sortingSuccess.P999 -> 
            let swMerege = getSwMergeForSortingWidth sortingWidth
            getP999StageCountForMerge swMerege