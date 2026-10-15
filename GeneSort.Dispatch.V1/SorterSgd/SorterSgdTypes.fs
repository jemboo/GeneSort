namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SorterSgd.Standard
open GeneSort.Dispatch.V1.SorterSgd.Soss

type sorterSgdExecutorType = 
    | GenSoss
    | GenStandard
    | SummaryReport
    | HistoryReport
    | SnapshotReport
    | BinsReport


module SorterSgdExecutorType =

    let private executeEvolution evaluateEvolution (host: runHost) rp allowOverwrite cts progress =
        match host with
        | SgdRunHost sgdHost ->
            evaluateEvolution
                (sgdHost.MakeSortableTests EvolutionMakerContext.fileDb)
                (sgdHost.MakeSeedPoolSet EvolutionMakerContext.fileDb)
                sgdHost.MakeMutator
                host.RunDb sgdHost.GenSaveIntervals sgdHost.GenSaveSubIntervals rp allowOverwrite cts progress
        | SimpleRunHost _ ->
            async { return Error "SGD execution requires an SgdRun with generation save interval names." }

    let toString = function
        | GenSoss -> "GenSoss"
        | GenStandard -> "GenStandard"
        | SummaryReport -> "SummaryReport"
        | HistoryReport -> "HistoryReport"
        | SnapshotReport -> "SnapshotReport"
        | BinsReport -> "BinsReport"


    let private sossExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                executeEvolution SgdEx_Soss.evaluateEvolutionRunSoss host rp allowOverwrite cts progress
        }

    let private standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                executeEvolution SgdEx_Standard.evaluateEvolutionRunStandard host rp allowOverwrite cts progress
        }

    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        SorterSgdMakerCatalogs.registerAll ()
        match executorType with
        | sorterSgdExecutorType.GenSoss -> sossExecutor
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.SummaryReport -> Reporting.summaryReportExecutor
        | sorterSgdExecutorType.HistoryReport -> Reporting.poolHistoryReportExecutor
        | sorterSgdExecutorType.SnapshotReport -> Reporting.snapshotReportExecutor
        | sorterSgdExecutorType.BinsReport -> Reporting.poolBinsReportExecutor
