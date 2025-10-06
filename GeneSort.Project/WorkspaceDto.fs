
namespace GeneSort.Project

open MessagePack


[<MessagePackObject>]
type workspaceDto = 
    { 
        [<MessagePack.Key(0)>] Name: string
        [<MessagePack.Key(1)>] Description: string
        [<MessagePack.Key(2)>] RootDirectory: string
        [<MessagePack.Key(3)>] ReportNames: string []
        [<MessagePack.Key(4)>] RunParametersDtos: runParametersDto []
    }


module WorkspaceDto =  

    let toWorkspaceDto (workspace: workspace) : workspaceDto =
        { 
          Name = workspace.Name
          Description = workspace.Description
          RootDirectory = workspace.RootDirectory
          ReportNames = workspace.ReportNames
          RunParametersDtos = workspace.RunParametersArray 
                                |> Array.map(RunParametersDto.toRunParametersDto) 
        }

    let toDomain (dto: workspaceDto) : workspace =
        workspace.create
          dto.Name
          dto.Description
          dto.RootDirectory
          (dto.RunParametersDtos |> Array.map(RunParametersDto.fromDto))
          dto.ReportNames

