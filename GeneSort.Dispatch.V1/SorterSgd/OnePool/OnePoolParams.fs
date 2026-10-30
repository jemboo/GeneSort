namespace GeneSort.Dispatch.V1.SorterSgd.OnePool

open GeneSort.Project.V1.Parameters

module OnePoolParams =

    let onePoolSetupParams =
        [
            ParamKeys.sorterPoolCountKey, ["1"]
            ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"]
            ParamKeys.sorterCountPerPoolKey, ["32"]
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]


    let onePoolExecutionParams =
        GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.allSortersExecutionDefaults
