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
        | SortableTests so -> { Tag = "SortableTests"; Value = so }
        | SortableTestsSet so -> { Tag = "SortableTestsSet"; Value = so }
        | SortingSet so -> { Tag = "SorterModelSet"; Value = so }
        | SorterModelSetGen so -> { Tag = "SorterModelSetGen"; Value = so }
        | SortableTestsModelSet so -> { Tag = "SortableTestsModelSet"; Value = so }
        | SortableTestsModelSetGen so -> { Tag = "SortableTestsModelSetGen"; Value = so }
        | SorterSetEval so -> { Tag = "SorterSetEval"; Value = so }
        | SorterEvalBins so -> { Tag = "SorterSetEvalBins"; Value = so }
        | Project -> { Tag = "Project"; Value = "" }
        | TextReport trn -> { Tag = "TextReport"; Value = %trn }

    let toDomain (dto: outputDataTypeDto) : outputDataType =
        match dto.Tag with
        | "RunParameters" -> RunParameters
        | "SorterSet" -> SorterSet dto.Value
        | "SortableTests" -> SortableTests dto.Value
        | "SortableTestsSet" -> SortableTestsSet dto.Value
        | "SorterModelSet" -> SortingSet dto.Value
        | "SorterModelSetGen" -> SorterModelSetGen dto.Value
        | "SortableTestsModelSet" -> SortableTestsModelSet dto.Value
        | "SortableTestsModelSetGen" -> SortableTestsModelSetGen dto.Value
        | "SorterSetEval" -> SorterSetEval dto.Value
        | "SorterSetEvalBins" -> SorterEvalBins dto.Value
        | "Project" -> Project
        | "TextReport" -> TextReport (dto.Value |> UMX.tag<textReportName>)
        | _ -> failwith (sprintf "%s not handled" dto.Tag)