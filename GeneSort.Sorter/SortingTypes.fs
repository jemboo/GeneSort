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


[<Measure>] type mergeDimension

type mergeFillType =
    | Full
    | VanVoorhis


module CeLength =
    
    let toStageLength (sortingWidth:int<sortingWidth>) (ceLength: int<ceLength>) 
            : int<stageLength> =
        if %ceLength < 1 then
            failwith "ceLength must be at least 1"
        else
            ((%ceLength * 2) / %sortingWidth) |> UMX.tag<stageLength> 

module StageLength =

    let toCeLength (sortingWidth:int<sortingWidth>) (stageLength: int<stageLength>) 
            : int<ceLength> =
        if %stageLength < 1 then
            failwith "StageLength must be at least 1"
        else
            ((%stageLength * %sortingWidth) / 2) |> UMX.tag<ceLength> 

