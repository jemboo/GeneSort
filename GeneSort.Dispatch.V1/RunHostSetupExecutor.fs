namespace GeneSort.Dispatch.V1

open GeneSort.Project.V1.Catalogs.Setup

[<AutoOpen>]
module RunHostSetupExecutor =
    type runHost with
        member this.SetupExecutor = SetupExecutorCatalog.get WorkflowSources.defaults this.Run this.Storage
