namespace GeneSort.Dispatch.V1

open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SorterMutate

/// Reports are selected independently of a run's ExecuteFunctionName.
module ReportCatalog =
    let private reports = Map.ofList [
        "sorter-eval.full", SorterEvalExecutor.fullReportExecutor
        "sorter-eval.stage-stats", SorterEvalExecutor.stageStatsReportExecutor
        "sorter-eval.ce-bins", SorterEvalExecutor.ceBinsReportExecutor
        "sorter-mutate.standard", SorterMutateExecutor.standardReportExecutor
        "sorter-mutate.merge", SorterMutateExecutor.mergeReportExecutor
        "sorter-mutate.prefix", SorterMutateExecutor.prefixReportExecutor
        "evolution.summary", SorterSgd.Reporting.summaryReportExecutor
        "evolution.snapshot", SorterSgd.Reporting.snapshotReportExecutor
        "evolution.history", SorterSgd.Reporting.poolHistoryReportExecutor
        "evolution.bins", SorterSgd.Reporting.poolBinsReportExecutor
    ]

    let names = reports |> Map.keys |> Seq.toList

    let get name =
        match Map.tryFind name reports with
        | Some report -> report
        | None -> invalidArg (nameof name) (sprintf "No report named '%s' is registered." name)

module ReportOps =
    /// Uses saved run parameters, including completed runs, without updating them.
    let executeReport name minRepl maxRepl allowOverwrite cts progress host maxParallel =
        ProjectOps.executeReportRuns minRepl maxRepl allowOverwrite cts progress host (ReportCatalog.get name) maxParallel
