namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleS

open GeneSort.Project.V1.Parameters

module SimpleSParams =

    let activationProbability = 0.5

    let simpleSSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.msuf4SetupParams
        @ [ParamKeys.activationProbabilityKey, [string activationProbability]]

    let simpleSExecutionParams = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.msuf4ExecutionParams

    let activatorMutationRate = 0.1

    let activationExecutionParams =
        [ParamKeys.activatorMutationRateKey, [string activatorMutationRate]]
