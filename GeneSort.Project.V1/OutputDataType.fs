namespace GeneSort.Project.V1

open FSharp.UMX
open System

[<Measure>] type textReportName

type outputDataType =
    | Run of string<runName>
    | RunParameters of string<runName>
    | SorterPoolSet of string
    | SorterPoolSetSummaries of string
    | SorterSet of string
    | SortableTest of string
    | SorterSetEval of string
    | SorterPoolEvalBinsSetCollection of string
    | SorterPoolSetHistory of string
    | TextReport of string<textReportName>


module OutputDataType =
    let private appendParam (prefix: string) (param: string) =
        if String.IsNullOrEmpty param then prefix else prefix + "_" + param

    let toFolderName (outputDataType: outputDataType) : string =
        match outputDataType with
        | Run s -> "Run"
        | RunParameters s -> appendParam "RunParameters" %s
        | SorterPoolSet s -> appendParam "SorterPoolSet" %s
        | SorterPoolSetSummaries s -> appendParam "SorterPoolSetSummaries" %s
        | SorterSet s -> appendParam "SorterSet" s
        | SortableTest s -> appendParam "SortableTest" s
        | SorterSetEval s -> appendParam "SorterSetEval" s
        | SorterPoolEvalBinsSetCollection s -> appendParam "SorterPoolEvalBinsSetCollection" s
        | SorterPoolSetHistory s -> appendParam "SorterPoolSetHistoryCollection" s
        | TextReport s -> appendParam "Report\\TextReport" %s


    let fromFolderName (description: string) : outputDataType option =
        let parts = description.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let prefix = parts.[0]
        let param = if parts.Length > 1 then String.Join("_", parts.[1..]) else ""
        match prefix with
        | "Run" -> Some (Run (param |> UMX.tag<runName>))
        | "RunParameters" -> Some (RunParameters (param |> UMX.tag<runName>))
        | "SorterPoolSet" -> Some (SorterPoolSet param)
        | "SorterPoolSetSummaries" -> Some (SorterPoolSetSummaries param)
        | "SorterSet" -> Some (SorterSet param)
        | "SortableTest" -> Some (SortableTest param)
        | "SorterSetEval" -> Some (SorterSetEval param)
        | "SorterPoolEvalBinsSetCollection" -> Some (SorterPoolEvalBinsSetCollection param)
        | "SorterPoolSetHistoryCollection" -> Some (SorterPoolSetHistory param)
        | "TextReport" -> Some (TextReport (param |> UMX.tag<textReportName>))
        | _ -> None


    let extractTextReportNames (outputDataTypes: outputDataType array) : string<textReportName> list =
        outputDataTypes
        |> Array.choose (function TextReport name -> Some name | _ -> None)
        |> Array.toList