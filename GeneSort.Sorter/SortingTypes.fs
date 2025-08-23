namespace GeneSort.Sorter

open FSharp.UMX

[<Measure>] type sortingWidth
[<Measure>] type sorterCount
[<Measure>] type sorterId
[<Measure>] type stageCount
[<Measure>] type ceCount
[<Measure>] type symbolSetSize
[<Measure>] type sortableCount
[<Measure>] type sorterTestId
[<Measure>] type sorterTestSetId


[<Measure>] type sorterParentId
[<Measure>] type sorterSetId




[<Measure>] type sorterSetConcatMapId
[<Measure>] type sorterSetParentMapId
[<Measure>] type sorterSetMutatorId
[<Measure>] type sorterSpeedBinSetId
[<Measure>] type stageWindowSize
[<Measure>] type switchFrequency



module CeCount =
    
    let toStageCount (sortingWidth:int<sortingWidth>) (ceCount: int<ceCount>) 
            : int<stageCount> =
        if %ceCount < 1 then
            failwith "CeCount must be at least 1"
        else
            ((%ceCount * 2) / %sortingWidth) |> UMX.tag<stageCount> 

module StageCount =

    let toCeCount (sortingWidth:int<sortingWidth>) (stageCount: int<stageCount>) 
            : int<ceCount> =
        if %stageCount < 1 then
            failwith "StageCount must be at least 1"
        else
            ((%stageCount * %sortingWidth) / 2) |> UMX.tag<ceCount> 

