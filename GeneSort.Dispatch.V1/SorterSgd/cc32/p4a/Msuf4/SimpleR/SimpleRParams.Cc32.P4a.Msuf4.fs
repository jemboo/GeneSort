namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.SimpleR

open GeneSort.Project.V1.Parameters

module SimpleRParams =

    let simpleRSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.msuf4SetupParams
        @ [
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let runSetupDefaults =
        [
            ParamKeys.sorterPoolCountKey, ["1"]
            ParamKeys.sorterCountPerPoolKey, ["32"]
            ParamKeys.mutationModKey, [0 .. 3] |> List.map string
        ]

    let defaultSetupParams =
        runSetupDefaults
        @ [
            ParamKeys.aMaxKey, ["3"]
        ]

    let simpleRExecutionParams = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.msuf4ExecutionParams

    let activatorMutationRate = 0.1

    let activationExecutionParams =
        [ParamKeys.activatorMutationRateKey, [string activatorMutationRate]]

    let activationSweepExecutionParams =
        [ParamKeys.activatorMutationRateKey, [0.05; 0.1; 0.2] |> List.map string]
