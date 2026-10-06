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
        FilterCatalogName: string
        EnhancerCatalogName: string
        AllowOverwrite: bool
    }

module RunDto =

    let fromDomain (project: run) : runDto =
        let saveIntervalsName = project.GenSaveIntervalsName |> Option.toObj
        let saveSubIntervalsName = project.GenSaveSubIntervalsName |> Option.toObj
        {
            DataBaseName = %project.DatabaseName
            ProjectName = %project.ProjectName
            RunName = %project.RunName
            Description = project.Description
            GenSaveIntervalsName = saveIntervalsName
            GenSaveSubIntervalsName = saveSubIntervalsName
            QueryCatalogName = project.QueryCatalogName
            FilterCatalogName = project.FilterCatalogName
            EnhancerCatalogName = project.EnhancerCatalogName
            AllowOverwrite = project.AllowOverwrite
            ParameterSpans =
                project.ParameterSpans
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values })
                |> List.toArray
        }

    let toDomain (dto: runDto) : run =
        let databaseName = dto.DataBaseName |> UMX.tag<databaseName>
        let projectName = dto.ProjectName |> UMX.tag<projectName>
        let runName = dto.RunName |> UMX.tag<runName>
        let parameterSpans =
            dto.ParameterSpans
            |> Option.ofObj
            |> Option.defaultValue [||]
            |> Array.map (fun span -> span.Key, (span.Values |> Array.toList))
            |> Array.toList
        let queryCatalogName =
            dto.QueryCatalogName
            |> Option.ofObj
            |> Option.defaultValue (sprintf "%s.%s" dto.ProjectName dto.DataBaseName)
        let filterCatalogName = dto.FilterCatalogName |> Option.ofObj |> Option.defaultValue "identity"
        let enhancerCatalogName = dto.EnhancerCatalogName |> Option.ofObj |> Option.defaultValue "sortable-test"
        let allowOverwrite = dto.AllowOverwrite
        match Option.ofObj dto.GenSaveIntervalsName, Option.ofObj dto.GenSaveSubIntervalsName with
        | None, None ->
            SimpleRun (
                SimpleRun.create
                    databaseName
                    projectName
                    runName
                    dto.Description
                    parameterSpans
                    queryCatalogName
                    filterCatalogName
                    enhancerCatalogName
                    allowOverwrite)
        | saveIntervals, saveSubIntervals ->
            SgdRun (
                SgdRun.create
                    databaseName
                    projectName
                    runName
                    dto.Description
                    parameterSpans
                    (saveIntervals |> Option.defaultValue "expInterval100_L50ss")
                    (saveSubIntervals |> Option.defaultValue "summaryInterval_C.1p5C")
                    queryCatalogName
                    filterCatalogName
                    enhancerCatalogName
                    allowOverwrite
            )
