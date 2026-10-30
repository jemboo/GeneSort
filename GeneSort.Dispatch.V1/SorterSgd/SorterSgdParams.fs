namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Parameters

module SorterSgdParams =

    let sorterSgdSetupParams =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"]
        ]

    // Legacy workflows may apply these in setup, execution, or both. Callers retain that choice.
    let evolutionExecutionDefaults =
        [
            ParamKeys.orthoRateKey, ["4.001"]
            ParamKeys.sorterChildCountKey, ["1"]
            ParamKeys.distinctSorterHashesKey, ["true"]
            ParamKeys.prioritizeNewMutantsKey, ["true"]
            ParamKeys.sortedFractionKey, ["0.99"]
        ]

    let evolutionSetupDefaults = evolutionExecutionDefaults

    let allSortersExecutionDefaults =
        evolutionExecutionDefaults
        @ [ParamKeys.selectedSorterCountPerPoolKey, ["All"]]

    let sossSeedExecutionParams = [ParamKeys.seedSossKey, ["1234"]]

    let sossScheduleExecutionParams =
        sossSeedExecutionParams
        @ [
            ParamKeys.partitionCountKey, ["2"]
            ParamKeys.generationsPerSubsetKey, ["10"]
            ParamKeys.generationsPerMergedSetKey, ["20"]
        ]

    // Partition choices remain local to the longer Soss experiments.
    let longSossScheduleExecutionParams =
        sossSeedExecutionParams
        @ [
            ParamKeys.generationsPerSubsetKey, ["100"]
            ParamKeys.generationsPerMergedSetKey, ["100"]
        ]
