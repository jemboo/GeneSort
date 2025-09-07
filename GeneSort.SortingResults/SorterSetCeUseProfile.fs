namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.SortingOps


type sorterCeUseProfile = {
    segmentTotals: ArrayProperties.segmentWithPayload<int> []
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
}

module SorterCeUseProfile =

    //all segments will be equal size if blockGrowthRate = 1. As blockGrowthRate gets larger, the last segments get larger.
    let makeProfileSegments 
            (segmentCount:int) 
            (blockGrowthRate:float) 
            (unbrokenLength: int) 
                    : ArrayProperties.segment [] = 
        ArrayProperties.breakIntoExponentialSegments segmentCount blockGrowthRate unbrokenLength


    let makeSorterCeUseProfile 
            (profileSegments:ArrayProperties.segment [])
            (sorterSetId: Guid<sorterSetId>)
            (sorterTestsId: Guid<sortableTestsId>)
            (sorterEval : sorterEval) : sorterCeUseProfile =
        {   
            segmentTotals = ArrayProperties.getSegmentSums sorterEval.CeBlockUsage.UseCounts profileSegments
            sorterId = sorterEval.SorterId
            sorterSetId = sorterSetId
            sorterTestsId = sorterTestsId
        }

    
    let getUsageProfileData (prefix:string) (profile: sorterCeUseProfile) : string =
        sprintf "%s \t%s \t%s \t%s \t%s" 
                prefix
                (%profile.sorterId.ToString()) 
                (%profile.sorterSetId.ToString()) 
                (%profile.sorterTestsId.ToString())
                (profile.segmentTotals |> ArrayProperties.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))




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
            (sorterSetEval:sorterSetEval) =

        let profileSegments = ArrayProperties.breakIntoExponentialSegments segmentCount blockGrowthRate (sorterSetEval.CeLength |> int)

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

    let getUsageProfileHeader (arraySegments: ArrayProperties.segment[]) : string =
        sprintf "SorterId \tSorterSetId \tSorterTestsId \t%s" 
                    (arraySegments |> ArrayProperties.getSegmentReportHeader )

   
