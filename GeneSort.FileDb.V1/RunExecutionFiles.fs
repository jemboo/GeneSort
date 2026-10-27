namespace GeneSort.FileDb.V1

open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1
open GeneSort.Project.V1.Runtime

/// Execution takes the selected paths themselves. Parameters are used only for storage validation and saving.
module RunExecutionFiles =
    let runPathsAsync projectsFolder configured (paths: (evoPath * evoProgress)[]) progress =
        RunPhaseFiles.runAsync {
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
        } projectsFolder configured paths progress

    /// Saved-row entry point for callers reopening parameter files. Live workflows use runPathsAsync.
    let runSavedAsync projectsFolder configured selected progress =
        RunPhaseFiles.runAsync {
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
        } projectsFolder configured selected progress
