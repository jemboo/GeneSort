namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs

open GeneSort.Project.V1.Parameters

module MsrsParams =

    let msrsSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.P4bParams.p4bSetupParams
        @ [ParamKeys.simpleSorterModelTypeKey, ["Msrs"]]
