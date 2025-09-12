
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
    let sorterModelNameKey = "SorterModelName"
    let sortableArrayTypeKey = "SortableArrayType"
    let sortingWidthKey = "SortingWidth"

    
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
        match run.Parameters.TryFind sorterModelNameKey with
        | Some value -> value |> SortableArrayType.fromString
        | None -> failwith "SortableArrayTypeName parameter not found"

    let getSorterModelName (run: Run) : SorterModelKey =
        match run.Parameters.TryFind sorterModelNameKey with
        | Some value -> value |> SorterModelKey.fromString
        | None -> failwith "SorterModel parameter not found"


    let getSortingWidth (run: Run) : int<sortingWidth> =
        match run.Parameters.TryFind sortingWidthKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v |> UMX.tag<sortingWidth>
            | false, _ -> failwith "Invalid SortingWidth value"
        | None -> failwith "SortingWidth parameter not found"


    let getMaxOrbiit (run: Run) : int =
        match run.Parameters.TryFind maxOrbiitKey with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v
            | false, _ -> failwith "Invalid MaxOrbiit value"
        | None -> failwith "MaxOrbiit parameter not found"

