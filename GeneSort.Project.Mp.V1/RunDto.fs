namespace GeneSort.Project.Mp.V1


open FSharp.UMX
open MessagePack
open GeneSort.Project.V1

[<MessagePackObject>]
type parameterSpanDto =
    {
        [<MessagePack.Key(0)>] Key: string
        [<MessagePack.Key(1)>] Values: string []
    }

[<MessagePackObject>]
type runDto =
    {
        [<MessagePack.Key(0)>] DataBaseName: string
        [<MessagePack.Key(1)>] ProjectName: string
        [<MessagePack.Key(2)>] RunName: string
        [<MessagePack.Key(3)>] Description: string
    }

module RunDto =
    let fromDomain (project: run) : runDto =
        {
            DataBaseName = %project.DatabaseName
            ProjectName = %project.ProjectName
            RunName = %project.RunName
            Description = project.Description
        }

    let toDomain (dto: runDto) : run =
        run.create
          (dto.DataBaseName |> UMX.tag<databaseName> )
          (dto.ProjectName |> UMX.tag<projectName> )
          (dto.RunName |> UMX.tag<runName> )
          dto.Description