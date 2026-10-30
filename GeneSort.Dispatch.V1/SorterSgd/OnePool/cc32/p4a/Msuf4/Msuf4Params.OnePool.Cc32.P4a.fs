namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4

open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a

module Msuf4Params =

    let msuf4SetupParams =
        P4aParams.p4aSetupParams
        @ [ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]]

    // Standard prepares mutation settings during setup; Soss supplies them during execution.
    // Both phases share the same mutation values through the lists below.
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

    let standardSetupDefaults = mutationExecutionDefaults @ [ParamKeys.adjustCeLengthFrequencyKey, ["10"]]

    let msuf4ExecutionParams = GeneSort.Dispatch.V1.SorterSgd.OnePool.OnePoolParams.onePoolExecutionParams

    let adjustmentExecutionParams = [ParamKeys.adjustCeLengthFrequencyKey, ["10"]]
