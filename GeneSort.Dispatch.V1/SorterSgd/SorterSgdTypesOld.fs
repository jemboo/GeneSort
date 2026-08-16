namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Dispatch.V1

type sorterSgdExecutorTypeOld = 
    | GenStandard
    | GenMerge
    | GenPrefix
    | SummaryReport
    | SnapshotReport


module SorterSgdExecutorTypeOld =

    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"
        | SummaryReport -> "SummaryReport"
        | SnapshotReport -> "SnapshotReport"


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SgdExecutorOld.evaluateEvolutionRunOld
                    SortableTestMakers.makeStandardTests
                    PoolSetMakers.createSeedSorterPoolSetStandard
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SgdExecutorOld.evaluateEvolutionRunOld
                    SortableTestMakers.makeMergeTests
                    PoolSetMakers.createSeedSorterPoolSetMerge
                    host rp allowOverwrite cts progress }

    let prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SgdExecutorOld.evaluateEvolutionRunOld
                    SortableTestMakers.makePrefixTests
                    PoolSetMakers.createSeedSorterPoolSetPrefix
                    host rp allowOverwrite cts progress }


    let getExecutor (executorType: sorterSgdExecutorTypeOld) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorTypeOld.GenStandard -> standardExecutor
        | sorterSgdExecutorTypeOld.GenMerge -> mergeExecutor
        | sorterSgdExecutorTypeOld.GenPrefix -> prefixExecutor
        | sorterSgdExecutorTypeOld.SummaryReport -> Reporting.fullReportExecutor
        | sorterSgdExecutorTypeOld.SnapshotReport -> Reporting.snapshotReportExecutor
