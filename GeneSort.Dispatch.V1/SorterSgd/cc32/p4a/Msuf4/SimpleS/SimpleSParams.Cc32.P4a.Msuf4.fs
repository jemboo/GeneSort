namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleS

open GeneSort.Project.V1.Parameters

module SimpleSParams =

    let simpleSSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.msuf4SetupParams
        @ [
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let defaultSetupParams =
        [
            ParamKeys.activationProbabilityKey, ["0.5"]
            ParamKeys.sorterPoolCountKey, ["8"]
            ParamKeys.sorterCountPerPoolKey, ["32"]
            ParamKeys.mutationModKey, [0 .. 1] |> List.map string
        ]

    let simpleSExecutionParams = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.msuf4ExecutionParams

    let activatorMutationRate = 0.1

    let activationExecutionParams =
        [ParamKeys.activatorMutationRateKey, [string activatorMutationRate]]
