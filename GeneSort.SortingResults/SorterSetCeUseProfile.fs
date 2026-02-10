namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps


type sorterCeUseProfile = {
    sorterId: Guid<sorterId>
    sorterSetId: Guid<sorterSetId>
    sorterTestsId: Guid<sorterTestId>
    lastUsedCeIndex: int<ceIndex>
    unsortedCount: int<sortableCount>
    ceCount: int<ceLength>
    stageLength: int<stageLength>
    segmentTotals: segmentWithPayload<int> []
}

module SorterCeUseProfile =

    let makeSorterCeUseProfile 
            (profileSegments: segment [])
            (sorterSetId: Guid<sorterSetId>)
            (sorterTestsId: Guid<sorterTestId>)
            (sorterEval : sorterEval) : sorterCeUseProfile =
        {   
            sorterCeUseProfile.segmentTotals = 
                    SegmentWithPayload.getSegmentSums (sorterEval.CeBlockEval.CeUseCounts.ToArray()) profileSegments
            sorterId = sorterEval.SorterId
            sorterSetId = sorterSetId
            lastUsedCeIndex = sorterEval.CeBlockEval.CeUseCounts.LastUsedCeIndex
            unsortedCount = sorterEval.CeBlockEval.UnsortedCount
            ceCount = sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
            stageLength = sorterEval.CeBlockEval.getStageSequence.StageLength
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
    sorterTestsId: Guid<sorterTestId>
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
                                    (sorterSetEval |> SorterSetEval.getCeLength |> int)
                                    lastLength
        {
            sorterSetCeUseProfile.profileSegments = profileSegments
            sorterSetId = sorterSetEval.SorterSetId
            sorterTestsId = sorterSetEval.SorterTestId
            sorterCeUseProfiles = [| for se in sorterSetEval.SorterEvals -> 
                                        SorterCeUseProfile.makeSorterCeUseProfile 
                                            profileSegments 
                                            sorterSetEval.SorterSetId 
                                            sorterSetEval.SorterTestId 
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
   
