namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Dispatch.V1

type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | GenPrefix
    | SummaryReport
    | SnapshotReport


module SorterSgdExecutorType =

    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"
        | SummaryReport -> "SummaryReport"
        | SnapshotReport -> "SnapshotReport"


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SgdExecutor.evaluateEvolutionRun
                    SortableTestMakers.makeStandardTests
                    PoolSetMakers.createSeedSorterPoolSetStandard
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SgdExecutor.evaluateEvolutionRun
                    SortableTestMakers.makeMergeTests
                    PoolSetMakers.createSeedSorterPoolSetMerge
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeSummaryReport
                    host rp allowOverwrite cts progress }


    let prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SgdExecutor.evaluateEvolutionRun
                    SortableTestMakers.makePrefixTests
                    PoolSetMakers.createSeedSorterPoolSetPrefix
                    host rp allowOverwrite cts progress }


    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.GenPrefix -> prefixExecutor
        | sorterSgdExecutorType.SummaryReport -> Reporting.fullReportExecutor
        | sorterSgdExecutorType.SnapshotReport -> Reporting.snapshotReportExecutor
