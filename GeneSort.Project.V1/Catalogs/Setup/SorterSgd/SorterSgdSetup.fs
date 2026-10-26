namespace GeneSort.Project.V1.Catalogs.Setup.SorterSgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Standard
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Soss

module SorterSgdSetup =

    let private setupExecutor (sources: workflowSources) prepare (configured: run) (storage: runStorage) : runFunction =
        match configured with
        | run.SgdRun sgd ->
            let names = SgdRun.makerNames sgd
            let makeTests = SortableTestsMakerCatalog.get names.SortableTests sources.Evolution
            let makeSeedPool = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet sources.Evolution
            let intervals = RunIntervals.resolve configured.RunName (SgdRun.genSaveIntervalsName sgd)
            let subIntervals = RunIntervals.resolve configured.RunName (SgdRun.genSaveSubIntervalsName sgd)
            fun rp _ cts progress ->
                prepare makeTests makeSeedPool storage.RunDb intervals subIntervals rp cts progress
        | run.SimpleRun _ -> invalidArg "configured" "SGD setup requires an SgdRun with generation save interval names."

    let bind (sources: workflowSources) (configured: run) (storage: runStorage) : runFunction =
        match configured.ExecuteFunctionName with
        | (ExecuteFunctionNames.soss | ExecuteFunctionNames.standard)
            when configured.SetupFunctionName = SetupCatalogNames.evoAppend || configured.SetupFunctionName = SetupCatalogNames.sossAppend ->
            fun rp allowOverwrite cts progress -> asyncResult {
                    let! project = rp.GetSourceProjectName() |> Result.ofOption "Missing SourceProjectName."
                    let! database = rp.GetSourceDbName() |> Result.ofOption "Missing SourceDbName."
                    let source = sources.OpenDatabase project database configured.RunQueryCatalogName
                    return! EvoAppendSetup.prepare source configured storage rp allowOverwrite cts progress
                }
        | ExecuteFunctionNames.standard -> setupExecutor sources SgdEx_Standard.setupEvolutionRunStandard configured storage
        | ExecuteFunctionNames.soss -> setupExecutor sources SgdEx_Soss.setupEvolutionRunSoss configured storage
        | name -> invalidArg (nameof configured) (sprintf "No evolution setup is configured for execute function %A." name)
