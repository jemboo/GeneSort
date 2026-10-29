namespace GeneSort.FileDb.V1

open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1
open GeneSort.Project.V1.Runtime
open System.Threading
open GeneSort.Core
open GeneSort.Eval.V1.Sgd
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Storage

/// Execution takes the selected paths themselves. Parameters are used only for storage validation and saving.
module RunExecutionFiles =
    let private recoverPath _ (storage: runStorage) (path: evoPath, state: evoProgress) = asyncResult {
        let! checkpoint = Utils.loadOutputDataWithSchedules path.CheckpointIntervals
                              OutputData.asSorterPoolSet (geneSortDataType.SorterPoolSet "")
                              path.GenSaveIntervals storage.RunDb (EvoPath.toParameters path state)
        let generation =
            match checkpoint with
            | Some (sorterPoolSet.Standard pool) -> pool.GenerationNumber
            | Some (sorterPoolSet.Soss pool) -> pool.GenerationNumber
            | None -> path.Start.FirstGeneration
        return Evolution (path, state.WithCurrentGeneration(Some generation).WithFinished(Some false))
    }

    let runPathsCancellableAsync token maxParallel projectsFolder configured (paths: (evoPath * evoProgress)[]) progress =
        RunPhaseFiles.runCancellableAsync token maxParallel {
            Name = "Execution"
            ToParameters = fun (path, state) ->
                EvoPath.requireRun configured path
                EvoPath.toParameters path state
            Bind = fun _ storage ->
                let execute = ExecuteFunctionCatalog.bindPath storage
                fun (path, state) overwrite cts sink -> async {
                    let! result = execute path state overwrite cts sink
                    return result |> Result.map (fun updated -> Evolution (path, updated))
                }
            SkipFinished = true
            SaveParameters = true
            RecoverCancelled = Some recoverPath
        } projectsFolder configured paths progress

    /// Saved-row entry point for callers reopening parameter files. Live workflows use runPathsAsync.
    let runSavedCancellableAsync token maxParallel projectsFolder (configured: run) selected progress =
        RunPhaseFiles.runCancellableAsync token maxParallel {
            Name = "Execution"
            ToParameters = id
            Bind = fun configured storage ->
                match ExecuteFunctionCatalog.get configured.ExecuteFunctionName with
                | SimpleExecution bind ->
                    let execute = bind storage
                    fun row overwrite cts sink -> execute row overwrite cts sink |> Async.map (Result.map Parameters)
                | SgdExecution _ ->
                    let execute = ExecuteFunctionCatalog.bindPath storage
                    fun row overwrite cts sink -> async {
                        let path, state = EvoPath.fromParameters configured row
                        let! result = execute path state overwrite cts sink
                        return result |> Result.map (fun updated -> Evolution (path, updated))
                    }
            SkipFinished = true
            SaveParameters = true
            RecoverCancelled = if configured.IsEvolution then
                                   Some (fun configured storage row -> recoverPath configured storage (EvoPath.fromParameters configured row))
                               else None
        } projectsFolder configured selected progress

    let runPathsParallelAsync maxParallel projectsFolder configured paths progress =
        runPathsCancellableAsync CancellationToken.None maxParallel projectsFolder configured paths progress

    let runSavedParallelAsync maxParallel projectsFolder configured selected progress =
        runSavedCancellableAsync CancellationToken.None maxParallel projectsFolder configured selected progress

    let runPathsAsync projectsFolder configured paths progress =
        runPathsParallelAsync 1 projectsFolder configured paths progress

    let runSavedAsync projectsFolder configured selected progress =
        runSavedParallelAsync 1 projectsFolder configured selected progress
