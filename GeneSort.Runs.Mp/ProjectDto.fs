namespace GeneSort.Runs.Mp


open FSharp.UMX
open MessagePack
open GeneSort.Runs
open GeneSort.Runs.Mp

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
    }

module ProjectDto =
    let fromDomain (project: project) : projectDto =
        {
            ProjectName = %project.ProjectName
            Description = project.Description
            OutputDataTypes = project.OutputDataTypes |> Array.map(OutputDataTypeDto.fromDomain)
        }

    let toDomain (dto: projectDto) : project =
        project.create
          (dto.ProjectName |> UMX.tag<projectName> )
          dto.Description
          (dto.OutputDataTypes |> Array.map(OutputDataTypeDto.toDomain))