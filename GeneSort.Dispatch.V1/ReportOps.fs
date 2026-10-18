namespace GeneSort.Dispatch.V1

open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SorterMutate

/// Reports are selected independently of a run's ExecuteFunctionName.
module ReportCatalog =
    [<Literal>]
    let sorterEvalFull = "sorter-eval.full"
    [<Literal>]
    let sorterEvalStageStats = "sorter-eval.stage-stats"
    [<Literal>]
    let sorterEvalCeBins = "sorter-eval.ce-bins"
    [<Literal>]
    let sorterMutateStandard = "sorter-mutate.standard"
    [<Literal>]
    let sorterMutateMerge = "sorter-mutate.merge"
    [<Literal>]
    let sorterMutatePrefix = "sorter-mutate.prefix"
    [<Literal>]
    let evolutionSummary = "evolution.summary"
    [<Literal>]
    let evolutionSnapshot = "evolution.snapshot"
    [<Literal>]
    let evolutionHistory = "evolution.history"
    [<Literal>]
    let evolutionBins = "evolution.bins"

    let private reports = Map.ofList [
        sorterEvalFull, SorterEvalExecutor.fullReportExecutor
        sorterEvalStageStats, SorterEvalExecutor.stageStatsReportExecutor
        sorterEvalCeBins, SorterEvalExecutor.ceBinsReportExecutor
        sorterMutateStandard, SorterMutateExecutor.standardReportExecutor
        sorterMutateMerge, SorterMutateExecutor.mergeReportExecutor
        sorterMutatePrefix, SorterMutateExecutor.prefixReportExecutor
        evolutionSummary, SorterSgd.Reporting.summaryReportExecutor
        evolutionSnapshot, SorterSgd.Reporting.snapshotReportExecutor
        evolutionHistory, SorterSgd.Reporting.poolHistoryReportExecutor
        evolutionBins, SorterSgd.Reporting.poolBinsReportExecutor
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
