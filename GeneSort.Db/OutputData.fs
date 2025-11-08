
namespace GeneSort.Db

open FSharp.UMX

open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs.Params
open GeneSort.Runs
open GeneSort.Core


type outputDataType =
    | RunParameters
    | SorterSet of string option
    | SortableTestSet of string option
    | SorterModelSet of string option
    | SorterModelSetMaker of string option
    | SortableTestModelSet of string option
    | SortableTestModelSetMaker of string option
    | SorterSetEval of string option
    | SorterSetEvalBins of string option
    | Project
    | TextReport of string<textReportName>


     
module OutputDataType =
   
    let toFolderName (outputDataType: outputDataType) : string =
        match outputDataType with
        | RunParameters -> "RunParameters"
        | SorterSet None -> "SorterSet"
        | SorterSet (Some s) -> "SorterSet_" + s
        | SortableTestSet None -> "SortableTestSet"
        | SortableTestSet (Some s) -> "SortableTestSet_" + s
        | SorterModelSet (Some s) -> "SorterModelSet_" + s
        | SorterModelSetMaker None -> "SorterModelSetMaker"
        | SorterModelSetMaker (Some s) -> "SorterModelSetMaker_" + s
        | SortableTestModelSet None -> "SortableTestModelSet"
        | SortableTestModelSet (Some s) -> "SortableTestModelSet_" + s
        | SortableTestModelSetMaker None -> "SortableTestModelSetMaker"
        | SortableTestModelSetMaker (Some s) -> "SortableTestModelSetMaker_" + s
        | SorterSetEval None -> "SorterSetEval"
        | SorterSetEval (Some s) -> "SorterSetEval_" + s
        | SorterSetEvalBins None -> "SorterSetEvalBins"
        | SorterSetEvalBins (Some s) -> "SorterSetEvalBins_" + s
        | Project -> "Project"
        | TextReport tr -> "TextReport"


    let fromFolderName (description: string) : outputDataType option =
        let parts = description.Split([|'_'|], 2)
        let prefix = parts.[0].Trim()
        let param = 
            if parts.Length = 2 then 
                let trimmed = parts.[1].Trim()
                if trimmed = "" then None else Some trimmed
            else 
                None
        match prefix with
        | "RunParameters" -> 
            match param with
            | None -> Some RunParameters
            | _ -> None
        | "SorterSet" -> Some (SorterSet param)
        | "SortableTestSet" -> Some (SortableTestSet param)
        | "SorterModelSet" -> Some (SorterModelSet param)
        | "SorterModelSetMaker" -> Some (SorterModelSetMaker param)
        | "SortableTestModelSet" -> Some (SortableTestModelSet param)
        | "SortableTestModelSetMaker" -> Some (SortableTestModelSetMaker param)
        | "SorterSetEval" -> Some (SorterSetEval param)
        | "SorterSetEvalBins" -> Some (SorterSetEvalBins param)
        | "Project" -> 
            match param with
            | None -> Some Project
            | _ -> None
        | "TextReport" -> Some (TextReport %"Unknown")
        | _ -> None



type outputData =
    | RunParameters of runParameters
    | SorterSet of sorterSet
    | SortableTestSet of sortableTestSet
    | SorterModelSet of sorterModelSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    | Project of project
    | TextReport of dataTableFile
