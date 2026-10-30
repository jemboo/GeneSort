namespace GeneSort.Project.Mp.V1

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution

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
        RunParametersEnhancerName: string
        SetupParametersEnhancerName: string
        AllowOverwrite: bool
        SortableTestsMakerName: string
        SeedPoolSetMakerName: string
        MutatorMakerName: string
        SetupFunctionName: string
        SetupSortableTestsModule: string
        SetupSorterEvalModule: string
        ExecuteFunctionName: string
        ParameterSource: runDto option
        SourceParameterSpans: parameterSpanDto []
        CheckpointIntervalsName: string
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
            CheckpointIntervalsName = project.CheckpointIntervalsName |> Option.toObj
            GenSaveIntervalsName = saveIntervalsName
            GenSaveSubIntervalsName = saveSubIntervalsName
            RunQueryCatalogName = project.RunQueryCatalogName
            SetupQueryCatalogName = project.SetupQueryCatalogName
            SetupParameterSpans =
                project.SetupParameterSpans
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values })
                |> List.toArray
            SetupFunctionName = project.SetupFunctionName |> Option.toObj
            SetupSortableTestsModule = project.SgdSetup |> Option.map (fun setup -> string setup.SortableTestsModule) |> Option.toObj
            SetupSorterEvalModule = project.SgdSetup |> Option.map (fun setup -> string setup.SorterEvalModule) |> Option.toObj
            ExecuteFunctionName = project.ExecuteFunctionName
            ParameterSource = project.ParameterSource |> Option.map (fst >> fromDomain)
            SourceParameterSpans =
                project.ParameterSource |> Option.map snd |> Option.defaultValue []
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values }) |> List.toArray
            RunParametersEnhancerName = project.RunParametersEnhancerName
            SetupParametersEnhancerName = project.SetupParametersEnhancerName
            AllowOverwrite = project.AllowOverwrite
            SortableTestsMakerName = project.MakerNames |> Option.map (fun names -> names.SortableTests) |> Option.toObj
            SeedPoolSetMakerName = project.MakerNames |> Option.map (fun names -> names.SeedPoolSet) |> Option.toObj
            MutatorMakerName = project.MakerNames |> Option.map (fun names -> names.Mutator) |> Option.toObj
            RunParameterSpans =
                project.RunParameterSpans
                |> List.map (fun (key, values) -> { Key = key; Values = List.toArray values })
                |> List.toArray
        }

    let rec private restore isParameterSource (dto: runDto) : run =
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
        let runParametersEnhancerName = dto.RunParametersEnhancerName
        let allowOverwrite = dto.AllowOverwrite
        if System.String.IsNullOrWhiteSpace dto.ExecuteFunctionName then
            invalidArg "dto" "Run data must specify an explicit ExecuteFunctionName."
        let config: sgdBaseConfig = {
            DatabaseName = databaseName
            ProjectName = projectName
            RunName = runName
            Description = dto.Description
            AllowOverwrite = allowOverwrite
            Execution = {
                FunctionName = dto.ExecuteFunctionName
                ParameterSpans = runParameterSpans
                QueryCatalogName = runQueryCatalogName
                ParametersEnhancerName = runParametersEnhancerName
            }
            Setup = {
                FunctionName = dto.SetupFunctionName
                SortableTestsModule = UMX.tag dto.SetupSortableTestsModule
                SorterEvalModule = UMX.tag dto.SetupSorterEvalModule
                ParameterSpans = setupParameterSpans
                QueryCatalogName = dto.SetupQueryCatalogName
                ParametersEnhancerName = dto.SetupParametersEnhancerName
            }
        }
        let configured =
          match Option.ofObj dto.GenSaveIntervalsName, Option.ofObj dto.GenSaveSubIntervalsName with
          | None, None when isParameterSource && not (System.String.IsNullOrWhiteSpace dto.SetupSortableTestsModule) ->
            Run.ofSimple (SimpleRun.createEvolutionSource config)
          | None, None ->
            Run.ofSimple (SimpleRun.create {
                DatabaseName = config.DatabaseName
                ProjectName = config.ProjectName
                RunName = config.RunName
                Description = config.Description
                AllowOverwrite = config.AllowOverwrite
                Execution = config.Execution
                Setup = {
                    FunctionName = dto.SetupFunctionName
                    ParameterSpans = config.Setup.ParameterSpans
                    QueryCatalogName = config.Setup.QueryCatalogName
                    ParametersEnhancerName = config.Setup.ParametersEnhancerName
                }
            })
          | saveIntervals, saveSubIntervals ->
            let requireMakerName field value =
                if System.String.IsNullOrWhiteSpace value then
                    invalidArg "dto" (sprintf "An SGD run must specify an explicit %s." field)
                value
            let makerNames =
                evolutionMakerNames.create {
                    SortableTests = requireMakerName "SortableTestsMakerName" dto.SortableTestsMakerName
                    SeedPoolSet = requireMakerName "SeedPoolSetMakerName" dto.SeedPoolSetMakerName
                    Mutator = requireMakerName "MutatorMakerName" dto.MutatorMakerName
                }
            Run.ofSgd (
                SgdRun.create config {
                    MakerNames = makerNames
                    GenSaveIntervalsName = (saveIntervals |> Option.defaultValue GenIntervalNames.expInterval100_L50ss)
                    GenSaveSubIntervalsName = (saveSubIntervals |> Option.defaultValue GenIntervalNames.summaryInterval_C_1p5C)
                } |> SgdRun.withCheckpointIntervals (Option.ofObj dto.CheckpointIntervalsName)
            )
        match dto.ParameterSource with
        | None -> configured
        | Some source ->
            let spans =
                dto.SourceParameterSpans |> Option.ofObj |> Option.defaultValue [||]
                |> Array.map (fun span -> span.Key, Array.toList span.Values) |> Array.toList
            configured |> Run.withParameterSource (restore true source) spans

    /// Restore explicit setup choices and source databases; no legacy SGD inference.
    let toDomain dto = restore false dto
