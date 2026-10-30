namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Mssi

open GeneSort.Project.V1.Parameters

module MssiParams =

    let mssiSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.P4bParams.p4bSetupParams
        @ [ParamKeys.simpleSorterModelTypeKey, ["Mssi"]]
