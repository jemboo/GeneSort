namespace GeneSort.Dispatch.V1

open GeneSort.FileDb.V1

/// Supplies evolution setup inputs from the existing file-backed libraries.
module WorkflowSources =
    let defaults = WorkflowSourceFiles.create (RunHost.projectsRoot ())
