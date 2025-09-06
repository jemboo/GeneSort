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
            profileSegments = ArrayProperties.getSegmentSums sorterEval.CeBlockUsage.UseCounts profileSegments
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
                (profile.profileSegments |> ArrayProperties.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))




type sorterSetCeUseProfile = {
    profileSegments: ArrayProperties.segmentWithPayload<int> []
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
}

module SorterSetCeUseProfile =


    let makeSorterSetCeUseProfile
            (sorterSetEval:sorterSetEval) =

       // let yab = sorterSetEval.SorterEvals |> Array.map(fun se -> se |> SorterCeUseProfile.makeSorterCeUseProfile sorterSetEval)


        None

    let getUsageProfileHeader (arraySegments: ArrayProperties.segment[]) : string =
        sprintf "SorterId \tSorterSetId \tSorterTestsId \t%s" 
                    (arraySegments |> ArrayProperties.getSegmentReportHeader )

   
