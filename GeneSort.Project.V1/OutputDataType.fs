namespace GeneSort.Project.V1

open FSharp.UMX
open System

[<Measure>] type textReportName

type outputDataType =
    | Run of string<runName>
    | RunParameters of string<runName>
    | SorterRunResult of string
    | SorterSet of string
    | SortableTest of string
    | SorterSetEval of string
    | TextReport of string<textReportName>


module OutputDataType =
    let private appendParam (prefix: string) (param: string) =
        if String.IsNullOrEmpty param then prefix else prefix + "_" + param

    let toFolderName (outputDataType: outputDataType) : string =
        match outputDataType with
        | Run s -> "Run"
        | RunParameters s -> appendParam "RunParameters" %s
        | SorterRunResult s -> appendParam "SorterRunResult" %s
        | SorterSet s -> appendParam "SorterSet" s
        | SortableTest s -> appendParam "SortableTest" s
        | SorterSetEval s -> appendParam "SorterSetEval" s
        | TextReport s -> appendParam "Report\\TextReport" %s


    let fromFolderName (description: string) : outputDataType option =
        let parts = description.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let prefix = parts.[0]
        let param = if parts.Length > 1 then String.Join("_", parts.[1..]) else ""
        match prefix with
        | "Run" -> Some (Run (param |> UMX.tag<runName>))
        | "RunParameters" -> Some (RunParameters (param |> UMX.tag<runName>))
        | "SorterRunResult" -> Some (SorterRunResult param)
        | "SorterSet" -> Some (SorterSet param)
        | "SortableTest" -> Some (SortableTest param)
        | "SorterSetEval" -> Some (SorterSetEval param)
        | "TextReport" -> Some (TextReport (param |> UMX.tag<textReportName>))
        | _ -> None


    let extractTextReportNames (outputDataTypes: outputDataType array) : string<textReportName> list =
        outputDataTypes
        |> Array.choose (function TextReport name -> Some name | _ -> None)
        |> Array.toList