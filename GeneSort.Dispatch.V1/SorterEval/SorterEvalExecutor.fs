namespace GeneSort.Dispatch.V1.SorterEval

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils



module SorterEvalExecutor =

    /// Creates the Setup artifacts without evaluating sorters or completing the run.
    /// The host must have its setup context bound before calling this function.
    let _makeSetup
            (host: runHost)
            (rp: runParameters)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let log msg =
            OpsUtils.report progress
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (RunParameters.getIdString rp) msg)
        asyncResult {
            try
                do! checkCancellation cts.Token
                log "Generating and saving evaluation setup..."
                do! host.Setup rp
                do! checkCancellation cts.Token
                log "Setup complete. Ready to run evaluation."
                return rp
            with e ->
                let errorMsg = sprintf "Setup error in %s: %s" (RunParameters.getIdString rp) e.Message
                log errorMsg
                return! Error errorMsg
        } |> Async.map (logResult progress log)

    /// Runs evaluation using previously saved Setup artifacts.
    let _makeSorterEvals (host: runHost) rp allowOverwrite cts progress =
        SorterEvalExecution._makeSorterEvals host.ExecutionContext rp allowOverwrite cts progress

    let private bindSetup (host: runHost) allowOverwrite (cts: CancellationTokenSource) : runHost =
        let checkedSetup (setup: setupFunction) : setupFunction =
            fun rp -> asyncResult {
                do! checkCancellation cts.Token
                return! setup rp
            }
        let context =
            setupContext.create {
                MakeSorterModelSets = Map.ofList [
                    SorterModelSetCatalogNames.uniform, SorterModelSetMakers.makeUniformSorterModelSet host allowOverwrite |> checkedSetup
                ]
                MakeStandardTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.makeStandardTests host allowOverwrite |> checkedSetup
                MakeMergeTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.getMergeTests host allowOverwrite |> checkedSetup
                MakePrefixTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.getPrefixTests host allowOverwrite |> checkedSetup
            }
        host.WithSetupContext(context)


    /// Uses the run's setup catalog for Standard, Merge, or Prefix setup.
    let setupExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let host = bindSetup host allowOverwrite cts
                _makeSetup host rp cts progress }

    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                host.ExecuteFunction host.ExecutionContext rp allowOverwrite cts progress }

    let mergeExecutor = standardExecutor

    let prefixExecutor = standardExecutor


    let stageStatsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeStageStatsReport
                    host rp allowOverwrite cts progress }

    let ceBinsReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeCeBinSummaryStats
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeFullReport
                    host rp allowOverwrite cts progress }
