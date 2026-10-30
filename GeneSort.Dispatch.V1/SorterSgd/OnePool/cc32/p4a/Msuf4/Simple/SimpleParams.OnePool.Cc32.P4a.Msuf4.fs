namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Simple

open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4

module SimpleParams =

    // Simple has no additional setup parameters beyond the Msuf4 defaults.
    let simpleSetupParams = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.msuf4SetupParams

    // The Simple Standard workflow also establishes its evolution defaults during setup.
    let simpleStandardSetupParams =
        simpleSetupParams
        @ Msuf4Params.standardSetupDefaults
        @ [
            ParamKeys.orthoRateKey, ["4.001"]
            ParamKeys.sorterChildCountKey, ["1"]
            ParamKeys.distinctSorterHashesKey, ["true"]
            ParamKeys.prioritizeNewMutantsKey, ["true"]
            ParamKeys.sortedFractionKey, ["0.99"]
            ParamKeys.selectedSorterCountPerPoolKey, ["All"]
        ]

    let simpleExecutionParams = GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a.Msuf4.Msuf4Params.msuf4ExecutionParams

    let controlledActivatorMutationRate = 0.1

    let controlledActivationExecutionParams =
        [ParamKeys.activatorMutationRateKey, [string controlledActivatorMutationRate]]
