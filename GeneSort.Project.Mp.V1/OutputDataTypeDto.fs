namespace GeneSort.Project.Mp.V1

open FSharp.UMX
open MessagePack
open GeneSort.Project.V1

[<MessagePackObject>]
type outputDataTypeDto =
    {
        [<MessagePack.Key(0)>] Tag: string
        [<MessagePack.Key(1)>] Value: string
    }

module OutputDataTypeDto =

    let fromDomain (outputDataType: outputDataType) : outputDataTypeDto =
        match outputDataType with
        | RunParameters so -> { Tag = "RunParameters"; Value = %so }
        | SorterSet so -> { Tag = "SorterSet"; Value = so }
        | SortableTest so -> { Tag = "SortableTest"; Value = so }
        | SortableTestSet so -> { Tag = "SortableTestSet"; Value = so }
        | SortingSet so -> { Tag = "SorterModelSet"; Value = so }
        | SorterModelSetGen so -> { Tag = "SorterModelSetGen"; Value = so }
        | SortableTestModelSet so -> { Tag = "SortableTestModelSet"; Value = so }
        | SortableTestModelSetGen so -> { Tag = "SortableTestModelSetGen"; Value = so }
        | SorterSetEval so -> { Tag = "SorterSetEval"; Value = so }
        | SorterEvalBins so -> { Tag = "SorterSetEvalBins"; Value = so }
        | Run so -> { Tag = "Run"; Value = %so }
        | TextReport trn -> { Tag = "TextReport"; Value = %trn }
        | _ -> failwith (sprintf "%A not handled" outputDataType)


    let toDomain (dto: outputDataTypeDto) : outputDataType =
        match dto.Tag with
        | "RunParameters" -> RunParameters (dto.Value |> UMX.tag<runName>)
        | "SorterSet" -> SorterSet dto.Value
        | "SortableTest" -> SortableTest dto.Value
        | "SortableTestSet" -> SortableTestSet dto.Value
        | "SorterModelSet" -> SortingSet dto.Value
        | "SorterModelSetGen" -> SorterModelSetGen dto.Value
        | "SortableTestModelSet" -> SortableTestModelSet dto.Value
        | "SortableTestModelSetGen" -> SortableTestModelSetGen dto.Value
        | "SorterSetEval" -> SorterSetEval dto.Value
        | "SorterSetEvalBins" -> SorterEvalBins dto.Value
        | "Run" -> Run (dto.Value |> UMX.tag<runName>)
        | "TextReport" -> TextReport (dto.Value |> UMX.tag<textReportName>)
        | _ -> failwith (sprintf "%s not handled" dto.Tag)