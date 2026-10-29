namespace GeneSort.Dispatch.V1

open GeneSort.FileDb.V1

/// Supplies evolution setup inputs from the existing file-backed libraries.
module WorkflowSources =
    let defaults =
        // Register producer metadata before any command-line setup opens a source.
        RunTypeCatalog.all |> ignore
        WorkflowSourceFiles.create (RunHost.projectsRoot ())
