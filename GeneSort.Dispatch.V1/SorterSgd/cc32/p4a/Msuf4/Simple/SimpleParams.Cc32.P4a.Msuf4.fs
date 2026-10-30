namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Simple

open GeneSort.Project.V1.Parameters

module SimpleParams =

    let simpleSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.msuf4SetupParams
        @ [
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let standardSetupDefaults =
        GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.evolutionSetupDefaults
        @ [
            ParamKeys.selectedSorterCountPerPoolKey, ["All"]
            ParamKeys.adjustCeLengthFrequencyKey, ["10"]
        ]

    let standardSweepSetupDefaults =
        standardSetupDefaults
        @ [
            ParamKeys.codeModKey, ["NoMods"]
        ]

    let simpleExecutionParams = GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4.Msuf4Params.msuf4ExecutionParams
