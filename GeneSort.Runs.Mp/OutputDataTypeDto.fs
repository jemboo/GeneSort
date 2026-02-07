namespace GeneSort.Runs.Mp

open FSharp.UMX
open MessagePack
open GeneSort.Runs

[<MessagePackObject>]
type outputDataTypeDto =
    {
        [<MessagePack.Key(0)>] Tag: string
        [<MessagePack.Key(1)>] Value: string
    }

module OutputDataTypeDto =
    let fromDomain (outputDataType: outputDataType) : outputDataTypeDto =
        match outputDataType with
        | RunParameters -> { Tag = "RunParameters"; Value = "" }
        | SorterSet so -> { Tag = "SorterSet"; Value = so }
        | SortableTest so -> { Tag = "SortableTest"; Value = so }
        | SortableTestSet so -> { Tag = "SortableTestSet"; Value = so }
        | SortingModelSet so -> { Tag = "SorterModelSet"; Value = so }
        | SorterModelSetMaker so -> { Tag = "SorterModelSetMaker"; Value = so }
        | SortableTestModelSet so -> { Tag = "SortableTestModelSet"; Value = so }
        | SortableTestModelSetMaker so -> { Tag = "SortableTestModelSetMaker"; Value = so }
        | SorterSetEval so -> { Tag = "SorterSetEval"; Value = so }
        | SorterSetEvalBins so -> { Tag = "SorterSetEvalBins"; Value = so }
        | Project -> { Tag = "Project"; Value = "" }
        | TextReport trn -> { Tag = "TextReport"; Value = %trn }

    let toDomain (dto: outputDataTypeDto) : outputDataType =
        match dto.Tag with
        | "RunParameters" -> RunParameters
        | "SorterSet" -> SorterSet dto.Value
        | "SortableTest" -> SortableTest dto.Value
        | "SortableTestSet" -> SortableTestSet dto.Value
        | "SorterModelSet" -> SortingModelSet dto.Value
        | "SorterModelSetMaker" -> SorterModelSetMaker dto.Value
        | "SortableTestModelSet" -> SortableTestModelSet dto.Value
        | "SortableTestModelSetMaker" -> SortableTestModelSetMaker dto.Value
        | "SorterSetEval" -> SorterSetEval dto.Value
        | "SorterSetEvalBins" -> SorterSetEvalBins dto.Value
        | "Project" -> Project
        | "TextReport" -> TextReport (dto.Value |> UMX.tag<textReportName>)
        | _ -> failwith (sprintf "%s not handled" dto.Tag)