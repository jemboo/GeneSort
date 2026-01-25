namespace GeneSort.Sorting

open FSharp.UMX

[<Measure>] type sortingWidth
[<Measure>] type sorterCount
[<Measure>] type sorterId
[<Measure>] type stageLength
[<Measure>] type ceLength
[<Measure>] type symbolSetSize
[<Measure>] type sortableCount
[<Measure>] type sorterTestId
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
    | NoFill
    | VanVoorhis


type sortableDataFormat = 
    | BoolArray
    | IntArray
    | PackedIntArray
    | Int8Vector256


module SortableDataFormat =

    let toString (t:sortableDataFormat) : string =
        match t with
        | Int8Vector256 -> "Int8Vector256"
        | IntArray -> "Ints"
        | BoolArray -> "Bools"

    let fromString (s:string) : sortableDataFormat =
        match s with
        | "Ints" -> IntArray
        | "Bools" -> BoolArray
        | "Int8Vector256" -> Int8Vector256
        | _ -> failwithf "Unknown SortableDataFormat: %s" s
    let all () : sortableDataFormat list =
        [ IntArray; BoolArray ]




module SortingWidth =
    let toString (w: int<sortingWidth> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"


module SorterCount =
    let toString (c: int<sorterCount> option) : string =
       match c with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module MergeDimension =
    let toString (d: int<mergeDimension> option) : string =
        match d with
        | Some v -> sprintf "%d" %v
        | None -> "None"



module MergeFillType =

    let toString (t:mergeFillType) : string =
        match t with
        | NoFill -> "NoFill"
        | VanVoorhis -> "VanVoorhis"

    let fromString (s:string) : mergeFillType =
        match s with
        | "NoFill" -> mergeFillType.NoFill
        | "VanVoorhis" -> mergeFillType.VanVoorhis
        | _ -> failwithf "Unknown MergeFillType: %s" s

    let all () : mergeFillType list =
        [ mergeFillType.NoFill; mergeFillType.VanVoorhis ]


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

