
namespace GeneSort.Runs

open FSharp.UMX


type outputDataType =
    | Project
    | RunParameters
    | SorterSet of string option
    | SortableTest of string option
    | SortableTestSet of string option
    | SorterModelSet of string option
    | SorterModelSetMaker of string option
    | SortableTestModelSet of string option
    | SortableTestModelSetMaker of string option
    | SorterSetEval of string option
    | SorterSetEvalBins of string option
    | TextReport of string<textReportName>


     
module OutputDataType =
   
    let toFolderName (outputDataType: outputDataType) : string =
        match outputDataType with
        | RunParameters -> "RunParameters"
        | SorterSet None -> "SorterSet"
        | SorterSet (Some s) -> "SorterSet_" + s
        | SortableTest None -> "SortableTestSet"
        | SortableTest (Some s) -> "SortableTestSet_" + s
        | SortableTestSet None -> "SortableTestSet"
        | SortableTestSet (Some s) -> "SortableTestSet_" + s
        | SorterModelSet None -> "SorterModelSet"
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
        | "SortableTest" -> Some (SortableTest param)
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

    let extractTextReportNames (outputDataTypes: outputDataType array) : string<textReportName> list =
        outputDataTypes
        |> Array.choose (function
            | TextReport name -> Some name
            | _ -> None)
        |> Array.toList
