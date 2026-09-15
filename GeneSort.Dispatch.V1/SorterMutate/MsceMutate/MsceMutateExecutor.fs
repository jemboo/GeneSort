namespace GeneSort.Dispatch.V1.SorterMutate.Msce

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.SortingLib.Sorter
open GeneSort.Sorting.Sorter
open GeneSort.Dispatch.V1.SorterMutate


module MsceMutateExecutor =

    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SorterMutateExecutor._evaluateMutants 
                    SorterMutateExecutor.makeMutantSorterModels
                    SortableTestMakers.makeStandardTests
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                SorterMutateExecutor._evaluateMutants 
                    SorterMutateExecutor.makeMutantSorterModels
                    SortableTestMakers.makeMergeTests
                    host rp allowOverwrite cts progress }

    let mergeReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makeMergeMutantDetails
                    host rp allowOverwrite cts progress }

    let standardReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makeStandardMutantDetails
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterMutateExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterMutateExecutorType.GenStandard -> standardExecutor
        | sorterMutateExecutorType.GenMerge -> mergeExecutor
        | sorterMutateExecutorType.MergeReport -> mergeReportExecutor
        | sorterMutateExecutorType.StandardReport -> standardReportExecutor





