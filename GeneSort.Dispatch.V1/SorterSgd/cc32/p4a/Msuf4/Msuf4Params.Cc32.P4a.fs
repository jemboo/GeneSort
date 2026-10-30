namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.Msuf4

open GeneSort.Project.V1.Parameters

module Msuf4Params =

    let msuf4SetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc32.p4a.P4aParams.p4aSetupParams
        @ [ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]]

    // Fixed Msuf4 mutation defaults; sweep runs supply their own rates.
    let sossMutationExecutionParams =
        [
            ParamKeys.codeModKey, ["NoMods"]
            ParamKeys.mutatorVariantKey,
                [GeneSort.Model.Sorting.V1.mutatorVariant.V2 |> GeneSort.Model.Sorting.V1.MutatorVariant.toString]
        ]

    let mutationExecutionDefaults =
        sossMutationExecutionParams
        @ [
            ParamKeys.paraRateKey, [string 1.15]
            ParamKeys.selfSymRateKey, [string 1.75]
            ParamKeys.seedModificationRateKey, [string 0.020]
            ParamKeys.modificationRateKey, [string 0.035]
        ]

    let standardSetupDefaults = mutationExecutionDefaults

    let msuf4ExecutionParams = GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.allSortersExecutionDefaults

    let adjustmentExecutionParams = [ParamKeys.adjustCeLengthFrequencyKey, ["10"]]
