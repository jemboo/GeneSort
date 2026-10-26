namespace GeneSort.Project.V1.Runtime

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage

/// A phase ready to execute, with storage and workflow dependencies already bound.
type runFunction =
    runParameters -> bool<allowOverwrite> -> CancellationTokenSource ->
        IProgress<string> option -> Async<Result<runParameters, string>>

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
