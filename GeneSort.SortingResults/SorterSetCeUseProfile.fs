namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.SortingOps


type sorterCeUseProfile = {
    profileSegments: ArrayProperties.segmentWithPayload<int> []
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
}

module SorterCeUseProfile =

    //all segments will be equal size if blockGrowthRate = 1. As blockGrowthRate gets larger, the last segments get larger.
    let makeProfileSegments 
            (segmentCount:int) (blockGrowthRate:float) (ceBlockWithUsage: ceBlockWithUsage) = 
        ceBlockWithUsage.UseCounts |> ArrayProperties.breakIntoExponentialSegments segmentCount blockGrowthRate

    let makeSorterCeUseProfile 
            (profileSegments:ArrayProperties.segment [])
            (sorterSetId: Guid<sorterSetId>)
            (sorterTestsId: Guid<sortableTestsId>)
            (sorterEval : sorterEval) : sorterCeUseProfile =
        {   
            profileSegments = ArrayProperties.getSegmentSums sorterEval.CeBlockUsage.UseCounts profileSegments
            sorterId = sorterEval.SorterId
            sorterSetId = sorterSetId
            sorterTestsId = sorterTestsId
        }

    let getUsageProfileHeader (arraySegments: ArrayProperties.segment[]) : string =
        sprintf "SorterId \tSorterSetId \tSorterTestsId \t%s" 
                    (arraySegments |> ArrayProperties.getSegmentReportHeader )

    
    let getUsageProfileData (profile: sorterCeUseProfile) : string =
        sprintf "%s \t%s \t%s \t%s" 
                (%profile.sorterId.ToString()) 
                (%profile.sorterSetId.ToString()) 
                (%profile.sorterTestsId.ToString())
                (profile.profileSegments |> ArrayProperties.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))



type sorterSetCeUseProfile = {
    profileSegments: ArrayProperties.segmentWithPayload<int> []
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
}



module SorterSetCeUseProfile =

    //all segments will be equal size if blockGrowthRate = 1. As blockGrowthRate gets larger, the last segments get larger.
    let makeProfileSegments 
            (segmentCount:int) (blockGrowthRate:float) (ceBlockWithUsage: ceBlockWithUsage) = 
        ceBlockWithUsage.UseCounts |> ArrayProperties.breakIntoExponentialSegments segmentCount blockGrowthRate

    let makeSorterSetCeUseProfile
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

    
    let getUsageProfileData (profile: sorterCeUseProfile) : string =
        sprintf "%s \t%s \t%s \t%s" 
                (%profile.sorterId.ToString()) 
                (%profile.sorterSetId.ToString()) 
                (%profile.sorterTestsId.ToString())
                (profile.profileSegments |> ArrayProperties.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))

