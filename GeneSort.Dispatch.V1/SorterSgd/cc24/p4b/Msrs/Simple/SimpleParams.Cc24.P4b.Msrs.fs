namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs.Simple

open GeneSort.Project.V1.Parameters

module SimpleParams =

    let simpleSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs.MsrsParams.msrsSetupParams
        @ [
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let standardExecutionDefaults =
        GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.evolutionExecutionDefaults
        @ [
            ParamKeys.generationCurrentKey, ["0"]
            ParamKeys.selectedSorterCountPerPoolKey, ["All"]
        ]

    let orthoParaExecutionDefaults =
        standardExecutionDefaults
        @ [
            ParamKeys.modificationRateKey, ["0.99"]
            ParamKeys.mutationModKey, ["0"]
            ParamKeys.sorterPoolCountKey, ["256"]
            ParamKeys.sorterCountPerPoolKey, ["32"]
            ParamKeys.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string
            ParamKeys.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string
        ]

    // These legacy workflows apply the same defaults during setup and execution.
    let standardSetupDefaults = standardExecutionDefaults
    let orthoParaSetupDefaults = orthoParaExecutionDefaults
