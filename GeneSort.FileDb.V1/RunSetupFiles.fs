namespace GeneSort.FileDb.V1

open GeneSort.Project.V1.Catalogs.Setup

/// Prepare only the selected saved rows, reusing shared setup artifacts.
module RunSetupFiles =
    let runWithSourcesCancellableAsync token maxParallel sources projectsFolder configured selected progress =
        RunPhaseFiles.runCancellableAsync token maxParallel {
            Name = "Setup"
            ToParameters = id
            Bind = SetupExecutorCatalog.prepare sources
            SkipFinished = true
            SaveParameters = true
            RecoverCancelled = None
        } projectsFolder configured selected progress

    let runWithSourcesParallelAsync maxParallel sources projectsFolder configured selected progress =
        runWithSourcesCancellableAsync System.Threading.CancellationToken.None maxParallel sources projectsFolder configured selected progress

    let runCancellableAsync token maxParallel projectsFolder configured selected progress =
        runWithSourcesCancellableAsync token maxParallel (WorkflowSourceFiles.create projectsFolder) projectsFolder configured selected progress

    let runWithSourcesAsync sources projectsFolder configured selected progress =
        runWithSourcesParallelAsync 1 sources projectsFolder configured selected progress

    let runParallelAsync maxParallel projectsFolder configured selected progress =
        runWithSourcesParallelAsync maxParallel (WorkflowSourceFiles.create projectsFolder) projectsFolder configured selected progress

    let runAsync projectsFolder configured selected progress =
        runWithSourcesAsync (WorkflowSourceFiles.create projectsFolder) projectsFolder configured selected progress
