namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Dispatch.V1.SorterSgd.Standard

type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | GenPrefix
    | SummaryReport
    | HistoryReport
    | SnapshotReport
    | BinsReport


module SorterSgdExecutorType =

    let private executeEvolution (host: runHost) rp allowOverwrite cts progress makeTests createSeedPoolSet =
        match host with
        | SgdRunHost sgdHost ->
            SgdEx_Standard.evaluateEvolutionRunStandard
                makeTests
                createSeedPoolSet
                host.RunDb sgdHost.GenSaveIntervals sgdHost.GenSaveSubIntervals rp allowOverwrite cts progress
        | SimpleRunHost _ ->
            async { return Error "SGD execution requires an SgdRun with generation save interval names." }

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
                executeEvolution host rp allowOverwrite cts progress
                    SortableTestsMakers.makeStandardTests
                    PoolSetMakers.createSeedSorterPoolSetStandard
        }

    let private mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                executeEvolution host rp allowOverwrite cts progress
                    SortableTestsMakers.makeMergeTests
                    PoolSetMakers.createSeedSorterPoolSetMerge
        }

    let private prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                executeEvolution host rp allowOverwrite cts progress
                    SortableTestsMakers.makePrefixTests
                    PoolSetMakers.createSeedSorterPoolSetPrefix
        }


    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.GenPrefix -> prefixExecutor
        | sorterSgdExecutorType.SummaryReport -> Reporting.summaryReportExecutor
        | sorterSgdExecutorType.HistoryReport -> Reporting.poolHistoryReportExecutor
        | sorterSgdExecutorType.SnapshotReport -> Reporting.snapshotReportExecutor
        | sorterSgdExecutorType.BinsReport -> Reporting.poolBinsReportExecutor
