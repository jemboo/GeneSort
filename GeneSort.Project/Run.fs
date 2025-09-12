
namespace GeneSort.Project

open System
open GeneSort.Sorter
open GeneSort.Project.Params
open FSharp.UMX


[<Measure>] type cycleNumber

// Run type
type Run = 
    { Index: int
      Cycle: int<cycleNumber>
      mutable Parameters: Map<string, string> }


module Run =

    let getSorterModel (run: Run) : SorterModelKey =
        match run.Parameters.TryFind "SorterModel" with
        | Some value -> value |> SorterModelKey.fromString
        | None -> failwith "SorterModel parameter not found"


    let getSortingWidth (run: Run) : int<sortingWidth> =
        match run.Parameters.TryFind "SortingWidth" with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v |> UMX.tag<sortingWidth>
            | false, _ -> failwith "Invalid SortingWidth value"
        | None -> failwith "SortingWidth parameter not found"


    let getMaxOrbiit (run: Run) : int =
        match run.Parameters.TryFind "MaxOrbiit" with
        | Some value -> 
            match System.Int32.TryParse(value) with
            | true, v -> v
            | false, _ -> failwith "Invalid MaxOrbiit value"
        | None -> failwith "MaxOrbiit parameter not found"

