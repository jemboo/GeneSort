namespace GeneSort.Runs.Mp

open FSharp.UMX
open MessagePack
open GeneSort.Runs

[<MessagePackObject>]
type outputDataTypeDto =
    {
        [<MessagePack.Key(0)>] Tag: string
        [<MessagePack.Key(1)>] Value: string option
    }

module OutputDataTypeDto =
    let fromDomain (outputDataType: outputDataType) : outputDataTypeDto =
        match outputDataType with
        | RunParameters -> { Tag = "RunParameters"; Value = None }
        | SorterSet so -> { Tag = "SorterSet"; Value = so }
        | SortableTestSet so -> { Tag = "SortableTestSet"; Value = so }
        | SorterModelSet so -> { Tag = "SorterModelSet"; Value = so }
        | SorterModelSetMaker so -> { Tag = "SorterModelSetMaker"; Value = so }
        | SortableTestModelSet so -> { Tag = "SortableTestModelSet"; Value = so }
        | SortableTestModelSetMaker so -> { Tag = "SortableTestModelSetMaker"; Value = so }
        | SorterSetEval so -> { Tag = "SorterSetEval"; Value = so }
        | SorterSetEvalBins so -> { Tag = "SorterSetEvalBins"; Value = so }
        | Project -> { Tag = "Project"; Value = None }
        | TextReport trn -> { Tag = "TextReport"; Value = Some %trn }

    let toDomain (dto: outputDataTypeDto) : outputDataType =
        match dto.Tag with
        | "RunParameters" ->
            match dto.Value with
            | None -> RunParameters
            | Some _ -> failwith "Invalid value for RunParameters"
        | "SorterSet" -> SorterSet dto.Value
        | "SortableTestSet" -> SortableTestSet dto.Value
        | "SorterModelSet" -> SorterModelSet dto.Value
        | "SorterModelSetMaker" -> SorterModelSetMaker dto.Value
        | "SortableTestModelSet" -> SortableTestModelSet dto.Value
        | "SortableTestModelSetMaker" -> SortableTestModelSetMaker dto.Value
        | "SorterSetEval" -> SorterSetEval dto.Value
        | "SorterSetEvalBins" -> SorterSetEvalBins dto.Value
        | "Project" ->
            match dto.Value with
            | None -> Project
            | Some _ -> failwith "Invalid value for Project"
        | "TextReport" ->
            match dto.Value with
            | Some s -> TextReport (s |> UMX.tag<textReportName>)
            | None -> failwith "Missing value for TextReport"
        | _ -> failwith $"Unknown tag: {dto.Tag}"