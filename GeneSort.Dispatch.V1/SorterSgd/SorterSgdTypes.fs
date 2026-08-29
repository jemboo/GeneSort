namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Dispatch.V1
open GeneSort.Db.V1

type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | GenPrefix
    | SummaryReport
    | HistoryReport
    | SnapshotReport
    | BinsReport


module SorterSgdExecutorType =

    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"
        | SummaryReport -> "SummaryReport"
        | HistoryReport -> "HistoryReport"
        | SnapshotReport -> "SnapshotReport"
        | BinsReport -> "BinsReport"


    let private standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let genDb = host.RunDb :?> IGeneSortGenDb
                SgdExecutor.evaluateEvolutionRun
                    SortableTestMakers.makeStandardTests
                    PoolSetMakers.createSeedSorterPoolSetStandard
                    genDb rp allowOverwrite cts progress }

    let private mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let genDb = host.RunDb :?> IGeneSortGenDb
                SgdExecutor.evaluateEvolutionRun
                    SortableTestMakers.makeMergeTests
                    PoolSetMakers.createSeedSorterPoolSetMerge
                    genDb rp allowOverwrite cts progress }

    let private prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let genDb = host.RunDb :?> IGeneSortGenDb
                SgdExecutor.evaluateEvolutionRun
                    SortableTestMakers.makePrefixTests
                    PoolSetMakers.createSeedSorterPoolSetPrefix
                    genDb rp allowOverwrite cts progress }


    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.GenPrefix -> prefixExecutor
        | sorterSgdExecutorType.SummaryReport -> Reporting.summaryReportExecutor
        | sorterSgdExecutorType.HistoryReport -> Reporting.poolHistoryReportExecutor
        | sorterSgdExecutorType.SnapshotReport -> Reporting.snapshotReportExecutor
        | sorterSgdExecutorType.BinsReport -> Reporting.poolBinsReportExecutor
