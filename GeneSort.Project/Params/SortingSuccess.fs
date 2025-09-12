namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open SwFull
open SwMerge
open CeLength
open StageCount

type sortingSuccess = 
    | Record
    | P900
    | P999


module SortingSuccess =

   let getCeLengthForFull (sortingSuccess:sortingSuccess) 
                (sortingWidth:int<sortingWidth>) : int<ceLength> =
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


   let getCeLengthForMerge (sortingSuccess:sortingSuccess) 
            (sortingWidth:int<sortingWidth>) : int<ceLength> = 
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
            let swFull = getSwFullForSortingWidth sortingWidth
            getP900StageCountForFull swFull
        | sortingSuccess.P999 -> 
            let swFull = getSwFullForSortingWidth sortingWidth
            getP999StageCountForFull swFull


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


   let getStageCountForMergeSw4 (sortingSuccess:sortingSuccess)
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
            getP999StageCountForMergeSw4 swMerege