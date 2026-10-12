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
        SortableTestsMakerName: string
        SeedPoolSetMakerName: string
        MutatorMakerName: string
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
            SortableTestsMakerName = project.MakerNames |> Option.map (fun names -> names.SortableTests) |> Option.toObj
            SeedPoolSetMakerName = project.MakerNames |> Option.map (fun names -> names.SeedPoolSet) |> Option.toObj
            MutatorMakerName = project.MakerNames |> Option.map (fun names -> names.Mutator) |> Option.toObj
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
            let makerNames =
                match Option.ofObj dto.SortableTestsMakerName, Option.ofObj dto.SeedPoolSetMakerName, Option.ofObj dto.MutatorMakerName with
                | Some tests, Some seed, Some mutator ->
                    { SortableTests = tests; SeedPoolSet = seed; Mutator = mutator }
                | None, None, None ->
                    // Older runs encoded input selection in their project/executor names.
                    let projectParts = (dto.ProjectName |> Option.ofObj |> Option.defaultValue "").Split('.')
                    let runName = dto.RunName |> Option.ofObj |> Option.defaultValue ""
                    if runName.EndsWith("GenPrefix") || runName.EndsWith("GenSoss") then
                        EvolutionMakerNames.prefix
                    elif runName.EndsWith("GenMerge") then
                        EvolutionMakerNames.merge
                    elif runName.EndsWith("GenStandard") then
                        EvolutionMakerNames.standard
                    elif Array.contains "Prefix" projectParts then
                        EvolutionMakerNames.prefix
                    elif Array.contains "Merge" projectParts then
                        EvolutionMakerNames.merge
                    else EvolutionMakerNames.standard
                | _ -> invalidArg "dto" "An SGD run must specify all three evolution maker names, or omit all three for a legacy run."
            SgdRun (
                SgdRun.create
                    makerNames
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
