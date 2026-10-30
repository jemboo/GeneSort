namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.Simple

open GeneSort.Project.V1.Parameters

module SimpleParams =

    let simpleSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi.MssiParams.mssiSetupParams
        @ [
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let standardExecutionDefaults =
        GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.evolutionExecutionDefaults
        @ [
            ParamKeys.generationCurrentKey, ["0"]
            ParamKeys.mutationModKey, ["0"]
            ParamKeys.selectedSorterCountPerPoolKey, ["All"]
        ]

    // These legacy workflows apply the same defaults during setup and execution.
    let standardSetupDefaults = standardExecutionDefaults
