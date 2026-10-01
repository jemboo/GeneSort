namespace GeneSort.Project.Mp.V1

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
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
        ParameterSpans: parameterSpanDto []
        GenSaveIntervalsName: string
        GenSaveSubIntervalsName: string
        QueryCatalogName: string
    }

module RunDto =

    let fromDomain (project: run) : runDto =
        {
            DataBaseName = %project.DatabaseName
            ProjectName = %project.ProjectName
            RunName = %project.RunName
            Description = project.Description
            GenSaveIntervalsName = project.GenSaveIntervalsName
            GenSaveSubIntervalsName = project.GenSaveSubIntervalsName
            QueryCatalogName = project.QueryCatalogName
            ParameterSpans =
                project.ParameterSpans
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values })
                |> List.toArray
        }

    let toDomain (dto: runDto) : run =
        run.createWithCatalogAndIntervalNames
          (dto.DataBaseName |> UMX.tag<databaseName> )
          (dto.ProjectName |> UMX.tag<projectName> )
          (dto.RunName |> UMX.tag<runName> )
          dto.Description
          (dto.ParameterSpans
           |> Option.ofObj
           |> Option.defaultValue [||]
           |> Array.map (fun span -> span.Key, (span.Values |> Array.toList))
          |> Array.toList)
          (dto.GenSaveIntervalsName |> Option.ofObj |> Option.defaultValue "expInterval100_L50ss")
          (dto.GenSaveSubIntervalsName |> Option.ofObj |> Option.defaultValue "summaryInterval_C.1p5C")
          (dto.QueryCatalogName |> Option.ofObj |> Option.defaultValue (sprintf "%s.%s" dto.ProjectName dto.DataBaseName))
