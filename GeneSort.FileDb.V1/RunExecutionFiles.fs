namespace GeneSort.FileDb.V1

open GeneSort.Project.V1.Catalogs.Execution

/// Execute only the selected saved rows, using prepared inputs and the workflow's checkpoint resume.
module RunExecutionFiles =
    let runAsync projectsFolder configured selected progress =
        RunPhaseFiles.runAsync {
            Name = "Execution"
            Bind = ExecuteFunctionCatalog.bind
            SkipFinished = true
            SaveParameters = true
        } projectsFolder configured selected progress
