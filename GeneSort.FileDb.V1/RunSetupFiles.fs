namespace GeneSort.FileDb.V1

open GeneSort.Project.V1.Catalogs.Setup

/// Prepare only the selected saved rows, reusing shared setup artifacts.
module RunSetupFiles =
    let runWithSourcesAsync sources projectsFolder configured selected progress =
        RunPhaseFiles.runAsync {
            Name = "Setup"
            Bind = SetupExecutorCatalog.get sources
            SkipFinished = true
            SaveParameters = true
        } projectsFolder configured selected progress

    let runAsync projectsFolder configured selected progress =
        runWithSourcesAsync (WorkflowSourceFiles.create projectsFolder) projectsFolder configured selected progress
