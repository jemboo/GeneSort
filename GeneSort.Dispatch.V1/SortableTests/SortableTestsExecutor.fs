namespace GeneSort.Dispatch.V1.SortableTests

open GeneSort.Dispatch.V1

module SortableTestsExecutor =
    /// Standalone sortable tests have no setup artifacts; generation belongs to execution.
    let getSetupExecutor (_: runHost) =
        { new IRunParamsExecutor with
            member _.Execute _ rp _ _ _ = async { return Ok rp } }
