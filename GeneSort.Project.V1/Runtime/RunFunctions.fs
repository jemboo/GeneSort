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

module RunStorage =
    /// Setup reads use the resolved path's fixed inputs. Results queries and file
    /// identities continue to use the existing database implementation unchanged.
    let forPath (path: evoPath) (storage: runStorage) =
        let db = storage.RunDb
        let resolve mode (query: queryParams) =
            if mode = geneSortDataMode.Setup then EvoPath.setupQuery query.OutputDataType path else query
        let planned = { new IGeneSortDb with
            member _.databaseName = db.databaseName
            member _.saveAsync mode query data overwrite = db.saveAsync mode (resolve mode query) data overwrite
            member _.loadAsync mode query = db.loadAsync mode (resolve mode query)
            member _.loadIfFoundAsync mode query = db.loadIfFoundAsync mode (resolve mode query)
            member _.doesOutPutDataExist mode query = db.doesOutPutDataExist mode (resolve mode query)
            member _.getRunParameters name first last cancellation progress = db.getRunParameters name first last cancellation progress
            member _.makeQueryParamsFromRunParams parameters output = db.makeQueryParamsFromRunParams parameters output
        }
        runStorage.create { RunDb = planned; QueryParamsFromRunParams = storage.QueryParamsFromRunParams }

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
    | SgdExecution of (runStorage -> sgdExecution -> runFunction)

type workflowSourcesArgs = {
    OpenDatabase: sourceDatabaseOpener
    Evolution: evolutionSetupSources
}

/// Source access supplied by the application for setup and source-dependent reports.
type workflowSources = private { args: workflowSourcesArgs } with
    member this.OpenDatabase = this.args.OpenDatabase
    member this.Evolution = this.args.Evolution
    static member create (args: workflowSourcesArgs) : workflowSources = { args = args }
