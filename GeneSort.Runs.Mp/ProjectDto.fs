namespace GeneSort.Runs.Mp


open FSharp.UMX
open MessagePack
open GeneSort.Runs
open GeneSort.Runs.Mp
open GeneSort.Runs.Params

[<MessagePackObject>]
type parameterSpanDto =
    {
        [<MessagePack.Key(0)>] Key: string
        [<MessagePack.Key(1)>] Values: string []
    }

[<MessagePackObject>]
type projectDto =
    {
        [<MessagePack.Key(0)>] ProjectName: string
        [<MessagePack.Key(1)>] Description: string
        [<MessagePack.Key(3)>] OutputDataTypes: outputDataTypeDto []
        [<MessagePack.Key(4)>] RunParametersDtos: runParametersDto []
        [<MessagePack.Key(5)>] ParameterSpans: parameterSpanDto []
        [<MessagePack.Key(6)>] MaxReplicate: int
    }

module ProjectDto =
    let fromDomain (project: project) : projectDto =
        {
          ProjectName = %project.ProjectName
          Description = project.Description
          OutputDataTypes = project.OutputDataTypes |> Array.map(OutputDataTypeDto.fromDomain)
          RunParametersDtos = project.RunParametersArray
                                |> Array.map(RunParametersDto.fromDomain)
          ParameterSpans = project.ParameterSpans
                           |> List.map (fun (k, vs) -> { Key = k; Values = vs |> List.toArray })
                           |> List.toArray
          MaxReplicate = %project.MaxReplicate
        }

    let toDomain (dto: projectDto) : project =
        project.create
          (dto.ProjectName |> UMX.tag<projectName> )
          dto.Description
          (dto.ParameterSpans |> Array.toList |> List.map (fun ps -> ps.Key, ps.Values |> Array.toList))
          (dto.MaxReplicate |> UMX.tag<replNumber>)
          (dto.RunParametersDtos |> Array.map(fun dto -> RunParametersDto.fromDto dto))
          (dto.OutputDataTypes |> Array.map(OutputDataTypeDto.toDomain))
