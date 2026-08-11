namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4

open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd


module Msuf4SgdExecutor =

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
