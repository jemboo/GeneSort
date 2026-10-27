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

/// Selects setup from the run's execution family; individual factories honor its setup catalog and makers.
module SetupExecutorCatalog =
    let private standard (sources: workflowSources) (configured: run) (storage: runStorage) : runFunction =
        fun rp _ cts progress -> asyncResult {
            let! start = asyncResult {
                match configured.ParameterSource with
                | Some (source, _) ->
                    let db = sources.OpenDatabase source.ProjectName source.DatabaseName source.RunQueryCatalogName
                    let! checkpoint = EvoCheckpoint.load db rp cts.Token progress
                    return! EvoStartImport.importStandardStart configured storage.RunDb rp checkpoint cts.Token
                | None ->
                    let! sgd = match configured with SgdRun sgd -> Ok sgd | _ -> Error "Standard setup requires an SgdRun."
                    let names = SgdRun.makerNames sgd
                    let makeTests = SortableTestsMakerCatalog.get names.SortableTests sources.Evolution
                    let makeSeed = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet sources.Evolution
                    return! EvoStartCreation.createStandardStart configured.ProjectName configured.DatabaseName makeTests makeSeed storage.RunDb rp cts.Token
            }
            return EvoStart.attach start rp
        }

    let private soss (sources: workflowSources) (configured: run) (storage: runStorage) : runFunction =
        fun rp _ cts progress -> asyncResult {
            let! start = asyncResult {
                match configured.ParameterSource with
                | Some (source, _) ->
                    let db = sources.OpenDatabase source.ProjectName source.DatabaseName source.RunQueryCatalogName
                    let! checkpoint = EvoCheckpoint.load db rp cts.Token progress
                    return! EvoStartImport.importSossStart configured storage.RunDb rp checkpoint cts.Token
                | None ->
                    let! sgd = match configured with SgdRun sgd -> Ok sgd | _ -> Error "Soss setup requires an SgdRun."
                    let names = SgdRun.makerNames sgd
                    let makeTests = SortableTestsMakerCatalog.get names.SortableTests sources.Evolution
                    let makeSeed = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet sources.Evolution
                    return! EvoStartCreation.createSossStart configured.ProjectName configured.DatabaseName makeTests makeSeed storage.RunDb rp cts.Token
            }
            return EvoStart.attach start rp
        }

    let private syncRoot = obj ()
    let mutable private builders: Map<string, workflowSources -> run -> runStorage -> runFunction> = Map.ofList [
        ExecuteFunctionNames.sortableTests, (fun _ _ _ rp _ _ _ -> async { return Ok rp })
        ExecuteFunctionNames.sorterEval, SorterEvalSetup.bind
        ExecuteFunctionNames.sorterMutate, SorterMutateSetup.bind
        ExecuteFunctionNames.standard, standard
        ExecuteFunctionNames.soss, soss
    ]

    let register name builder =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Setup executor registration requires an execute function name."
        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A setup executor for '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let get sources (configured: run) storage =
        let name = configured.ExecuteFunctionName
        let builder = lock syncRoot (fun () ->
            Map.tryFind name builders
            |> Option.defaultWith (fun () -> invalidOp (sprintf "No setup executor is registered for execute function '%s'." name)))
        builder sources configured storage
