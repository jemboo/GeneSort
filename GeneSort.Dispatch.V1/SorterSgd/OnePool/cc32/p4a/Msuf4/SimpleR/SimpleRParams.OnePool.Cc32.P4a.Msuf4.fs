namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.SimpleR

open GeneSort.Project.V1.Parameters

module SimpleRParams =

    let aMax = 3

    let simpleRSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.msuf4SetupParams
        @ [ParamKeys.aMaxKey, [string aMax]]

    let simpleRExecutionParams = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.msuf4ExecutionParams

    let activatorMutationRate = 0.1

    let activationExecutionParams =
        [ParamKeys.activatorMutationRateKey, [string activatorMutationRate]]
