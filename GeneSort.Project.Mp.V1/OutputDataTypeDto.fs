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
        | SorterRunResult srr -> { Tag = "SorterRunResult"; Value = srr }
        | SorterSet so -> { Tag = "SorterSet"; Value = so }
        | SortableTest so -> { Tag = "SortableTest"; Value = so }
        | SorterSetEval so -> { Tag = "SorterSetEval"; Value = so }
        | Run so -> { Tag = "Run"; Value = %so }
        | TextReport trn -> { Tag = "TextReport"; Value = %trn }
        | _ -> failwith (sprintf "%A not handled" outputDataType)


    let toDomain (dto: outputDataTypeDto) : outputDataType =
        match dto.Tag with
        | "RunParameters" -> RunParameters (dto.Value |> UMX.tag<runName>)
        | "SorterRunResult" -> SorterRunResult dto.Value
        | "SorterSet" -> SorterSet dto.Value
        | "SortableTest" -> SortableTest dto.Value
        | "SorterSetEval" -> SorterSetEval dto.Value
        | "Run" -> Run (dto.Value |> UMX.tag<runName>)
        | "TextReport" -> TextReport (dto.Value |> UMX.tag<textReportName>)
        | _ -> failwith (sprintf "%s not handled" dto.Tag)