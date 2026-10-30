namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.Simple

open GeneSort.Project.V1.Parameters

module SimpleParams =

    let simpleSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msrs.MsrsParams.msrsSetupParams
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
            ParamKeys.codeModKey, ["NoMods"]
            ParamKeys.mutatorVariantKey, [GeneSort.Model.Sorting.V1.mutatorVariant.V1 |> GeneSort.Model.Sorting.V1.MutatorVariant.toString]
        ]

    // These legacy workflows apply the same defaults during setup and execution.
    let standardSetupDefaults = standardExecutionDefaults
