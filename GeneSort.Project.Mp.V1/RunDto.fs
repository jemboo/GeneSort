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
        RunParameterSpans: parameterSpanDto []
        SetupParameterSpans: parameterSpanDto []
        GenSaveIntervalsName: string
        GenSaveSubIntervalsName: string
        RunQueryCatalogName: string
        SetupQueryCatalogName: string
        RunEnhancerFunctionName: string
        SetupEnhancerFunctionName: string
        AllowOverwrite: bool
        SortableTestsMakerName: string
        SeedPoolSetMakerName: string
        MutatorMakerName: string
        SetupFunctionName: string
        ExecuteFunctionName: string
        ParameterSource: runDto option
        SourceParameterSpans: parameterSpanDto []
    }

module RunDto =

    let rec fromDomain (project: run) : runDto =
        let saveIntervalsName = project.GenSaveIntervalsName |> Option.toObj
        let saveSubIntervalsName = project.GenSaveSubIntervalsName |> Option.toObj
        {
            DataBaseName = %project.DatabaseName
            ProjectName = %project.ProjectName
            RunName = %project.RunName
            Description = project.Description
            GenSaveIntervalsName = saveIntervalsName
            GenSaveSubIntervalsName = saveSubIntervalsName
            RunQueryCatalogName = project.RunQueryCatalogName
            SetupQueryCatalogName = project.SetupQueryCatalogName
            SetupParameterSpans =
                project.SetupParameterSpans
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values })
                |> List.toArray
            SetupFunctionName = project.SetupFunctionName
            ExecuteFunctionName = project.ExecuteFunctionName
            ParameterSource = project.ParameterSource |> Option.map (fst >> fromDomain)
            SourceParameterSpans =
                project.ParameterSource |> Option.map snd |> Option.defaultValue []
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values }) |> List.toArray
            RunEnhancerFunctionName = project.RunEnhancerFunctionName
            SetupEnhancerFunctionName = project.SetupEnhancerFunctionName
            AllowOverwrite = project.AllowOverwrite
            SortableTestsMakerName = project.MakerNames |> Option.map (fun names -> names.SortableTests) |> Option.toObj
            SeedPoolSetMakerName = project.MakerNames |> Option.map (fun names -> names.SeedPoolSet) |> Option.toObj
            MutatorMakerName = project.MakerNames |> Option.map (fun names -> names.Mutator) |> Option.toObj
            RunParameterSpans =
                project.RunParameterSpans
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values })
                |> List.toArray
        }

    let rec toDomain (dto: runDto) : run =
        let databaseName = dto.DataBaseName |> UMX.tag<databaseName>
        let projectName = dto.ProjectName |> UMX.tag<projectName>
        let runName = dto.RunName |> UMX.tag<runName>
        let setupParameterSpans =
            dto.SetupParameterSpans
            |> Array.map (fun span -> span.Key, Array.toList span.Values)
            |> Array.toList
        let runParameterSpans =
            dto.RunParameterSpans
            |> Option.ofObj
            |> Option.defaultValue [||]
            |> Array.map (fun span -> span.Key, (span.Values |> Array.toList))
            |> Array.toList
        let runQueryCatalogName = dto.RunQueryCatalogName
        let runEnhancerFunctionName = dto.RunEnhancerFunctionName
        let allowOverwrite = dto.AllowOverwrite
        let setupFunctionName = dto.SetupFunctionName
        if System.String.IsNullOrWhiteSpace dto.ExecuteFunctionName then
            invalidArg "dto" "Run data must specify an explicit ExecuteFunctionName."
        if System.String.IsNullOrWhiteSpace setupFunctionName then
            invalidArg "dto" "Run data must specify an explicit SetupFunctionName."
        let config: simpleRunConfig = {
            DatabaseName = databaseName
            ProjectName = projectName
            RunName = runName
            Description = dto.Description
            AllowOverwrite = allowOverwrite
            Execution = {
                FunctionName = dto.ExecuteFunctionName
                ParameterSpans = runParameterSpans
                QueryCatalogName = runQueryCatalogName
                EnhancerFunctionName = runEnhancerFunctionName
            }
            Setup = {
                FunctionName = setupFunctionName
                ParameterSpans = setupParameterSpans
                QueryCatalogName = dto.SetupQueryCatalogName
                EnhancerFunctionName = dto.SetupEnhancerFunctionName
            }
        }
        let configured =
          match Option.ofObj dto.GenSaveIntervalsName, Option.ofObj dto.GenSaveSubIntervalsName with
          | None, None ->
            SimpleRun (
                SimpleRun.create config)
          | saveIntervals, saveSubIntervals ->
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
                SgdRun.create config {
                    MakerNames = makerNames
                    GenSaveIntervalsName = (saveIntervals |> Option.defaultValue "expInterval100_L50ss")
                    GenSaveSubIntervalsName = (saveSubIntervals |> Option.defaultValue "summaryInterval_C.1p5C")
                }
            )
        match dto.ParameterSource with
        | None -> configured
        | Some source ->
            let spans =
                dto.SourceParameterSpans |> Option.ofObj |> Option.defaultValue [||]
                |> Array.map (fun span -> span.Key, Array.toList span.Values) |> Array.toList
            configured |> Run.withParameterSource (toDomain source) spans
