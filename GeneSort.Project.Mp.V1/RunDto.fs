namespace GeneSort.Project.Mp.V1

open GeneSort.Project.V1.Catalogs.Evolution
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
        SetupCatalogName: string
        ExecuteFunctionName: string
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
            SetupCatalogName = project.SetupCatalogName
            ExecuteFunctionName = project.ExecuteFunctionName |> Option.toObj
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
        let setupCatalogName = dto.SetupCatalogName
        if System.String.IsNullOrWhiteSpace setupCatalogName then
            invalidArg "dto" "Run data must specify an explicit SetupCatalogName."
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
                    setupCatalogName
                    filterCatalogName
                    enhancerCatalogName
                    allowOverwrite
                |> fun simple ->
                    match Option.ofObj dto.ExecuteFunctionName with
                    | Some name -> SimpleRun.withExecuteFunctionName name simple
                    | None -> simple)
        | saveIntervals, saveSubIntervals ->
            if System.String.IsNullOrWhiteSpace dto.ExecuteFunctionName then
                invalidArg "dto" "An SGD run must specify an explicit ExecuteFunctionName."
            let requireMakerName field value =
                if System.String.IsNullOrWhiteSpace value then
                    invalidArg "dto" (sprintf "An SGD run must specify an explicit %s." field)
                value
            let makerNames =
                evolutionMakerNames.create
                    (requireMakerName "SortableTestsMakerName" dto.SortableTestsMakerName)
                    (requireMakerName "SeedPoolSetMakerName" dto.SeedPoolSetMakerName)
                    (requireMakerName "MutatorMakerName" dto.MutatorMakerName)
            SgdRun (
                SgdRun.create
                    makerNames
                    dto.ExecuteFunctionName
                    databaseName
                    projectName
                    runName
                    dto.Description
                    parameterSpans
                    (saveIntervals |> Option.defaultValue "expInterval100_L50ss")
                    (saveSubIntervals |> Option.defaultValue "summaryInterval_C.1p5C")
                    queryCatalogName
                    setupCatalogName
                    filterCatalogName
                    enhancerCatalogName
                    allowOverwrite
            )
