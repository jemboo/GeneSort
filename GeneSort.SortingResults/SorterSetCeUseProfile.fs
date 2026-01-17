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
    segmentTotals: segmentWithPayload<int> []
}

module SorterCeUseProfile =

    let makeSorterCeUseProfile 
            (profileSegments: segment [])
            (sorterSetId: Guid<sorterSetId>)
            (sorterTestsId: Guid<sortableTestsId>)
            (sorterEval : sorterEval) : sorterCeUseProfile =
        {   
            sorterCeUseProfile.segmentTotals = 
                    SegmentWithPayload.getSegmentSums (sorterEval.CeBlockUsage.UseCounts.ToArray()) profileSegments
            sorterId = sorterEval.SorterId
            sorterSetId = sorterSetId
            lastUsedCeIndex = sorterEval.getLastUsedCeIndex
            unsortedCount = %sorterEval.UnsortedCount
            ceCount = sorterEval.getUsedCeCount()
            stageLength = sorterEval.getStageLength()
            sorterTestsId = sorterTestsId
        }

    let makeReportLine 
            (id:string)
            (repl:string)
            (sortingWidth:string) 
            (sorterModelKey:string)
            (sortableDataType:string)
            (mergeFillType:string)
            (mergeDimension:string)
            (profile: sorterCeUseProfile) : string[] =
        [|
            yield id
            yield repl
            yield sortingWidth
            yield sorterModelKey
            yield sortableDataType
            yield mergeFillType
            yield mergeDimension
            yield (%profile.sorterId.ToString()) 
            yield (%profile.sorterSetId.ToString()) 
            yield (%profile.sorterTestsId.ToString())
            yield (profile.unsortedCount.ToString())
            yield (profile.ceCount.ToString())
            yield (profile.stageLength.ToString())
            yield (profile.lastUsedCeIndex.ToString())
            yield! (profile.segmentTotals |> SegmentWithPayload.getSegmentPayloadReportData (fun (i:int) -> i.ToString()))
        |]



type sorterSetCeUseProfile = {
    profileSegments: segment []
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sortableTestsId>
    sorterCeUseProfiles : sorterCeUseProfile []
}

module SorterSetCeUseProfile =

    let makeSorterSetCeUseProfile
            (segmentCount:int)
            (blockGrowthRate:float)
            (sorterSetEval:sorterSetEval) : sorterSetCeUseProfile  =

        let lastLength = 1
        let profileSegments = Segment.breakIntoExponentialSegments
                                    segmentCount 
                                    blockGrowthRate 
                                    (sorterSetEval.CeLength |> int)
                                    lastLength
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

    let makeReportLines 
            (id: string)
            (repl:string) 
            (sortingWidth:string) 
            (sorterModelKey:string)
            (sortableDataType:string)
            (mergeFillType:string)
            (mergeDimension:string)
            (sorterSetCeUseProfile: sorterSetCeUseProfile) : string [][] =
        [|
                for profile in sorterSetCeUseProfile.sorterCeUseProfiles do
                    yield SorterCeUseProfile.makeReportLine 
                                                id
                                                repl    
                                                sortingWidth 
                                                sorterModelKey
                                                sortableDataType
                                                mergeFillType
                                                mergeDimension 
                                                profile
        |]
   
