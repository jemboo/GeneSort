namespace GeneSort.Project.V1.Catalogs.Reports

open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1
open GeneSort.Project.V1.Runtime


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

    let private reports : Map<string, workflowSources -> run -> runStorage -> runFunction> = Map.ofList [
        sorterEvalFull, (fun _ -> SorterEval.Reporting.makeFullReport)
        sorterEvalStageStats, (fun _ -> SorterEval.Reporting.makeStageStatsReport)
        sorterEvalCeBins, (fun _ -> SorterEval.Reporting.makeCeBinSummaryStats)
        sorterMutateStandard, (fun sources -> SorterMutate.Reporting.makeMutantReport (fun rp -> SorterMutate.Reporting.makeStandardMutantDetails (sources.EvolutionModules (setupParameters.create rp.ParamMap)) rp))
        sorterMutateMerge, (fun sources -> SorterMutate.Reporting.makeMutantReport (fun rp -> SorterMutate.Reporting.makeMergeMutantDetails (sources.EvolutionModules (setupParameters.create rp.ParamMap)) rp))
        sorterMutatePrefix, (fun sources -> SorterMutate.Reporting.makeMutantReport (fun rp -> SorterMutate.Reporting.makePrefixMutantDetails (sources.EvolutionModules (setupParameters.create rp.ParamMap)) rp))
        evolutionSummary, (fun _ -> SorterSgd.Reporting.summaryReportExecutor)
        evolutionSnapshot, (fun _ -> SorterSgd.Reporting.snapshotReportExecutor)
        evolutionHistory, (fun _ -> SorterSgd.Reporting.poolHistoryReportExecutor)
        evolutionBins, (fun _ -> SorterSgd.Reporting.poolBinsReportExecutor)
    ]

    let names = reports |> Map.keys |> Seq.toList

    let get name =
        match Map.tryFind name reports with
        | Some report -> report
        | None -> invalidArg (nameof name) (sprintf "No report named '%s' is registered." name)


