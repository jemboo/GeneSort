namespace GeneSort.Project.V1.Catalogs.Setup

open System
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Setup.SorterEval
open GeneSort.Project.V1.Catalogs.Setup.SorterMutate
open GeneSort.Project.V1.Catalogs.Setup.SorterSgd

/// Each setup function name selects its executor directly.
module SetupExecutorCatalog =
    let private standard (sources: workflowSources) (configured: run) (storage: runStorage) : runPhaseFunction =
        fun rp _ cts progress -> asyncResult {
            match configured.ParameterSource with
            | Some (source, _) ->
                let db = sources.OpenDatabase source.ProjectName source.DatabaseName source.RunQueryCatalogName
                let! checkpoint = EvoCheckpoint.load db rp cts.Token progress
                let! (start: evoStart) = EvoStartImport.importStandardStart configured storage.RunDb rp checkpoint cts.Token
                let imported = SetupInputReferences.recordFiles [start.Tests; start.Pool] rp
                let initialized = EvoStart.initializeExecution start imported
                let path, initialProgress = EvoPath.fromStart configured start initialized
                return Evolution (path, initialProgress)
            | None ->
                let! sgd = match configured with EvolutionRun sgd | ContinuationRun sgd -> Ok sgd | _ -> Error "Standard setup requires an SgdRun."
                let names = SgdRun.makerNames sgd
                let libraries = sources.EvolutionModules (setupParameters.create rp.ParamMap)
                let makeTests = SortableTestsMakerCatalog.get names.SortableTests libraries
                let makeSeed = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet libraries
                let! start = EvoStartCreation.createStandardStart configured.ProjectName configured.DatabaseName makeTests makeSeed storage.RunDb rp cts.Token
                let initialized = EvoStart.initializeExecution start rp
                let path, initialProgress = EvoPath.fromStart configured start initialized
                return Evolution (path, initialProgress)
        }

    let private soss (sources: workflowSources) (configured: run) (storage: runStorage) : runPhaseFunction =
        fun rp _ cts progress -> asyncResult {
            match configured.ParameterSource with
            | Some (source, _) ->
                let db = sources.OpenDatabase source.ProjectName source.DatabaseName source.RunQueryCatalogName
                let! checkpoint = EvoCheckpoint.load db rp cts.Token progress
                let! (start: evoStart) = EvoStartImport.importSossStart configured storage.RunDb rp checkpoint cts.Token
                let imported = SetupInputReferences.recordFiles [start.Tests; start.Pool] rp
                let initialized = EvoStart.initializeExecution start imported
                let path, initialProgress = EvoPath.fromStart configured start initialized
                return Evolution (path, initialProgress)
            | None ->
                let! sgd = match configured with EvolutionRun sgd | ContinuationRun sgd -> Ok sgd | _ -> Error "Soss setup requires an SgdRun."
                let names = SgdRun.makerNames sgd
                let libraries = sources.EvolutionModules (setupParameters.create rp.ParamMap)
                let makeTests = SortableTestsMakerCatalog.get names.SortableTests libraries
                let makeSeed = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet libraries
                let! start = EvoStartCreation.createSossStart configured.ProjectName configured.DatabaseName makeTests makeSeed storage.RunDb rp cts.Token
                let initialized = EvoStart.initializeExecution start rp
                let path, initialProgress = EvoPath.fromStart configured start initialized
                return Evolution (path, initialProgress)
        }

    let private simple bind sources configured storage : runPhaseFunction =
        let execute = bind sources configured storage
        fun rp overwrite cts progress -> execute rp overwrite cts progress |> Async.map (Result.map Parameters)

    let private syncRoot = obj ()
    let mutable private builders: Map<string, workflowSources -> run -> runStorage -> runPhaseFunction> = Map.ofList [
        SetupCatalogNames.standard, simple SorterEvalSetup.bind
        SetupCatalogNames.merge, simple SorterEvalSetup.bind
        SetupCatalogNames.prefix, simple SorterEvalSetup.bind
        SetupCatalogNames.mutantStandard, simple SorterMutateSetup.bind
        SetupCatalogNames.mutantMerge, simple SorterMutateSetup.bind
        SetupCatalogNames.mutantPrefix, simple SorterMutateSetup.bind
        SetupCatalogNames.sgdStandard, standard
        SetupCatalogNames.sgdSoss, soss
    ]

    let register name builder =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Setup executor registration requires a setup function name."
        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A setup executor for '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let prepare sources (configured: run) storage =
        if not configured.HasSetup then invalidArg "configured" "A direct run has no Setup phase. Run Execution instead."
        let name = Run.requireSetupFunctionName configured
        let builder = lock syncRoot (fun () ->
            Map.tryFind name builders
            |> Option.defaultWith (fun () -> invalidOp (sprintf "No setup executor is registered for setup function '%s'." name)))
        let execute = builder sources configured storage
        fun (rp: runParameters) overwrite cts progress ->
            execute (rp.WithSetupCompleted(Some true)) overwrite cts progress

    /// Parameter-only adapter for callers that deliberately persist and reopen between phases.
    let get sources configured storage : runFunction =
        let setup = prepare sources configured storage
        fun rp overwrite cts progress -> setup rp overwrite cts progress |> Async.map (Result.map RunPhaseOutput.toParameters)
