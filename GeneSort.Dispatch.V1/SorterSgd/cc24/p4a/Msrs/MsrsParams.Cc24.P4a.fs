namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p4a.Msrs

open GeneSort.Project.V1.Parameters

module MsrsParams =

    let msrsSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p4a.P4aParams.p4aSetupParams
        @ [ParamKeys.simpleSorterModelTypeKey, ["Msrs"]]
