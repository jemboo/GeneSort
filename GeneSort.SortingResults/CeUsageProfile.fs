namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.SortingOps


type ceUsageProfile = {
    profileSegments: ArrayProperties.segmentWithPayload<int> []
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
}

module CeUsageProfile =

    //all segments will be equal size if blockGrowthRate = 1. As blockGrowthRate gets larger, the last segments get larger.
    let makeProfileSegments 
            (segmentCount:int) (blockGrowthRate:float) (ceBlockWithUsage: ceBlockWithUsage) = 
        ceBlockWithUsage.UseCounts |> ArrayProperties.breakIntoExponentialSegments segmentCount blockGrowthRate

    let makeCeUsageProfile 
            (profileSegments:ArrayProperties.segment [])
            (sorterSetId: Guid<sorterSetId>)
            (sorterTestsId: Guid<sortableTestsId>)
            (sorterEval : sorterEval) =
        {   
            profileSegments = ArrayProperties.getSegmentSums sorterEval.CeBlockUsage.UseCounts profileSegments
            sorterId = sorterEval.SorterId
            sorterSetId = sorterSetId
            sorterTestsId = sorterTestsId
        }

    let getUsageProfileHeader (arraySegments: ArrayProperties.segment[]) : string =
        sprintf "SorterId \tSorterSetId \tSorterTestsId \t%s" 
                    (arraySegments |> ArrayProperties.getSegmentReportHeader )

    
    let getUsageProfileData (profile: ceUsageProfile) : string =
        sprintf "%s \t%s \t%s \t%s" 
                (%profile.sorterId.ToString()) 
                (%profile.sorterSetId.ToString()) 
                (%profile.sorterTestsId.ToString())
                (profile.profileSegments |> ArrayProperties.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))

