
namespace GeneSort.Project

open MessagePack


[<MessagePackObject>]
type WorkspaceDto = 
    { 
        [<MessagePack.Key(0)>] Name: string
        [<MessagePack.Key(1)>]  Description: string
        [<MessagePack.Key(2)>] RootDirectory: string
        [<MessagePack.Key(3)>] RunParametersDtos: runParametersDto []
    }


module WorkspaceDto =  

    let toWorkspaceDto (workspace: workspace) : WorkspaceDto =
        { 
          Name = workspace.Name
          Description = workspace.Description
          RootDirectory = workspace.RootDirectory
          RunParametersDtos = workspace.RunParametersArray 
                                |> Array.map(RunParametersDto.toRunParametersDto) 
        }

    let fromWorkspaceDto (dto: WorkspaceDto) : workspace =
        workspace.create
          dto.Name
          dto.Description
          dto.RootDirectory
          (dto.RunParametersDtos |> Array.map(RunParametersDto.fromDto))

