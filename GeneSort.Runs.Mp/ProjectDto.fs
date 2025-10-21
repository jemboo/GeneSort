
namespace GeneSort.Runs.Mp

open MessagePack
open GeneSort.Runs
open GeneSort.Runs.Mp


[<MessagePackObject>]
type projectDto = 
    { 
        [<MessagePack.Key(0)>] Name: string
        [<MessagePack.Key(1)>] Description: string
        [<MessagePack.Key(3)>] ReportNames: string []
        [<MessagePack.Key(4)>] RunParametersDtos: runParametersDto []
    }


module ProjectDto =  

    let fromDomain (project: project) : projectDto =
        { 
          Name = project.ProjectName
          Description = project.Description
          ReportNames = project.ReportNames
          RunParametersDtos = project.RunParametersArray 
                                |> Array.map(RunParametersDto.fromDomain) 
        }

    let toDomain (dto: projectDto) : project =
        project.create
          dto.Name
          dto.Description
          (dto.RunParametersDtos |> Array.map(RunParametersDto.fromDto))
          dto.ReportNames

