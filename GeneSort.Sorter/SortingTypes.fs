namespace GeneSort.Sorter

open FSharp.UMX

[<Measure>] type sortingWidth
[<Measure>] type sorterCount
[<Measure>] type sorterId
[<Measure>] type stageLength
[<Measure>] type ceLength
[<Measure>] type symbolSetSize
[<Measure>] type sortableCount
[<Measure>] type sortableTestsId
[<Measure>] type sortableTestSetId


[<Measure>] type sorterParentId
[<Measure>] type sorterSetId




[<Measure>] type sorterSetConcatMapId
[<Measure>] type sorterSetParentMapId
[<Measure>] type sorterSetMutatorId
[<Measure>] type sorterSpeedBinSetId
[<Measure>] type stageWindowSize
[<Measure>] type switchFrequency



module CeLength =
    
    let toStageCount (sortingWidth:int<sortingWidth>) (ceLength: int<ceLength>) 
            : int<stageLength> =
        if %ceLength < 1 then
            failwith "ceLength must be at least 1"
        else
            ((%ceLength * 2) / %sortingWidth) |> UMX.tag<stageLength> 

module StageCount =

    let toCeLength (sortingWidth:int<sortingWidth>) (stageCount: int<stageLength>) 
            : int<ceLength> =
        if %stageCount < 1 then
            failwith "StageCount must be at least 1"
        else
            ((%stageCount * %sortingWidth) / 2) |> UMX.tag<ceLength> 

