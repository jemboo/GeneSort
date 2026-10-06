namespace GeneSort.Project.Mp.V1

open FSharp.UMX
open GeneSort.Project.V1

type outputDataTypeDto =
    {
        Tag: string
        Value: string
    }

module OutputDataTypeDto =

    let fromDomain (outputDataType: outputDataType) : outputDataTypeDto =
        match outputDataType with
        | RunParameters so -> { Tag = "RunParameters"; Value = %so }
        | SorterPoolSetSummarySet srr -> { Tag = "SorterPoolSetSummarySet"; Value = srr }
        | SorterSet so -> { Tag = "SorterSet"; Value = so }
        | outputDataType.SortableTests so -> { Tag = "SortableTests"; Value = so }
        | SorterSetEval so -> { Tag = "SorterSetEval"; Value = so }
        | SorterPoolBinsSetSeries so -> { Tag = "SorterPoolEvalBinsSet"; Value = so }
        | SorterPoolSetHistory so -> { Tag = "SorterPoolSetHistoryCollection"; Value = so }
        | Run so -> { Tag = "Run"; Value = %so }
        | TextReport trn -> { Tag = "TextReport"; Value = %trn }
        | _ -> failwith (sprintf "%A not handled" outputDataType)


    let toDomain (dto: outputDataTypeDto) : outputDataType =
        match dto.Tag with
        | "RunParameters" -> RunParameters (dto.Value |> UMX.tag<runName>)
        | "SorterPoolSetSummarySet" -> SorterPoolSetSummarySet dto.Value
        | "SorterSet" -> SorterSet dto.Value
        | "SortableTests" -> outputDataType.SortableTests dto.Value
        | "SorterSetEval" -> SorterSetEval dto.Value
        | "SorterPoolEvalBinsSetCollection" -> SorterPoolBinsSetSeries dto.Value
        | "SorterPoolSetHistoryCollection" -> SorterPoolSetHistory dto.Value
        | "Run" -> Run (dto.Value |> UMX.tag<runName>)
        | "TextReport" -> TextReport (dto.Value |> UMX.tag<textReportName>)
        | _ -> failwith (sprintf "%s not handled" dto.Tag)