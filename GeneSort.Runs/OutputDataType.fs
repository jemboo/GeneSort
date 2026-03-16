namespace GeneSort.Runs
open FSharp.UMX
open System

type outputDataType =
    | Project
    | RunParameters
    | SorterSet of string  // Made string mandatory; use "" for none.
    | SortableTest of string
    | SortableTestSet of string
    | SortingSet of string
    | SorterModelSetGen of string
    | SortableTestModelSet of string
    | SortableTestModelSetGen of string
    | SorterSetEval of string
    | SorterSetEvalBins of string
    | TextReport of string<textReportName>

module OutputDataType =
    let private appendParam (prefix: string) (param: string) =
        if String.IsNullOrEmpty param then prefix else prefix + "_" + param

    let toFolderName (outputDataType: outputDataType) : string =
        match outputDataType with
        | RunParameters -> "RunParameters"
        | SorterSet s -> appendParam "SorterSet" s
        | SortableTest s -> appendParam "SortableTest" s
        | SortableTestSet s -> appendParam "SortableTestSet" s
        | SortingSet s -> appendParam "SortingSet" s
        | SorterModelSetGen s -> appendParam "SorterModelSetGen" s
        | SortableTestModelSet s -> appendParam "SortableTestModelSet" s
        | SortableTestModelSetGen s -> appendParam "SortableTestModelSetGen" s
        | SorterSetEval s -> appendParam "SorterSetEval" s
        | SorterSetEvalBins s -> appendParam "SorterSetEvalBins" s
        | Project -> "Project"
        | TextReport s -> appendParam "TextReport" %s

    let fromFolderName (description: string) : outputDataType option =
        let parts = description.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let prefix = parts.[0]
        let param = if parts.Length > 1 then String.Join("_", parts.[1..]) else ""
        match prefix with
        | "RunParameters" when param = "" -> Some RunParameters
        | "SorterSet" -> Some (SorterSet param)
        | "SortableTest" -> Some (SortableTest param)
        | "SortableTestSet" -> Some (SortableTestSet param)
        | "SortingSet" -> Some (SortingSet param)
        | "SorterModelSetGen" -> Some (SorterModelSetGen param)
        | "SortableTestModelSet" -> Some (SortableTestModelSet param)
        | "SortableTestModelSetGen" -> Some (SortableTestModelSetGen param)
        | "SorterSetEval" -> Some (SorterSetEval param)
        | "SorterSetEvalBins" -> Some (SorterSetEvalBins param)
        | "Project" when param = "" -> Some Project
        | "TextReport" -> Some (TextReport (param |> UMX.tag<textReportName>))
        | _ -> None

    let extractTextReportNames (outputDataTypes: outputDataType array) : string<textReportName> list =
        outputDataTypes
        |> Array.choose (function TextReport name -> Some name | _ -> None)
        |> Array.toList