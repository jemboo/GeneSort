namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.SortingOps


type sorterCeUseProfile = {
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
    lastUsedCeIndex: int
    unsortedCount: int
    ceCount: int<ceLength>
    stageLength: int<stageLength>
    segmentTotals: ArrayProperties.segmentWithPayload<int> []
}

module SorterCeUseProfile =

    //all segments will be equal size if blockGrowthRate = 1. As blockGrowthRate gets larger, the last segments get larger.
    let makeProfileSegments 
            (segmentCount:int) 
            (blockGrowthRate:float) 
            (unbrokenLength: int) : ArrayProperties.segment [] = 

        ArrayProperties.breakIntoExponentialSegments segmentCount blockGrowthRate unbrokenLength


    let makeSorterCeUseProfile 
            (profileSegments:ArrayProperties.segment [])
            (sorterSetId: Guid<sorterSetId>)
            (sorterTestsId: Guid<sortableTestsId>)
            (sorterEval : sorterEval) : sorterCeUseProfile =
        {   
            sorterCeUseProfile.segmentTotals = 
                    ArrayProperties.getSegmentSums sorterEval.CeBlockUsage.UseCounts profileSegments
            sorterId = sorterEval.SorterId
            sorterSetId = sorterSetId
            lastUsedCeIndex = sorterEval.getLastUsedCeIndex
            unsortedCount = sorterEval.UnsortedCount
            ceCount = sorterEval.getUsedCeCount()
            stageLength = sorterEval.getStageLength()
            sorterTestsId = sorterTestsId
        }

    let makeReportLine (sortingWidth:int<sortingWidth> option) (sorterModelKey:string) (profile: sorterCeUseProfile) : string[] =
        [|
            match sortingWidth with
            | Some sw -> yield (%sw.ToString())
            | None -> ()
            yield sorterModelKey
            yield (%profile.sorterId.ToString()) 
            yield (%profile.sorterSetId.ToString()) 
            yield (%profile.sorterTestsId.ToString())
            yield (profile.unsortedCount.ToString())
            yield (profile.ceCount.ToString())
            yield (profile.stageLength.ToString())
            yield (profile.lastUsedCeIndex.ToString())
            yield! (profile.segmentTotals |> ArrayProperties.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))
        |]



type sorterSetCeUseProfile = {
    profileSegments: ArrayProperties.segment []
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
    sorterCeUseProfiles : sorterCeUseProfile []
}

module SorterSetCeUseProfile =

    let makeSorterSetCeUseProfile
            (segmentCount:int)
            (blockGrowthRate:float)
            (sorterSetEval:sorterSetEval) : sorterSetCeUseProfile  =

        let profileSegments = ArrayProperties.breakIntoExponentialSegments2 
                                    segmentCount 
                                    blockGrowthRate 
                                    (sorterSetEval.CeLength |> int)
        {
            sorterSetCeUseProfile.profileSegments = profileSegments
            sorterSetId = sorterSetEval.SorterSetId
            sorterTestsId = sorterSetEval.SorterTestsId
            sorterCeUseProfiles = [| for se in sorterSetEval.SorterEvals -> 
                                        SorterCeUseProfile.makeSorterCeUseProfile 
                                            profileSegments 
                                            sorterSetEval.SorterSetId 
                                            sorterSetEval.SorterTestsId 
                                            se |]
        }

    let makeReportLines (sortingWidth:int<sortingWidth> option) (sorterModelKey:string) 
                        (sorterSetCeUseProfile: sorterSetCeUseProfile) : string [][] =
        [|
                for profile in sorterSetCeUseProfile.sorterCeUseProfiles do
                    yield SorterCeUseProfile.makeReportLine sortingWidth sorterModelKey profile
        |]
   
