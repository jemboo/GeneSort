namespace GeneSort.Project.Mp.V1

open FSharp.UMX
open GeneSort.Project.V1

type parameterSpanDto =
    {
        Key: string
        Values: string []
    }

type runDto =
    {
        DataBaseName: string
        ProjectName: string
        RunName: string
        Description: string
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