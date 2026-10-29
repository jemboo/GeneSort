namespace GeneSort.Project.V1.Runtime

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage

/// A phase ready to execute, with storage and workflow dependencies already bound.
type runFunction =
    runParameters -> bool<allowOverwrite> -> CancellationTokenSource ->
        IProgress<string> option -> Async<Result<runParameters, string>>

/// The shared phase runner handles simple runs and evolutionary paths.
/// Evolution carries only its fixed path and separate progress; no duplicate parameter row or start.
type runPhaseOutput =
    | Parameters of runParameters
    | Evolution of evoPath * evoProgress

module RunPhaseOutput =
    /// Keep the existing parameter representation at the persistence boundary.
    let toParameters = function
        | Parameters parameters -> parameters
        | Evolution (path, progress) -> EvoPath.toParameters path progress

type runPhaseFunction =
    runParameters -> bool<allowOverwrite> -> CancellationTokenSource ->
        IProgress<string> option -> Async<Result<runPhaseOutput, string>>

/// Evolution consumes a fixed specification and returns only new progress.
type evoPathFunction =
    evoPath -> evoProgress -> bool<allowOverwrite> -> CancellationTokenSource ->
        IProgress<string> option -> Async<Result<evoProgress, string>>

type runStorageArgs = {
    RunDb: IGeneSortDb
    QueryParamsFromRunParams: runQueryParamsBuilder
}

type runStorage = private { args: runStorageArgs } with
    member this.RunDb = this.args.RunDb
    member this.QueryParamsFromRunParams = this.args.QueryParamsFromRunParams
    static member create (args: runStorageArgs) : runStorage = { args = args }

type sgdExecutionArgs = {
    MakeMutator: sorterModelMutatorMaker
    GenSaveIntervals: genIntervalConfig
    GenSaveSubIntervals: genIntervalConfig
}

/// Evolution dependencies are required when binding an SGD execution function.
type sgdExecution = private { args: sgdExecutionArgs } with
    member this.MakeMutator = this.args.MakeMutator
    member this.GenSaveIntervals = this.args.GenSaveIntervals
    member this.GenSaveSubIntervals = this.args.GenSaveSubIntervals
    static member create (args: sgdExecutionArgs) : sgdExecution = { args = args }

type executionBinding =
    | SimpleExecution of (runStorage -> runFunction)
    | SgdExecution of (runStorage -> sgdExecution -> evoPathFunction)

/// Output address and query contract exposed by a producing run module.
type runModuleSource = {
    ProjectName: string<projectName>
    DatabaseName: string<databaseName>
    ExecutionQueryCatalogName: string
}

/// Hosts register module metadata; shared loaders do not infer workflow catalogs.
module RunModuleCatalog =
    let private syncRoot = obj ()
    let mutable private modules: Map<string, runModuleSource> = Map.empty
    let register name source =
        if String.IsNullOrWhiteSpace name || String.IsNullOrWhiteSpace source.ExecutionQueryCatalogName then
            invalidArg "source" "A module requires a name and an execution query catalog."
        lock syncRoot (fun () ->
            match modules.TryFind name with
            | Some existing when existing <> source -> invalidOp (sprintf "Conflicting metadata for module '%s'." name)
            | _ -> modules <- modules.Add(name, source))
    let resolve name =
        lock syncRoot (fun () -> modules.TryFind name)
        |> Result.ofOption (sprintf "Unknown producing module '%s'. Register its database and execution query catalog before Setup." name)

type workflowSourcesArgs = {
    OpenDatabase: sourceDatabaseOpener
    EvolutionModules: setupParameters -> evolutionSetupSources
    Evolution: string<databaseName> -> string<databaseName> -> evolutionSetupSources
}

/// Source access supplied by the application for setup and source-dependent reports.
type workflowSources = private { args: workflowSourcesArgs } with
    member this.OpenDatabase = this.args.OpenDatabase
    member this.Evolution = this.args.Evolution
    member this.EvolutionModules = this.args.EvolutionModules
    member this.LoadModuleOutput moduleName (parameters: setupParameters) outputType = async {
        match RunModuleCatalog.resolve moduleName with
        | Error error -> return Error error
        | Ok source ->
            try
                let database = this.OpenDatabase source.ProjectName source.DatabaseName source.ExecutionQueryCatalogName
                let row = runParameters.create parameters.ParamMap
                match database.makeQueryParamsFromRunParams row outputType with
                | None -> return Error (sprintf "Module '%s' catalog '%s' cannot query %A with the supplied setup parameters." moduleName source.ExecutionQueryCatalogName outputType)
                | Some query ->
                    let! loaded = database.loadAsync geneSortDataMode.Results query
                    return loaded |> Result.mapError (fun error ->
                        sprintf "Source module %s, catalog %s, query %O: %s. Generate the matching producer output, then rerun Setup." moduleName source.ExecutionQueryCatalogName query error)
            with error -> return Error (sprintf "Cannot load output from module '%s': %s" moduleName error.Message)
    }
    static member create (args: workflowSourcesArgs) : workflowSources = { args = args }
