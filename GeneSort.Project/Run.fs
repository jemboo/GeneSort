
namespace GeneSort.Project

open System
open GeneSort.Sorter
open GeneSort.Project.Params
open FSharp.UMX
open GeneSort.Sorter.Sortable


[<Measure>] type cycleNumber

// Run type
type Run = 
    { Index: int
      Cycle: int<cycleNumber>
      mutable Parameters: Map<string, string> }


module Run =

    let cycleKey = "Cycle"
    let maxOrbiitKey = "MaxOrbiit"
    let sorterCountKey = "SorterCount"
    let sorterModelTypeKey = "SorterModelType"
    let sortableArrayTypeKey = "SortableArrayType"
    let sortingWidthKey = "SortingWidth"
    let stageLengthKey = "StageLength"
    let ceLengthKey = "CeLength"

    
    let getCycle (run: Run) : int<cycleNumber> =
        match run.Parameters.TryFind cycleKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v |> UMX.tag<cycleNumber>
            | false, _ -> failwith "Invalid Cycle value"
        | None -> failwith "Cycle parameter not found"
    
    let setCycle (run: Run) (cycle:int<cycleNumber>) : unit =
        run.Parameters <- (run.Parameters |> Map.add cycleKey (%cycle.ToString()))



    let getSortableArrayType (run: Run) : sortableArrayType =
        match run.Parameters.TryFind sorterModelTypeKey with
        | Some value -> value |> SortableArrayType.fromString
        | None -> failwith "SortableArrayTypeName parameter not found"

    let setSortableArrayType  (run: Run) (sortableArrayType:sortableArrayType) : unit =
        run.Parameters <- (run.Parameters |> Map.add cycleKey (sortableArrayType |> SortableArrayType.toString))



    let getSorterModelKey (run: Run) : sorterModelKey =
        match run.Parameters.TryFind sorterModelTypeKey with
        | Some value -> value |> SorterModelKey.fromString
        | None -> failwith "SorterModel parameter not found"

    let setSorterModelKey  (run: Run) (sorterModelKey:sorterModelKey) : unit =
        run.Parameters <- (run.Parameters |> Map.add cycleKey (sorterModelKey |> SorterModelKey.toString))



    let getSortingWidth (run: Run) : int<sortingWidth> =
        match run.Parameters.TryFind sortingWidthKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v |> UMX.tag<sortingWidth>
            | false, _ -> failwith "Invalid SortingWidth value"
        | None -> failwith "SortingWidth parameter not found"

    let setSortingWidth (run: Run) (sortingWidth:int<sortingWidth>) : unit =
            run.Parameters <- (run.Parameters |> Map.add sortingWidthKey (%sortingWidth.ToString()))



    let getMaxOrbiit (run: Run) : int =
        match run.Parameters.TryFind maxOrbiitKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v
            | false, _ -> failwith "Invalid MaxOrbiit value"
        | None -> failwith "MaxOrbiit parameter not found"

    let setMaxOrbiit (run: Run) (maxOrbiit:int) : unit =
            run.Parameters <- (run.Parameters |> Map.add maxOrbiitKey (maxOrbiit.ToString()))


    let getStageLength (run: Run) : int<stageLength> =
        match run.Parameters.TryFind stageLengthKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, stageLength -> stageLength |> UMX.tag<stageLength>
            | false, _ -> failwith "Invalid stageLength value"
        | None -> failwith "SortingWidth parameter not found"

    let setStageLength (run: Run) (stageLength:int<stageLength>) : unit =
            run.Parameters <- (run.Parameters |> Map.add stageLengthKey (%stageLength.ToString()))


    let getCeLength (run: Run) : int<ceLength> =
        match run.Parameters.TryFind ceLengthKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, ceLength -> ceLength |> UMX.tag<ceLength>
            | false, _ -> failwith "Invalid CeLength value"
        | None -> failwith "CeLength parameter not found"

    let setCeLength (run: Run) (ceLength:int<ceLength>) : unit =
            run.Parameters <- (run.Parameters |> Map.add ceLengthKey (%ceLength.ToString()))


    let getSorterCount (run: Run) : int<sorterCount> =
        match run.Parameters.TryFind sorterCountKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, sorterCount -> sorterCount |> UMX.tag<sorterCount>
            | false, _ -> failwith "Invalid SorterCount value"
        | None -> failwith "SorterCount parameter not found"

    let setSorterCount (run: Run) (sorterCount:int<sorterCount>) : unit =
            run.Parameters <- (run.Parameters |> Map.add sorterCountKey (%sorterCount.ToString()))
