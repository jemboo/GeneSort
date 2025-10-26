
namespace GeneSort.Runs.Mp

open FSharp.UMX
open MessagePack
open GeneSort.Runs
open GeneSort.Runs.Mp
open GeneSort.Runs.Params


[<MessagePackObject>]
type projectDto = 
    { 
        [<MessagePack.Key(0)>] ProjectName: string
        [<MessagePack.Key(1)>] Description: string
        [<MessagePack.Key(3)>] ReportNames: string []
        [<MessagePack.Key(4)>] RunParametersDtos: runParametersDto []
    }


module ProjectDto =  

    let fromDomain (project: project) : projectDto =
        { 
          ProjectName = %project.ProjectName
          Description = project.Description
          ReportNames = project.ReportNames
          RunParametersDtos = project.RunParametersArray 
                                |> Array.map(RunParametersDto.fromDomain) 
        }

    let toDomain (dto: projectDto) : project =
        project.create
          (dto.ProjectName |> UMX.tag<projectName> )
          dto.Description
          (dto.RunParametersDtos |> Array.map(RunParametersDto.fromDto))
          dto.ReportNames

