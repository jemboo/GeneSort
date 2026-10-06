namespace GeneSort.Runs

open FSharp.UMX
open System

[<Measure>] type textReportName

type outputDataType =
    | MutationSegmentEvalBinsSet of string
    | Project
    | RunParameters
    | SorterSet of string
    | SortableTests of string
    | SortableTestsSet of string
    | SortingSet of string
    | SorterModelSetGen of string
    | SortableTestsModelSet of string
    | SortableTestsModelSetGen of string
    | SorterSetEval of string
    | SorterEvalBins of string
    | TextReport of string<textReportName>

module OutputDataType =
    let private appendParam (prefix: string) (param: string) =
        if String.IsNullOrEmpty param then prefix else prefix + "_" + param

    let toFolderName (outputDataType: outputDataType) : string =
        match outputDataType with
        | MutationSegmentEvalBinsSet s -> appendParam "MutationSegmentEvalBinsSet" s
        | Project -> "Project"
        | RunParameters -> "RunParameters"
        | SorterSet s -> appendParam "SorterSet" s
        | SortableTests s -> appendParam "SortableTests" s
        | SortableTestsSet s -> appendParam "SortableTestsSet" s
        | SortingSet s -> appendParam "SortingSet" s
        | SorterModelSetGen s -> appendParam "SorterModelSetGen" s
        | SortableTestsModelSet s -> appendParam "SortableTestsModelSet" s
        | SortableTestsModelSetGen s -> appendParam "SortableTestsModelSetGen" s
        | SorterSetEval s -> appendParam "SorterSetEval" s
        | SorterEvalBins s -> appendParam "SorterEvalBins" s
        | TextReport s -> appendParam "Report\\TextReport" %s

    let fromFolderName (description: string) : outputDataType option =
        let parts = description.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let prefix = parts.[0]
        let param = if parts.Length > 1 then String.Join("_", parts.[1..]) else ""
        match prefix with
        | "MutationSegmentEvalBinsSet" -> Some (MutationSegmentEvalBinsSet param)
        | "Project" when param = "" -> Some Project
        | "RunParameters" when param = "" -> Some RunParameters
        | "SorterSet" -> Some (SorterSet param)
        | "SortableTests" -> Some (SortableTests param)
        | "SortableTestsSet" -> Some (SortableTestsSet param)
        | "SortingSet" -> Some (SortingSet param)
        | "SorterModelSetGen" -> Some (SorterModelSetGen param)
        | "SortableTestsModelSet" -> Some (SortableTestsModelSet param)
        | "SortableTestsModelSetGen" -> Some (SortableTestsModelSetGen param)
        | "SorterSetEval" -> Some (SorterSetEval param)
        | "SorterEvalBins" -> Some (SorterEvalBins param)
        | "TextReport" -> Some (TextReport (param |> UMX.tag<textReportName>))
        | _ -> None

    let extractTextReportNames (outputDataTypes: outputDataType array) : string<textReportName> list =
        outputDataTypes
        |> Array.choose (function TextReport name -> Some name | _ -> None)
        |> Array.toList