namespace GeneSort.Dispatch.V1

open GeneSort.Project.V1.Catalogs.Reports

module ReportOps =
    /// Uses saved run parameters, including completed runs, without updating them.
    let executeReport name minRepl maxRepl allowOverwrite cts progress (host: runHost) maxParallel =
        let report = ReportCatalog.get name WorkflowSources.defaults host.Run host.Storage
        ProjectOps.executeReportRuns minRepl maxRepl allowOverwrite cts progress host report maxParallel
