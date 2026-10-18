namespace GeneSort.Project.V1

open FSharp.UMX
open System

type geneSortDataMode =
    | Project
    | Setup
    | Results

module GeneSortDataMode =
    let toFolderName = function
        | geneSortDataMode.Project -> "Project"
        | geneSortDataMode.Setup -> "Setup"
        | geneSortDataMode.Results -> "Results"

type geneSortDataType =
    | Run of string<runName>
    | RunParameters of string<runName>
    | SorterPoolSet of string
    | SorterPoolSetSummarySet of string
    | SorterSet of string
    | SorterModelSet of string
    | SortableTests of string
    | SortableTestsWithPrefix of string
    | SorterSetEval of string
    | SorterPoolBinsSetSeries of string
    | SorterPoolSetHistory of string
    | TextReport of string<textReportName>


module GeneSortDataType =
    let private appendParam (prefix: string) (param: string) =
        if String.IsNullOrEmpty param then prefix else prefix + "_" + param

    let toFolderName (gsDataType: geneSortDataType) : string =
        match gsDataType with
        | Run s -> "Run"
        | RunParameters s -> appendParam "RunParameters" %s
        | SorterPoolSet s -> appendParam "SorterPoolSet" %s
        | SorterPoolSetSummarySet s -> appendParam "SorterPoolSetSummarySet" %s
        | SorterSet s -> appendParam "SorterSet" s
        | SorterModelSet s -> appendParam "SorterModelSet" s
        | SortableTests s -> appendParam "SortableTests" s
        | SortableTestsWithPrefix s -> appendParam "SortableTestsWithPrefix" s
        | SorterSetEval s -> appendParam "SorterSetEval" s
        | SorterPoolBinsSetSeries s -> appendParam "SorterPoolBinsSetSeries" s
        | SorterPoolSetHistory s -> appendParam "SorterPoolSetHistoryCollection" s
        | TextReport s -> appendParam "Report\\TextReport" %s


    let fromFolderName (description: string) : geneSortDataType option =
        let parts = description.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let prefix = parts.[0]
        let param = if parts.Length > 1 then String.Join("_", parts.[1..]) else ""
        match prefix with
        | "Run" -> Some (Run (param |> UMX.tag<runName>))
        | "RunParameters" -> Some (RunParameters (param |> UMX.tag<runName>))
        | "SorterPoolSet" -> Some (SorterPoolSet param)
        | "SorterPoolSetSummarySet" -> Some (SorterPoolSetSummarySet param)
        | "SorterSet" -> Some (SorterSet param)
        | "SorterModelSet" -> Some (SorterModelSet param)
        | "SortableTests" -> Some (SortableTests param)
        | "SortableTestsWithPrefix" -> Some (SortableTestsWithPrefix param)
        | "SorterSetEval" -> Some (SorterSetEval param)
        | "SorterPoolBinsSetSeries" -> Some (SorterPoolBinsSetSeries param)
        | "SorterPoolSetHistoryCollection" -> Some (SorterPoolSetHistory param)
        | "TextReport" -> Some (TextReport (param |> UMX.tag<textReportName>))
        | _ -> None


    let extractTextReportNames (gsDataType: geneSortDataType array) : string<textReportName> list =
        gsDataType
        |> Array.choose (function TextReport name -> Some name | _ -> None)
        |> Array.toList
