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
        [<MessagePack.Key(0)>] QueryName: string
        [<MessagePack.Key(1)>] RunName: string
        [<MessagePack.Key(2)>] Description: string
    }

module RunDto =
    let fromDomain (project: run) : runDto =
        {
            QueryName = %project.QueryName
            RunName = %project.RunName
            Description = project.Description
        }

    let toDomain (dto: runDto) : run =
        run.create
          (dto.QueryName |> UMX.tag<queryName> )
          (dto.RunName |> UMX.tag<runName> )
          dto.Description