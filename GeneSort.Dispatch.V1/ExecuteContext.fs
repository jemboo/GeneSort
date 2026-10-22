namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1

/// Inputs available to run functions, independent of the host and executor modules.
type executeContext = {
    RunDb: IGeneSortDb
    QueryParamsFromRunParams: runParameters -> geneSortDataType -> queryParams option
    MakeMutator: sorterModelMutatorMaker option
    GenSaveIntervals: genIntervalConfig option
    GenSaveSubIntervals: genIntervalConfig option
}

type executeFunction =
    executeContext -> runParameters -> bool<allowOverwrite> -> CancellationTokenSource ->
        IProgress<string> option -> Async<Result<runParameters, string>>
